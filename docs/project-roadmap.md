# Project Roadmap

## Overview

Manga-dotnet development roadmap spanning 6 phases over ~3-4 months. Each phase builds on the previous; dependencies tracked.

**Current Phase**: Phase 4 File Upload & Media (100% Complete - Feb 16)
**Previous Phases**: Foundation (100% Complete), Domain Modeling (100% Complete), Auth (100% Complete), Manga API (100% Complete)

---

## Phase Breakdown

### Phase 1: Foundation & Scaffolding (Complete)

**Status**: 100% Complete ✓
**Duration**: 1 week
**Completed**: 2026-02-13

**Goals**:
- Establish Clean Architecture layers
- Set up CQRS pattern with MediatR
- Configure PostgreSQL with EF Core
- Implement base exception handling

**Deliverables**:
- [x] Solution structure (4 layers + 3 test projects)
- [x] Domain layer: BaseEntity, AuditableEntity, ValueObject, interfaces
- [x] Application layer: DI, MediatR registration, validation/logging behaviors
- [x] Infrastructure layer: EF Core context, base repository, interceptors
- [x] Api layer: Minimal API, middleware, exception handler, health endpoint
- [x] Test project scaffolds

**Metrics**:
- LOC: 450 (source only)
- Code coverage: N/A (no logic yet)
- Test count: 0

**Next**: → Phase 2: Domain Modeling

---

### Phase 2: Domain Modeling (In Progress)

**Status**: 70% Complete
**Duration**: 2-3 sprints (6-9 weeks)
**Actual Start**: 2026-02-13
**Estimated Completion**: 2026-04-10

**Goals**:
- Define all core business entities
- Model domain value objects
- Create domain events
- Establish repository interfaces
- Achieve 80%+ test coverage for domain logic

**Completed Entities** (16 total):
1. **MangaSeries** — Core manga catalog entity (title, author, chapters, status, badge, rating)
2. **Chapter** — Chapters of manga (chapter number, views, published date)
3. **ChapterPage** — Individual pages within chapters (image storage)
4. **AlternativeTitle** — Manga titles in different languages
5. **Genre** — Manga genres (name, slug, description)
6. **MangaGenre** — Join table linking manga to genres
7. **Comment** — User comments on series/chapters/pages
8. **CommentReaction** — Reactions to comments (like/dislike)
9. **Bookmark** — User bookmarks of series/chapters
10. **ReadingHistory** — User reading progress tracking
11. **User** — Platform users (username, email, password hash, roles)
12. **Person** — Authors and artists (name, biography, image)
13. **Attachment** — File storage (covers, banners, avatars, chapter pages)
14. **ViewStat** — Daily view aggregation (anti-bloat for analytics)
15. **MangaBadge** — Hot/Top/New badges on manga
16. **UserRole** — User permission levels

**Completed Enums** (6 total):
- `SeriesStatus` (Ongoing=0, Completed=1, Hiatus=2)
- `MangaBadge` (Hot=0, Top=1, New=2)
- `ReactionType` (Like=0, Dislike=1)
- `UserRole` (User=0, Moderator=1, Admin=2)
- `AttachmentType` (Cover=0, Banner=1, Avatar=2, ChapterPage=3)
- `ViewTargetType` (Series=0, Chapter=1)

**Remaining Tasks**:

**Key Entities to Model**:

#### User Aggregate
```
User (AuditableEntity)
├─ Id: Guid
├─ Name: string
├─ Email: string (unique, indexed)
├─ PasswordHash: string (hashed)
├─ Bio: string?
├─ Avatar: string? (URL)
├─ Roles: List<string> (Admin, Member, Moderator)
├─ IsActive: bool
├─ JoinedAt: DateTimeOffset
└─ Events:
   ├─ UserCreated
   ├─ UserUpdated
   └─ UserDeactivated
```

#### Manga Aggregate
```
Manga (AuditableEntity)
├─ Id: Guid
├─ Title: string
├─ AlternativeTitle: string?
├─ Description: string
├─ Author: string
├─ Illustrator: string?
├─ Status: SeriesStatus (Ongoing, Completed, Hiatus, Cancelled)
├─ Genre: List<Genre> (Action, Comedy, Drama, Fantasy, etc.)
├─ Rating: ValueObject<decimal> (0-10)
├─ CoverImageUrl: string?
├─ PublishedAt: DateOnly
├─ Chapters: List<Chapter>
├─ SourceUrl: string? (original publication link)
└─ Events:
   ├─ MangaPublished
   ├─ MangaMetadataUpdated
   └─ MangaStatusChanged
```

#### Chapter Aggregate
```
Chapter (AuditableEntity)
├─ Id: Guid
├─ MangaId: Guid (foreign key)
├─ ChapterNumber: decimal (1, 1.5, 2, etc.)
├─ Title: string?
├─ ReleasedAt: DateOnly
├─ PageCount: int
├─ SourceUrl: string?
└─ Events:
   ├─ ChapterPublished
   ├─ ChapterUpdated
   └─ ChapterRemoved
```

#### UserLibrary Aggregate
```
UserLibrary (AuditableEntity)
├─ Id: Guid
├─ UserId: Guid (foreign key)
├─ Status: ReadingStatus (Reading, Completed, OnHold, Dropped, PlanToRead)
├─ Rating: int? (0-10)
├─ Notes: string?
├─ AddedAt: DateTimeOffset
└─ Events:
   ├─ MangaAddedToLibrary
   ├─ ReadingStatusChanged
   └─ RatingUpdated
```

#### Reading Progress Aggregate
```
ReadingProgress (AuditableEntity)
├─ Id: Guid
├─ UserId: Guid (foreign key)
├─ MangaId: Guid (foreign key)
├─ LastChapterRead: decimal
├─ LastReadAt: DateTimeOffset
├─ Percentage: int (0-100)
└─ Events:
   └─ ProgressUpdated
```

**Value Objects to Define**:
- `SeriesStatus` (enum or value object)
- `Genre` (enum)
- `ReadingStatus` (enum)
- `Rating` (record with validation)
- `UserEmail` (record with validation)

**Domain Events**:
- UserCreated
- MangaPublished
- ChapterPublished
- MangaAddedToLibrary
- ProgressUpdated
- (and all others listed above)

**Repositories Required**:
- IRepository<User>
- IRepository<Manga>
- IRepository<Chapter>
- IRepository<UserLibrary>
- IRepository<ReadingProgress>
- Plus aggregate roots

**Acceptance Criteria**:
- [x] All 16 entities defined (14 AuditableEntity, 1 BaseEntity)
- [x] 6 enums defined: SeriesStatus, MangaBadge, ReactionType, UserRole, AttachmentType, ViewTargetType
- [x] EF Core entity configurations (14+) with snake_case naming
- [x] 3 migrations applied: InitialSchema, AddPersonAndAttachment, AddViewStats
- [x] Soft-delete global filter on AuditableEntity
- [x] ViewStat anti-bloat strategy (daily time-bucketed aggregation)
- [ ] All value objects defined and tested
- [ ] All domain events defined
- [ ] Repository interfaces created
- [ ] Domain logic tests (>80% coverage)
- [ ] Aggregate root invariants enforced
- [ ] No framework dependencies in Domain

**Risks**:
- Scope creep with additional entities
- Consensus on entity relationships
- Performance impact of large aggregates

**Dependencies**:
- Phase 1 foundation must be complete

**Next**: → Phase 3: API Implementation

---

### Phase 3: Manga API Endpoints (COMPLETE)

**Status**: 100% Complete ✓
**Duration**: 1 sprint (1 week)
**Completed**: 2026-02-16

**Goals**:
- [x] Implement CRUD endpoints for Manga, Chapter, Genre
- [x] Create 14 MediatR handlers (commands & queries)
- [x] Build search/filter/sort capabilities
- [x] Implement pagination standard
- [x] Achieve >90% test coverage

**Implemented Endpoints**:

#### Manga Management (5 endpoints)
- [x] POST /api/manga (CreateMangaCommand)
- [x] GET /api/manga (ListMangaQuery: paginated, filterable by genre/status, sortable)
- [x] GET /api/manga/{id} (GetMangaQuery with chapter count)
- [x] PUT /api/manga/{id} (UpdateMangaCommand)
- [x] DELETE /api/manga/{id} (DeleteMangaCommand: soft-delete)

#### Chapter Management (2 endpoints)
- [x] POST /api/manga/{mangaId}/chapters (CreateChapterCommand)
- [x] GET /api/chapters/{id} (GetChapterQuery with denormalized chapter/page counts)
- [x] DELETE /api/chapters/{id} (DeleteChapterCommand: soft-delete)

#### Discovery & Search (2 endpoints)
- [x] GET /api/manga/search?q=... (SearchMangaQuery: title + author)
- [x] GET /api/manga/trending (GetTrendingMangaQuery: by view stats)

#### Genre Management (1 endpoint)
- [x] GET /api/genres (ListGenresQuery: with manga count per genre)

**Implemented Handlers & Validators** (14 total):
- CreateMangaCommandHandler + CreateMangaCommandValidator
- UpdateMangaCommandHandler + UpdateMangaCommandValidator
- DeleteMangaCommandHandler
- GetMangaQueryHandler
- ListMangaQueryHandler
- SearchMangaQueryHandler
- GetTrendingMangaQueryHandler
- CreateChapterCommandHandler + CreateChapterCommandValidator
- DeleteChapterCommandHandler
- GetChapterQueryHandler
- ListChaptersQueryHandler
- ListGenresQueryHandler

**Completed Acceptance Criteria**:
- [x] All endpoints implement Result<T> wrapper
- [x] All endpoints validate input via FluentValidation
- [x] OpenAPI documentation generated (Swagger/Scalar)
- [x] Integration tests for all endpoints (160 tests: 55 Phase 3)
- [x] p95 response time <200ms for GET, <500ms for POST/PUT/DELETE
- [x] Pagination works consistently (PageSize=20 default)
- [x] Sorting/filtering by genre, status, rating tested
- [x] Error responses in ProblemDetails (RFC 9457) format
- [x] Soft-delete works correctly for all mutations
- [x] Authorization enforced (manga create/update/delete)

**Key Implementation Details**:
- **Pagination:** PageNumber, PageSize (default 20), TotalCount, HasNextPage
- **Search:** Case-insensitive title + author search
- **Sorting:** By title, rating, view count, published date
- **Filters:** By status (Ongoing/Completed/Hiatus), genre, rating range
- **Denormalization:** Chapter counts cached on MangaSeries for performance

**Dependencies Met**:
- Phase 1: Foundation complete ✓
- Phase 2: Domain entities & auth complete ✓

**Test Results**:
- Total: 160 tests passing
- Auth: 48 tests (existing)
- Phase 3 Manga/Chapter/Genre: 55 tests
- Other: 57 tests
- Coverage: 90%+

**Next**: → Phase 5: Frontend Integration & Advanced Features

---

### Phase 4: File Upload & Media (COMPLETE)

**Status**: 100% Complete ✓
**Duration**: 1 day
**Completed**: 2026-02-16

**Goals**:
- [x] Implement JWT authentication
- [x] Enable user registration & login
- [x] Establish role-based authorization
- [x] Secure all endpoints appropriately
- [x] Zero auth-related security vulnerabilities
- [x] Full test coverage (48 tests passing)

**Implemented Components**:

#### Authentication Services
- **TokenService**: JWT generation/validation, token rotation, email/password reset tokens
- **PasswordHashService**: BCrypt password hashing (work factor 12)
- **MailKitEmailService**: Real SMTP email sending (prod)
- **DevEmailService**: Console email logging (dev)
- **TokenBlacklistService**: Redis-backed token revocation

#### Endpoints (8 total)
- [x] POST /api/auth/register (RegisterCommand)
- [x] POST /api/auth/login (LoginCommand)
- [x] POST /api/auth/refresh (RefreshCommand)
- [x] POST /api/auth/logout (LogoutCommand) [Authorized]
- [x] POST /api/auth/verify-email (VerifyEmailCommand)
- [x] POST /api/auth/forgot-password (ForgotPasswordCommand)
- [x] POST /api/auth/reset-password (ResetPasswordCommand)
- [x] GET /api/auth/me (GetCurrentUserQuery) [Authorized]

#### JWT Implementation
- [x] Access token expiry: 15 minutes
- [x] Refresh token TTL: 7 days, HttpOnly cookies
- [x] Claims: userId (sub), email, roles, permissions
- [x] Signing algorithm: HS256
- [x] Secret: appsettings (dev) / environment (prod)
- [x] Token rotation: Old token blacklisted on refresh
- [x] Email verification: Tokens expire after 24 hours
- [x] Password reset: Invalidates all user sessions

#### Authorization via RBAC
- [x] Permission enum with 6 permissions (View, Create, Update, Delete, Moderate, Admin)
- [x] Static RolePermissions mapping (User → Uploader → Moderator → Admin)
- [x] AuthorizationBehavior enforces permissions in MediatR pipeline
- [x] [Authorize] attributes on protected endpoints
- [x] Role-based checks work correctly

**Completed Acceptance Criteria**:
- [x] JWT tokens generated & validated correctly
- [x] Token refresh works with rotation
- [x] Password hashed with BCrypt (work factor 12)
- [x] Email uniqueness enforced
- [x] [Authorize] attributes protect endpoints
- [x] Role-based authorization working
- [x] Auth tests: 48 tests passing (100%)
- [x] No security vulnerabilities (OWASP compliant)
- [x] Audit logging for login/logout/register events
- [x] Token blacklisting prevents logout bypass
- [x] Email verification prevents account takeover
- [x] Password reset securely invalidates sessions
- [x] HTTPOnly cookies prevent XSS token theft
- [x] CSRF protection via SameSite=Strict

**Mitigation Applied**:
- Token compromise: 15-min access expiry, token rotation, blacklisting
- Password storage: BCrypt with work factor 12
- Session hijacking: HTTPOnly cookies, SameSite=Strict
- Email takeover: Email verification required

**Dependencies Met**:
- Domain User entity with password hash, roles
- RefreshToken entity for token storage
- Permission enum + RolePermissions mapping
- Redis integration for token blacklist

**Next**: → Phase 5: Frontend Integration

---

### Phase 4: File Upload & Media (COMPLETE)

**Status**: 100% Complete ✓
**Duration**: 1 day
**Completed**: 2026-02-16

**Goals**:
- [x] Implement file storage service
- [x] Add image processing capabilities
- [x] Create upload endpoint with authorization
- [x] Serve uploaded files with caching

**Implemented Components**:

#### File Storage Service
- **IFileStorageService** abstraction (Application layer)
- **LocalFileStorageService** implementation (Infrastructure) — saves to `uploads/` directory
- Unique filename generation with UUID
- Path normalization and validation

#### Image Processing Service
- **IImageProcessingService** abstraction for all image operations
- **SkiaSharpImageProcessingService** implementation
- Image resize to configurable dimensions
- WebP format conversion for web optimization
- Thumbnail generation (e.g., 150x225 for manga covers)

#### API Endpoints
- **POST /api/attachments/upload** — Multipart file upload
  - Validates file type (image/jpeg, image/png, image/webp)
  - Applies authorization (AttachmentUpload permission)
  - Processes images (resize, convert, thumbnail)
  - Returns AttachmentResponse with file URL and thumbnail
- **GET /api/attachments/{id}/file** — File serving endpoint
  - Returns original or resized file
  - Supports immutable cache headers for CDN caching
  - Fallback file serving with proper content types

#### Static File Serving
- StaticFilesMiddleware configured at /api/attachments/
- Immutable cache headers for versioned assets
- Proper MIME types for all file formats

#### Attachment Entity Updates
- Added ThumbnailUrl field for thumbnail URLs
- Added ThumbnailStoragePath field for internal storage path
- EF Core migration applied

#### NuGet Dependencies
- **SkiaSharp 3.x** for high-performance image processing

**Completed Acceptance Criteria**:
- [x] File upload endpoint functional with auth
- [x] Image processing (resize, convert, thumbnail) working
- [x] Static file serving with cache headers
- [x] Original + thumbnail URLs returned on upload
- [x] Attachment entity supports thumbnails
- [x] WebP format conversion enabled
- [x] File validation (type, size) enforced

**Key Implementation Details**:
- Files stored in `uploads/{year}/{month}/{day}/{uuid}.{ext}` structure
- Automatic thumbnail generation on upload
- Immutable cache strategy (versioned URLs)
- Multipart form validation
- Authorization integrated via MediatR AuthorizationBehavior

**Dependencies Met**:
- Phase 3: API endpoints available ✓
- Phase 2: Auth system working ✓

**Next**: → Phase 5: Advanced Features & Reading History

---

### Phase 5: Advanced Features & Reading History

**Status**: Pending
**Duration**: 2 sprints (6 weeks)
**Estimated Start**: 2026-02-17
**Estimated Completion**: 2026-03-31

**Goals**:
- Implement bookmark system
- Add reading history tracking
- Enhance user library features
- Optimize API performance

**Planned Features**:
- POST /api/bookmarks — Create bookmark
- DELETE /api/bookmarks/{id} — Remove bookmark
- GET /api/users/{userId}/bookmarks — List bookmarks
- POST /api/reading-history — Save reading progress
- GET /api/reading-history/{userId} — Get history
- GET /api/reading-history/{userId}/{mangaId} — Resume point

**Acceptance Criteria**:
- [ ] Bookmark CRUD endpoints functional
- [ ] Reading history saved on chapter view
- [ ] Resume reading from last chapter
- [ ] User library synced across devices
- [ ] All endpoints tested with >90% coverage

**Dependencies**:
- Phase 4: File storage complete ✓
- Backend ready for user features

**Next**: → Phase 6: Deployment & DevOps

---

### Phase 6: Deployment & DevOps

**Status**: Blocked (awaiting Phase 5)
**Duration**: 1.5 sprints (5 weeks)
**Estimated Start**: 2026-08-09
**Estimated Completion**: 2026-09-12

**Goals**:
- Production-ready infrastructure
- Automated CI/CD pipeline
- Database migration automation
- Health monitoring & alerting
- Zero-downtime deployments

**Components**:

#### Containerization
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
COPY src/Manga.Api/bin/Release/net10.0/publish/ /app
WORKDIR /app
EXPOSE 8080
CMD ["dotnet", "Manga.Api.dll"]
```

#### CI/CD Pipeline (GitHub Actions)
1. Push to main/develop
2. Run linting (StyleCop)
3. Run all tests
4. Build Docker image
5. Push to registry (Docker Hub / ACR)
6. Deploy to staging
7. Smoke tests
8. Manual approval
9. Deploy to production
10. Health check

#### Database Migrations
```bash
# Auto-applied on startup
dotnet ef database update
```

#### Monitoring Stack
- **Application Insights**: Request tracking, exceptions, performance
- **Datadog**: Infrastructure metrics, APM traces
- **ELK Stack**: Log aggregation, analysis
- **Grafana**: Metrics visualization

#### Health Checks
- GET /health → 200 (app healthy)
- GET /health/db → 200 (DB connected)
- GET /health/cache → 200 (Redis/cache healthy)

#### Deployment Strategy
- Blue-green deployment (zero downtime)
- Automated rollback on health check failure
- Database migration runs before traffic switch

**Acceptance Criteria**:
- [ ] Docker image builds & runs
- [ ] CI/CD pipeline fully automated
- [ ] All tests pass in CI before deploy
- [ ] Database migrations run automatically
- [ ] Health checks configured
- [ ] Monitoring dashboards live
- [ ] Alerts configured for anomalies
- [ ] Deployment to prod < 5 minutes
- [ ] 99.9% uptime SLA achievable

**Risks**:
- Database migration blocking (mitigation: backward-compatible migrations)
- Deployment failures (mitigation: automated rollback, smoke tests)

**Dependencies**:
- Phase 5: Feature complete API
- Phase 5: Frontend integration working

**Next**: → Maintenance & Iteration

---

## Timeline Summary

| Phase | Status | Start | End | Duration |
|-------|--------|-------|-----|----------|
| 1: Foundation | Complete | 2026-02-06 | 2026-02-13 | 1 week |
| 2: Domain Modeling & Auth | Complete | 2026-02-13 | 2026-02-15 | 3 days |
| 3: Manga API Endpoints | Complete | 2026-02-16 | 2026-02-16 | 1 week |
| 4: File Upload & Media | **Complete** | 2026-02-16 | 2026-02-16 | 1 day |
| 5: Advanced Features | Planned | 2026-02-17 | 2026-03-31 | 6 weeks |
| 6: Deployment & DevOps | Planned | 2026-04-01 | 2026-05-15 | 6 weeks |
| **Total** | | 2026-02-06 | ~2026-05-15 | **~13 weeks** |

---

## Success Metrics by Phase

### Phase 1 (Foundation)
- [x] Solution builds without warnings
- [x] All layers properly organized
- [x] DI container working
- [x] Health endpoint responding

### Phase 2 (Domain Modeling)
- [ ] All entities defined & validated
- [ ] Domain logic >80% tested
- [ ] No framework dependencies in Domain
- [ ] Entity relationships correctly modeled

### Phase 3 (API Endpoints) ✓ COMPLETE
- [x] All CRUD endpoints working (7 endpoints)
- [x] API response time p95 <200ms
- [x] OpenAPI docs complete & accurate
- [x] >90% endpoint test coverage (55/55 tests passing)
- [x] 14 MediatR handlers implemented
- [x] Pagination & sorting working
- [x] Search & filtering functional
- [x] Soft-delete working correctly

### Phase 4 (File Upload & Media) ✓ COMPLETE
- [x] File storage service implemented
- [x] Image processing (resize, convert, thumbnail)
- [x] Upload endpoint with authorization (2 endpoints total)
- [x] Static file serving with cache headers
- [x] Attachment entity with thumbnail support
- [x] 16 MediatR handlers total
- [x] SkiaSharp 3.x integration

### Phase 5 (Advanced Features)
- [ ] Bookmark CRUD endpoints
- [ ] Reading history tracking
- [ ] User library management
- [ ] All bookmarks/history tests passing

### Phase 5 (Advanced Features)
- [ ] Bookmarking system implemented
- [ ] Reading history tracking
- [ ] User library management
- [ ] Recommendations engine

### Phase 6 (DevOps)
- [ ] Automated CI/CD fully functional
- [ ] All tests pass in CI/CD
- [ ] Production deployment < 5 min
- [ ] 99.9% uptime achievable

---

## Known Issues & Backlog

### High Priority (Phase 7+)
- Rate limiting per user/IP
- WebSocket support for real-time updates
- Advanced search (full-text, faceted)
- Caching layer (Redis)
- Image optimization pipeline
- User notifications system

### Medium Priority
- Recommendation engine (based on reading history)
- Community features (ratings, comments, reviews)
- Import/export functionality
- Batch operations on chapters
- Archive old reading progress

### Low Priority
- Mobile app (native iOS/Android)
- Offline reading capability
- Manga/chapter metadata AI enrichment
- Translation workflow

---

## Risk Assessment

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|-----------|
| Scope creep in Phase 2 | High | Medium | Sprint planning, feature freeze |
| DB performance degradation | High | Low | Query profiling, indexing strategy |
| Auth security vulnerabilities | Critical | Low | Pen testing, security review |
| Migration downtime | High | Medium | Blue-green deployment, backward-compat migrations |
| Team member unavailability | Medium | Medium | Knowledge sharing, documentation |
| API design changes needed | Medium | High | Early stakeholder review, prototype endpoints |

---

## Dependency Graph

```
Phase 1 (Foundation) ✓
    ↓
Phase 2 (Domain Modeling)
    ↓
Phase 3 (API Implementation)
    ↓
Phase 4 (Auth & Authz)
    ↓
Phase 5 (Frontend Integration)
    ↓
Phase 6 (Deployment & DevOps)
    ↓
Production Ready
```

All phases are sequential; no parallel tracks.

---

## Review & Adjustment

- **Weekly**: Sprint progress review, blockers identified
- **Biweekly**: Stakeholder demo, feedback collection
- **End of phase**: Retrospective, lessons learned, roadmap adjustment

Next roadmap review: End of Phase 2 (2026-04-10)

---

**Document Version**: 1.4 (Phase 4: File Upload & Media Completed)
**Last Updated**: 2026-02-16
**Created By**: Documentation Team
