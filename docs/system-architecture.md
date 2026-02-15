# System Architecture

## Overview

Manga-dotnet follows **Clean Architecture** with strict layer separation and **CQRS** pattern for request handling. All external dependencies flow inward; layers never skip dependencies.

---

## Layered Architecture Diagram

```
┌──────────────────────────────────────────────────────────┐
│                   API Layer (Presentation)                │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  HTTP Endpoints (Minimal API)                       │ │
│  │  - GET /health                                      │ │
│  │  - (Future: POST /mangasd, GET /mangaka, etc)      │ │
│  └─────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  Middleware Pipeline                                │ │
│  │  - GlobalExceptionHandler → ProblemDetails          │ │
│  │  - Status code pages                                │ │
│  │  - HTTPS redirection                                │ │
│  └─────────────────────────────────────────────────────┘ │
└────────────────────────────┬─────────────────────────────┘
                             │ depends on
                             ↓
┌──────────────────────────────────────────────────────────┐
│              Application Layer (Use Cases)                │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  MediatR Command/Query Handlers                     │ │
│  │  - Commands: Create, Update, Delete                 │ │
│  │  - Queries: Get, List, Search                       │ │
│  │  - Handlers implement business use cases            │ │
│  └─────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  Pipeline Behaviors (Cross-Cutting)                 │ │
│  │  - ValidationBehavior: FluentValidation             │ │
│  │  - LoggingBehavior: Request/response logging        │ │
│  │  - (Future: Authorization, Rate limiting)           │ │
│  └─────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  Contracts & Abstractions                           │ │
│  │  - IAppDbContext (EF Core abstraction)              │ │
│  │  - IDateTimeProvider (testable clock)               │ │
│  │  - ICurrentUserService (auth info)                  │ │
│  │  - IRepository<T> (generic CRUD)                    │ │
│  └─────────────────────────────────────────────────────┘ │
└────────────────────────────┬─────────────────────────────┘
                             │ depends on
                             ↓
┌──────────────────────────────────────────────────────────┐
│           Infrastructure Layer (External Services)        │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  Persistence (EF Core)                              │ │
│  │  - AppDbContext: 16 DbSets, IAppDbContext impl      │ │
│  │  - Soft-delete global query filter (AuditableEntity)
│  │  - 14+ fluent entity configurations, indexes        │ │
│  │  - 3 migrations: InitialSchema, AddPerson, ViewStat
│  │  - Supports snake_case column naming                │ │
│  └─────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  Repositories                                       │ │
│  │  - BaseRepository<T> (implements IRepository<T>)    │ │
│  │  - Concrete repos for domain aggregates             │ │
│  └─────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  Interceptors & Services                            │ │
│  │  - AuditableEntityInterceptor (auto-audit)          │ │
│  │  - DateTimeProvider (UTC clock)                     │ │
│  └─────────────────────────────────────────────────────┘ │
└────────────────────────────┬─────────────────────────────┘
                             │ depends on
                             ↓
┌──────────────────────────────────────────────────────────┐
│            Domain Layer (Business Logic)                  │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  Entities (16 defined: 14 auditable, 1 optimized)   │ │
│  │  - MangaSeries, Chapter, ChapterPage (manga core)   │ │
│  │  - User, Person, Comment, CommentReaction          │ │
│  │  - Bookmark, ReadingHistory (user features)         │ │
│  │  - Genre, MangaGenre, AlternativeTitle             │ │
│  │  - Attachment (file storage: covers, banners)      │ │
│  │  - ViewStat (daily aggregation, anti-bloat)        │ │
│  │  - Inherit from AuditableEntity (soft-delete)      │ │
│  └─────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  Value Objects                                      │ │
│  │  - Rating, SeriesStatus, Genre, ISBN, etc.          │ │
│  │  - Immutable, structural equality                   │ │
│  └─────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  Domain Events                                      │ │
│  │  - MangaAddedToCatalog, UserCreated, etc.           │ │
│  │  - Implement IDomainEvent (MediatR INotification)   │ │
│  └─────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  Interfaces (no implementations)                    │ │
│  │  - IRepository<T>, IUnitOfWork                      │ │
│  └─────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  Exceptions (Domain errors)                         │ │
│  │  - DomainException, NotFoundException               │ │
│  └─────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────┘
         (No external dependencies - pure C#)
```

---

## Request/Response Flow

### Command Execution Flow (Create/Update/Delete)

```
┌─────────────────┐
│  HTTP Request   │
│  POST /mangask  │
└────────┬────────┘
         │
         ↓
┌──────────────────────────┐
│ Api Layer (Controller)   │
│ - Bind JSON to Command   │
│ - Dispatch via Mediator  │
└────────┬─────────────────┘
         │
         ↓
┌──────────────────────────────────────────┐
│ MediatR Pipeline - Behaviors             │
│ 1. ValidationBehavior                    │
│    - Runs FluentValidation               │
│    - Throws ValidationException if fails │
└────────┬─────────────────────────────────┘
         │
         ↓ (if valid)
┌──────────────────────────────────────────┐
│ MediatR Pipeline - Behaviors             │
│ 2. LoggingBehavior                       │
│    - Logs command input                  │
│    - Measures execution time             │
└────────┬─────────────────────────────────┘
         │
         ↓
┌──────────────────────────────────────────┐
│ Application Layer - Command Handler      │
│ CreateMangaCommandHandler                │
│ - Create domain entity                   │
│ - Call repository.AddAsync()             │
│ - Call unitOfWork.SaveChangesAsync()     │
└────────┬─────────────────────────────────┘
         │
         ↓
┌──────────────────────────────────────────┐
│ Infrastructure Layer - Repository        │
│ MangaRepository : BaseRepository<Manga>  │
│ - EF Core DbSet<Manga>.AddAsync()        │
└────────┬─────────────────────────────────┘
         │
         ↓
┌──────────────────────────────────────────┐
│ Infrastructure - DbContext               │
│ AppDbContext.SaveChangesAsync()          │
│ - Triggers AuditableEntityInterceptor    │
│   (populates CreatedAt, CreatedBy)       │
│ - Executes INSERT SQL via Npgsql         │
└────────┬─────────────────────────────────┘
         │
         ↓ (success)
┌──────────────────────────────────────────┐
│ Return Result<Guid>                      │
│ {                                        │
│   succeeded: true,                       │
│   data: mangaId,                         │
│   message: "Manga created"               │
│ }                                        │
└────────┬─────────────────────────────────┘
         │
         ↓
┌─────────────────────────────┐
│ HTTP Response (200 OK)      │
│ Content-Type: application/json
└─────────────────────────────┘

(Exception Handling)
  ValidationException → 422 Unprocessable Entity
  NotFoundException → 404 Not Found
  DomainException → 400 Bad Request
  Other → 500 Internal Server Error
```

### Query Execution Flow (Read Operations)

```
┌─────────────────┐
│  HTTP Request   │
│  GET /mangask   │
└────────┬────────┘
         │
         ↓
┌──────────────────────────┐
│ Api Layer (Controller)   │
│ - Create Query object    │
│ - Dispatch via Mediator  │
└────────┬─────────────────┘
         │
         ↓
┌──────────────────────────────────────────┐
│ MediatR Pipeline - Behaviors             │
│ LoggingBehavior                          │
│ - Logs query input                       │
└────────┬─────────────────────────────────┘
         │
         ↓
┌──────────────────────────────────────────┐
│ Application Layer - Query Handler        │
│ GetMangaByIdQueryHandler                 │
│ - Call repository.GetByIdAsync()         │
│ - Return Result<MangaDto>                │
└────────┬─────────────────────────────────┘
         │
         ↓
┌──────────────────────────────────────────┐
│ Infrastructure Layer - Repository        │
│ MangaRepository.GetByIdAsync()           │
│ - Query EF Core DbSet<Manga>             │
│ - Soft-delete filter applied             │
│ - Execute SELECT SQL via Npgsql          │
└────────┬─────────────────────────────────┘
         │
         ↓
┌──────────────────────────────────────────┐
│ Return MangaDto or null                  │
└────────┬─────────────────────────────────┘
         │
         ↓
┌─────────────────────────────┐
│ HTTP Response (200 OK)      │
│ With MangaDto payload       │
└─────────────────────────────┘
```

---

## Data Flow: Database Persistence

### Write Path (Create/Update/Delete)

```
Domain Entity
    ↓
Passed to Repository.AddAsync/UpdateAsync
    ↓
DbSet<T>.Add/Update() in EF Core
    ↓
DbContext.SaveChangesAsync()
    ↓
AuditableEntityInterceptor
- Intercepts SaveChanges event
- Checks ChangeTracker for Added/Modified entities
- Auto-populates CreatedAt, CreatedBy (Add)
- Auto-populates LastModifiedAt, LastModifiedBy (Update)
- Respects existing values if already set
    ↓
EF Core generates SQL
    ↓
Npgsql executes against PostgreSQL
    ↓
Success: Entity persisted with audit trail
Failure: Exception thrown, transaction rolled back
```

### Read Path (Query)

```
IRepository<T>.GetAllAsync/GetByIdAsync/FindAsync
    ↓
EF Core DbSet<T> query builder
    ↓
Global Query Filter Applied
- IsDeleted == false (for AuditableEntity)
- Soft-deleted rows excluded automatically
    ↓
Npgsql executes SELECT
    ↓
Results mapped to entity objects
    ↓
Returned as IReadOnlyList<T> or T?
```

### Soft Delete

```
Entity with IsDeleted = true
    ↓
Repository.UpdateAsync()
    ↓
DbContext.SaveChangesAsync()
    ↓
UPDATE statement: SET IsDeleted = true, DeletedAt = NOW()
    ↓
Physical row remains in database
Subsequent queries automatically exclude it
    ↓
Admin queries can force: .IgnoreQueryFilters()
```

---

## Dependency Injection Hierarchy

### Startup Chain (Program.cs)

```
1. builder.Services.AddApplicationServices()
   ├─ MediatR registration
   │  ├─ Scans assembly for IRequestHandler implementations
   │  └─ Registers all pipeline behaviors
   ├─ FluentValidation registration
   │  └─ Scans assembly for validators
   └─ Logging configuration

2. builder.Services.AddInfrastructureServices(config)
   ├─ EF Core DbContext
   │  ├─ AppDbContext configured
   │  ├─ Npgsql provider registered
   │  └─ Connection string from config
   ├─ Interceptors
   │  └─ AuditableEntityInterceptor
   ├─ Repositories
   │  ├─ Generic BaseRepository<T>
   │  └─ Concrete repositories (UserRepository, etc.)
   ├─ Services
   │  └─ DateTimeProvider
   └─ IUnitOfWork (implemented by AppDbContext)

3. API Layer Services
   ├─ AddOpenApi()
   ├─ AddProblemDetails()
   ├─ AddExceptionHandler<GlobalExceptionHandler>()
   ├─ AddValidation()
   ├─ AddHttpContextAccessor()
   └─ AddScoped<ICurrentUserService, CurrentUserService>()
```

### DI Resolution

```
Request comes in
    ↓
Framework creates new scope (per HTTP request)
    ↓
MediatR creates handler instance
    ↓
Handler constructor resolved:
  - IRepository<User> → UserRepository instance
    - Depends on: AppDbContext
    - Depends on: ILogger
  - IUnitOfWork → AppDbContext instance
    - Depends on: DbContextOptions<AppDbContext>
    - Depends on: ILogger
  - IDateTimeProvider → DateTimeProvider instance
    - Depends on: ILogger
    ↓
Handler executes, scope closed
```

---

## Middleware Pipeline

```
Request
  ↓
app.UseExceptionHandler()
├─ Catches all exceptions
├─ Maps to ProblemDetails
└─ Returns 400/404/422/500 based on exception type
  ↓
app.UseStatusCodePages()
├─ Standardizes responses for 4xx/5xx
  ↓
app.UseHttpsRedirection() (production)
├─ Redirects http → https
  ↓
MapOpenApi() (development only)
├─ Exposes /openapi/v1.json
  ↓
MapScalarApiReference() (development only)
├─ Serves /scalar/v1 UI
  ↓
MapHealthEndpoints()
├─ GET /health → { status: "healthy" }
  ↓
(Future endpoint mappings)
├─ MapMangaEndpoints()
├─ MapUserEndpoints()
├─ MapLibraryEndpoints()
  ↓
Response sent to client
```

---

## Entity Lifecycle

### Creation

```
new User { Name = "John", Email = "john@example.com" }
    ↓
_repository.AddAsync(user)
    ↓
DbSet<User>.Add()
    ↓
DbContext.SaveChangesAsync()
    ↓
ChangeTracker detects: EntityState.Added
    ↓
AuditableEntityInterceptor intercepts
    ↓
Sets:
- CreatedAt = DateTimeOffset.UtcNow
- CreatedBy = ICurrentUserService.UserId
    ↓
INSERT executed
    ↓
Entity now tracked, Id generated (GUID)
```

### Update

```
user.Name = "Jane"
    ↓
_repository.UpdateAsync(user)
    ↓
DbSet<User>.Update()
    ↓
DbContext.SaveChangesAsync()
    ↓
ChangeTracker detects: EntityState.Modified
    ↓
AuditableEntityInterceptor intercepts
    ↓
Sets:
- LastModifiedAt = DateTimeOffset.UtcNow
- LastModifiedBy = ICurrentUserService.UserId
(Does NOT change CreatedAt/CreatedBy)
    ↓
UPDATE executed
```

### Soft Delete

```
user.IsDeleted = true
    ↓
_repository.UpdateAsync(user)
    ↓
DbContext.SaveChangesAsync()
    ↓
Interceptor also sets:
- DeletedAt = DateTimeOffset.UtcNow
    ↓
UPDATE executed
    ↓
Future queries include global filter: WHERE IsDeleted = false
Result: User never appears in normal queries
```

### Hard Delete (Admin Only)

```
_repository.RemoveAsync(user)
    ↓
DbSet<User>.Remove()
    ↓
DbContext.SaveChangesAsync()
    ↓
DELETE executed (physical row deleted)
    ↓
Data permanently removed
```

---

## Error Handling Strategy

### Exception Mapping

```
Exception raised in handler
    ↓
Propagates up through MediatR
    ↓
GlobalExceptionHandler catches
    ↓
Type matching:
  ├─ ValidationException
  │  └─ → 422 Unprocessable Entity
  │     {
  │       errors: [{ field: "Name", message: "Required" }]
  │     }
  ├─ NotFoundException
  │  └─ → 404 Not Found
  ├─ DomainException
  │  └─ → 400 Bad Request
  └─ Others (ArgumentException, NullReferenceException, etc.)
     └─ → 500 Internal Server Error
         (Logged with full stack trace)
    ↓
ProblemDetails response returned
    ↓
HTTP response sent to client
```

### Logging

```
Before Handler Execution:
- Log command/query name
- Log correlation ID

During Execution:
- Catch exceptions
- Log with ERROR level
- Include exception context

After Execution:
- Log response
- Log execution time
- For slow queries: log SQL and parameters
```

---

## Concurrency & Transactions

### Transaction Scope

Each `SaveChangesAsync()` is wrapped in implicit transaction:
```
BEGIN TRANSACTION
  ├─ Interceptor runs
  ├─ SQL statements execute
  ├─ All success or all rollback
COMMIT TRANSACTION
```

### Optimistic Concurrency

For future: Add concurrency tokens to entities requiring optimistic locking
```csharp
public class User : AuditableEntity
{
    public string Name { get; set; }

    [Timestamp] // EF Core concurrency token
    public byte[] RowVersion { get; set; }
}
```

---

## Performance Considerations

### Indexing Strategy (PostgreSQL)

```sql
-- Primary keys (auto-indexed)
CREATE INDEX idx_users_id ON users(id);

-- Soft-delete filtering
CREATE INDEX idx_users_is_deleted ON users(is_deleted);

-- Common queries
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_mangaka_author ON mangaka(author);
CREATE INDEX idx_chapters_manga_id ON chapters(manga_id);

-- Soft-delete + filter combined
CREATE INDEX idx_users_is_deleted_id
ON users(is_deleted, id);
```

### Query Optimization

**Include Navigation Properties**:
```csharp
var user = await _dbContext.Users
    .Include(u => u.Mangaka) // Eager load
    .FirstOrDefaultAsync(u => u.Id == id, ct);
```

**Avoid N+1**:
```csharp
// Bad: 1 + N queries
var users = await _repository.GetAllAsync(ct);
foreach (var user in users)
{
    var mangaka = await GetUserManga(user.Id, ct); // N queries
}

// Good: 1 query
var users = await _dbContext.Users
    .Include(u => u.Mangaka)
    .ToListAsync(ct);
```

**Projection for DTO**:
```csharp
var userDtos = await _dbContext.Users
    .Where(u => !u.IsDeleted)
    .Select(u => new UserDto(u.Id, u.Name, u.Email))
    .ToListAsync(ct); // SELECT Id, Name, Email only
```

---

## Authentication & Security (Implemented)

### JWT Token Lifecycle

```
User submits credentials (RegisterCommand / LoginCommand)
    ↓
Validate email format & password strength (FluentValidation)
    ↓
Check email uniqueness (db query)
    ↓
Hash password with BCrypt (work factor 12)
    ↓
Create User entity + optional RefreshToken
    ↓
SaveChangesAsync → audit trail auto-populated
    ↓
TokenService.GenerateAccessToken()
  - Claims: userId (sub), email (email), roles, permissions
  - Expiry: 15 minutes
  - Algorithm: HS256
  - Signature: JWT_SECRET
    ↓
TokenService.GenerateRefreshToken()
  - Random 32-byte token
  - Expires: 7 days
  - Stored in DB + HttpOnly cookie
    ↓
Return AuthToken response with access + refresh
    ↓
Client stores access (memory), refresh (HttpOnly cookie)
```

### Request Authentication Flow

```
Client sends request with access token in Authorization header
    ↓
JWT authentication middleware validates signature
    ↓
Extract claims → populate HttpContext.User
    ↓
[Authorize] attribute checks token presence
    ↓
AuthorizationBehavior (MediatR) checks RBAC permissions
    ↓
CurrentUserService extracts userId from claims
    ↓
Handler uses ICurrentUserService.UserId for audit trail
    ↓
Response includes fresh access token in Set-Cookie header
```

### Token Refresh Flow

```
Access token expired (401)
    ↓
Client sends RefreshCommand with refresh token from cookie
    ↓
TokenService validates refresh token (signature, expiry, blacklist)
    ↓
If blacklisted → 401 (logout invalidated it)
    ↓
If valid → Generate new access token
    ↓
Optionally rotate refresh token (new random token, store old in blacklist)
    ↓
Return new access token + new refresh token (if rotated)
    ↓
Client updates memory + cookie
```

### Role-Based Access Control (RBAC)

```
Permission enum (static mapping):
  View    → Roles: User, Moderator, Admin
  Create  → Roles: Uploader, Admin
  Update  → Roles: Creator (own), Moderator, Admin
  Delete  → Roles: Creator (own), Moderator, Admin
  Moderate → Roles: Moderator, Admin
  Admin   → Roles: Admin only

AuthorizationBehavior checks:
1. Extract user roles from claims
2. Map role → permissions via RolePermissions
3. Check if required permission present
4. If missing → 403 Forbidden
```

### Email Verification & Password Reset

```
RegisterCommand → Send verification email
    ↓
VerifyEmailCommand (token from email link)
    ↓
TokenService.ValidateEmailToken()
    ↓
Update User.IsEmailVerified = true
    ↓
Subsequent login checks verification (configurable requirement)

ForgotPasswordCommand → Send reset email
    ↓
ResetPasswordCommand (token + new password)
    ↓
TokenService.ValidatePasswordResetToken()
    ↓
Hash new password
    ↓
Update User.PasswordHash
    ↓
Invalidate all refresh tokens (force re-login on other devices)
```

### Token Blacklisting & Logout

```
LogoutCommand (user authenticated)
    ↓
Extract refresh token from request
    ↓
RedisTokenBlacklistService.BlacklistTokenAsync(token, expiry)
    ↓
Token stored in Redis with TTL (expires at original expiry time)
    ↓
Subsequent RefreshCommand checks Redis blacklist
    ↓
If found → 401 Unauthorized
    ↓
Response clears HttpOnly cookie (Set-Cookie with Max-Age=0)
    ↓
Client memory access token becomes invalid on next API call (401)
```

### Security Measures

1. **Password Storage**: BCrypt with work factor 12 (configurable)
2. **Token Expiry**: Access 15min, Refresh 7 days
3. **Token Rotation**: Optional on refresh (new token invalidates old)
4. **Cookie Security**: HttpOnly, Secure, SameSite=Strict
5. **Email Verification**: Tokens expire, one-time use
6. **Password Reset**: Invalidates all sessions
7. **Rate Limiting**: (Future, placeholder in code)
8. **OWASP Compliance**: Input validation, output encoding, CSRF (SameSite)

---

## Deployment Architecture

### Development

```
Visual Studio / .NET CLI
    ↓
dotnet run / F5
    ↓
Launches Kestrel on http://localhost:5087
    ↓
Seed development data
    ↓
OpenAPI UI at /scalar/v1
```

### Production (Future)

```
Docker image
    ↓
Kestrel in container
    ↓
Reverse proxy (nginx/Azure App Gateway)
    ↓
Load balancer
    ↓
Health checks: GET /health
    ↓
PostgreSQL (managed, replicated)
    ↓
Logging aggregation (ELK/Seq)
    ↓
Monitoring (Datadog/Azure Monitor)
```

---

**Document Version**: 1.1
**Last Updated**: 2026-02-15
