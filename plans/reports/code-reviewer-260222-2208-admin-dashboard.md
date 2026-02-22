# Code Review: Admin Dashboard Implementation

**Date:** 2026-02-22
**Reviewer:** code-reviewer agent
**Scope:** 5-phase admin dashboard feature (backend + frontend)
**Files Reviewed:** 35+ files across manga-dotnet and web-manga

---

## Overall Assessment

Solid implementation that follows existing codebase patterns well. Clean Architecture compliance is strong, CQRS/MediatR usage is consistent, and the frontend component structure is well-organized with proper lazy loading. One critical security gap found (IsActive bypass), several medium-priority i18n and UX issues, and minor performance suggestions.

## Scores

| Category       | Score | Notes                                                    |
|---------------|-------|----------------------------------------------------------|
| Security       | 7/10  | Critical: deactivated users can still authenticate       |
| Code Quality   | 8/10  | Consistent patterns, good separation, some i18n gaps     |
| Architecture   | 9/10  | Clean Architecture compliance, proper CQRS, lazy loading |
| Performance    | 8/10  | Sequential COUNT queries acceptable at scale, good UX    |

---

## Critical Issues (Must Fix)

### C1. Deactivated users can still log in and refresh tokens

**Files affected:**
- `D:\projects\manga\manga-dotnet\src\Manga.Application\Auth\Commands\Login\LoginCommandHandler.cs`
- `D:\projects\manga\manga-dotnet\src\Manga.Application\Auth\Commands\RefreshToken\RefreshTokenCommandHandler.cs`

**Problem:** `UpdateUserStatusCommand` sets `User.IsActive = false`, but neither `LoginCommandHandler` nor `RefreshTokenCommandHandler` checks this flag. A deactivated user can log in, refresh tokens, and continue using the platform normally.

**Impact:** The entire user deactivation feature is a no-op. Admin toggles the switch, UI shows "inactive", but the user is unaffected.

**Fix for LoginCommandHandler (after line 25):**
```csharp
if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
    return Result<LoginTokenResult>.Failure("Invalid credentials.");

// ADD THIS CHECK:
if (!user.IsActive)
    return Result<LoginTokenResult>.Failure("Account has been deactivated.");
```

**Fix for RefreshTokenCommandHandler (after line 43, before generating new tokens):**
```csharp
if (storedToken.IsExpired)
    return Result<LoginTokenResult>.Failure("Refresh token expired.");

// ADD THIS CHECK:
if (!storedToken.User.IsActive)
    return Result<LoginTokenResult>.Failure("Account has been deactivated.");
```

**Also consider:** Revoking all refresh tokens for the user when `IsActive` is set to `false` in `UpdateUserStatusCommandHandler`:
```csharp
user.IsActive = request.IsActive;

if (!request.IsActive)
{
    var activeTokens = await db.RefreshTokens
        .Where(t => t.UserId == request.UserId && t.RevokedAt == null)
        .ToListAsync(ct);
    foreach (var token in activeTokens)
        token.RevokedAt = DateTimeOffset.UtcNow;
}
```

### C2. No per-route permission guard on frontend admin pages

**File:** `D:\projects\manga\web-manga\src\components\admin\AdminRoute.tsx`

**Problem:** `AdminRoute` only checks if user has ANY admin role (`ADMIN_ROLES = ['Admin', 'Moderator', 'Uploader']`). An Uploader can navigate directly to `/admin/users` or `/admin/comments` -- the page renders, attempts API calls, gets 403, and shows a generic error.

**Impact:** Not a security vulnerability (backend blocks unauthorized access correctly), but poor UX. Users see broken pages instead of clear "access denied" messages.

**Recommendation:** Either:
1. Add per-route role checks in `AdminRoute` or create separate route guards, OR
2. Show `AdminForbiddenPage` when an API call returns 403 in admin pages

Option 1 example -- add a wrapper component:
```tsx
function AdminPermissionRoute({ requiredRoles, children }: { requiredRoles: string[]; children: React.ReactNode }) {
  const user = useAppSelector(selectCurrentUser);
  const hasRole = user?.roles.some((r) => requiredRoles.includes(r));
  if (!hasRole) return <AdminForbiddenPage />;
  return <>{children}</>;
}
```

---

## High Priority (Should Fix)

### H1. Hardcoded English strings bypass i18n

Multiple admin components contain hardcoded English strings instead of using `useTranslation`:

| File | Hardcoded strings |
|------|-------------------|
| `AdminDeleteConfirmDialog.tsx` | "Cancel", "Delete", "Deleting..." |
| `UserRoleEditDialog.tsx` | "Edit Roles --", "Cancel", "Save", "Saving..." |
| `CommentDetailDialog.tsx` | "Comment by", "Deleted", "Manga:", "Date:", "Close" |
| `AdminForbiddenPage.tsx` | "Access Denied", "Back to Home" |
| `AdminTopBar.tsx` | "Admin" (title) |
| `AdminSidebar.tsx` | "Admin", "A" (branding) |
| `AdminUserListPage.tsx` line 134 | "Loading..." |
| `AdminCommentListPage.tsx` line 104 | "Loading..." |

**Impact:** Vietnamese users see mixed English/Vietnamese UI in admin panel.

**Fix:** Add missing keys to both `admin.json` files and replace hardcoded strings with `t()` calls. Example for `AdminDeleteConfirmDialog`:
```tsx
const { t } = useTranslation('admin');
// ...
<Button onClick={onCancel} disabled={loading}>{t('common.cancel')}</Button>
<Button ...>{loading ? t('common.deleting') : t('common.delete')}</Button>
```

### H2. Inconsistent loading state patterns

**Problem:** `AdminMangaListPage` and `AdminChapterListPage` use proper `Skeleton` rows for loading states, while `AdminUserListPage` and `AdminCommentListPage` use plain text "Loading...".

**Fix:** Replace text-based loading with Skeleton rows in `AdminUserListPage` and `AdminCommentListPage` to match the existing pattern.

### H3. MangaCreatePage/MangaEditPage navigate away from admin context

**Files:**
- `D:\projects\manga\web-manga\src\pages\manga\MangaCreatePage.tsx` (line 19)
- `D:\projects\manga\web-manga\src\pages\manga\MangaEditPage.tsx` (line 34)

**Problem:** After successful save, these pages navigate to `/manga/${id}` (public detail page) instead of back to `/admin/manga`. When used within `/admin/*` routes, this takes the user out of the admin panel unexpectedly.

**Fix:** Use `useLocation()` to detect if inside admin context and navigate accordingly:
```tsx
const location = useLocation();
const isAdmin = location.pathname.startsWith('/admin');
navigate(isAdmin ? '/admin/manga' : `/manga/${id}`);
```

---

## Medium Priority (Nice to Have)

### M1. Sequential database queries in GetAdminStatsQueryHandler

**File:** `D:\projects\manga\manga-dotnet\src\Manga.Application\Admin\Queries\GetAdminStats\GetAdminStatsQueryHandler.cs`

**Problem:** 5 sequential `CountAsync()` calls = 5 round-trips to the database.

**Current impact:** Minimal at current scale. Each COUNT on indexed tables is ~1ms, so total ~5ms.

**Future optimization:** Use `Task.WhenAll()` or a single raw SQL query if this becomes a bottleneck:
```csharp
var totalMangaTask = db.MangaSeries.CountAsync(ct);
var totalChaptersTask = db.Chapters.CountAsync(ct);
var totalUsersTask = db.Users.CountAsync(ct);
var totalCommentsTask = db.Comments.CountAsync(ct);
var newUsersTask = db.Users.CountAsync(u => u.CreatedAt >= cutoff, ct);
await Task.WhenAll(totalMangaTask, totalChaptersTask, totalUsersTask, totalCommentsTask, newUsersTask);
```

**Note:** `Task.WhenAll` with EF Core requires separate `DbContext` instances per query or a single `DbContext` that supports multi-threading (not default). Safest approach is raw SQL or leaving as sequential until perf becomes an issue.

### M2. No admin deactivation self-escalation guard for other admins

**File:** `D:\projects\manga\manga-dotnet\src\Manga.Application\Admin\Commands\UpdateUserStatus\UpdateUserStatusCommandHandler.cs`

**Problem:** An admin can deactivate another admin. Combined with C1, once fixed, this means one admin can lock out another admin. The role handler prevents removing the last Admin role (good) but the status handler doesn't prevent deactivating all but one admin.

**Recommendation:** Add a check similar to the role handler:
```csharp
if (!request.IsActive)
{
    var isTargetAdmin = await db.UserRoleMappings
        .AnyAsync(r => r.UserId == request.UserId && r.Role == UserRole.Admin, ct);
    if (isTargetAdmin)
    {
        var activeAdminCount = await db.Users
            .CountAsync(u => u.IsActive && u.UserRoles.Any(r => r.Role == UserRole.Admin), ct);
        if (activeAdminCount <= 1)
            return Result.Failure("Cannot deactivate the last active admin.");
    }
}
```

### M3. AdminSearchBar potential stale closure with onChange dependency

**File:** `D:\projects\manga\web-manga\src\components\admin\AdminSearchBar.tsx`

**Problem:** The `useEffect` on line 18-20 includes `onChange` in the dependency array. If a consumer forgets to wrap `onChange` in `useCallback`, this fires on every render. Current consumers all use `useCallback` correctly, but future usage could introduce bugs.

**Recommendation:** Document the requirement or use `useRef` for the callback:
```tsx
const onChangeRef = useRef(onChange);
onChangeRef.current = onChange;
useEffect(() => { onChangeRef.current(debouncedInput); }, [debouncedInput]);
```

### M4. Role changes fire multiple sequential API calls

**File:** `D:\projects\manga\web-manga\src\pages\admin\AdminUserListPage.tsx` (line 84)

**Problem:** `Promise.all(changes.map(...))` fires N parallel API calls for N role changes. If a user changes 3 roles, that's 3 API calls.

**Recommendation:** Consider a batch endpoint on the backend (e.g., `PUT /api/admin/users/{id}/roles` accepting an array of role changes) to reduce round-trips.

### M5. IgnoreQueryFilters edge case in ListAdminCommentsQueryHandler

**File:** `D:\projects\manga\manga-dotnet\src\Manga.Application\Admin\Queries\ListAdminComments\ListAdminCommentsQueryHandler.cs`

**Problem:** `IgnoreQueryFilters()` on Comments bypasses the soft-delete filter. If in the future users become soft-deleteable, the `c.User.Username` projection could return null for deleted users, potentially causing runtime errors.

**Recommendation:** Add null-safety in the Select projection:
```csharp
c.User != null ? c.User.Username : "[deleted]",
```

---

## Low Priority (Suggestions)

### L1. AdminSidebar file is 256 lines (exceeds 200-line guideline)

**File:** `D:\projects\manga\web-manga\src\components\admin\AdminSidebar.tsx`

Could extract `NavItemButton` and `SidebarContent` into separate files.

### L2. Hardcoded color values across admin components

Colors like `#3b82f6`, `#111827`, `#94a3b8`, `#f1f5f9` are repeated across all admin files. Consider extracting to a shared `admin-theme-constants.ts` or using MUI theme tokens.

### L3. StatusFilter type safety in AdminMangaListPage

**File:** `D:\projects\manga\web-manga\src\pages\admin\AdminMangaListPage.tsx` (line 112)

The `e.target.value as SeriesStatus | ''` cast is type-unsafe. Consider using a type guard or explicit conversion.

---

## Positive Observations

1. **PasswordHash never exposed** - `ListUsersQueryHandler` uses `.Select()` projection that explicitly maps fields; `PasswordHash` is never included in `AdminUserDto`. Verified.

2. **Permission model is correct** - All admin commands/queries use `[RequirePermission]` attributes. The `AuthorizationBehavior` pipeline behavior correctly enforces these. Admin gets all permissions via `Enum.GetValues<Permission>()`.

3. **Self-action prevention** - Both `UpdateUserRoleCommandHandler` and `UpdateUserStatusCommandHandler` check `request.UserId == callerId` and prevent self-modification.

4. **Last admin protection** - `UpdateUserRoleCommandHandler` prevents removing the last Admin role.

5. **Lazy loading for admin bundle** - All admin routes use `React.lazy()` ensuring the admin code is not loaded for regular users, keeping the main bundle small.

6. **Frontend sidebar role-based visibility** - `adminNavItems` correctly restricts nav items by role, and `AdminRoute` gates the entire admin section.

7. **i18n structure is complete** - Both EN and VI translation files have matching structure with all required keys (though some component strings are hardcoded as noted).

8. **Pagination is consistently clamped** - Both `ListUsersQueryHandler` and `ListAdminCommentsQueryHandler` use `Math.Clamp` for page size with a `MaxPageSize = 100`.

9. **Optimistic UI updates** - `AdminUserListPage` uses optimistic updates for status toggle with proper rollback on failure.

10. **Clean Architecture compliance** - Domain layer has zero external deps, Application layer uses only MediatR/FluentValidation, Infrastructure is properly isolated. New admin feature follows the same pattern as existing CRUD features.

---

## Recommended Action Priority

1. **CRITICAL** -- Fix C1 (IsActive check in auth handlers) immediately
2. **HIGH** -- Fix H1 (hardcoded i18n strings) before production deploy
3. **HIGH** -- Fix H3 (navigate away from admin context) for UX
4. **MEDIUM** -- Address C2 (per-route permission guard) and H2 (loading states)
5. **LOW** -- Track M1-M5 as tech debt for future sprints

---

## Metrics

| Metric | Value |
|--------|-------|
| Backend files reviewed | 14 (.cs) |
| Frontend files reviewed | 21+ (.tsx, .ts, .json) |
| i18n key coverage (EN) | 100% (structure) |
| i18n key coverage (VI) | 100% (structure) |
| Hardcoded string violations | 15+ instances across 6 files |
| Critical security issues | 1 (IsActive auth bypass) |
| PasswordHash exposure risk | None (verified) |
| Clean Architecture violations | None |
| MUI pattern consistency | Good (matches existing codebase) |

---

## Unresolved Questions

1. Should deactivated user sessions be immediately invalidated (revoke all refresh tokens + blacklist current access token), or is blocking new logins sufficient?
2. Is the Uploader role intentionally granted `AdminViewStats`? An Uploader seeing platform-wide counts might not be desired.
3. Should role name strings in the frontend (Reader, Uploader, Moderator, Admin) be i18n-translated, or kept as English identifiers matching the backend enum values?
