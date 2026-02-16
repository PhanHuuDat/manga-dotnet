# Codebase Summary: manga-dotnet

## Overview

Manga-dotnet is a .NET 10 Clean Architecture REST API with CQRS pattern for manga platform management. Phase 6 complete with file upload, view tracking, CI/CD pipelines, and chapter page anti-leak scrambling. Total 17 MediatR handlers for manga/chapter/genre/attachment/view operations, 11 API endpoints, file storage services, image processing + scrambling pipeline, Redis-backed view analytics, and GitHub Actions CI/CD.

**Total LOC (source only)**: ~2,700+ lines across 4 layers + 3 test projects (Phase 4-5: file upload, view tracking, CI/CD pipelines added)

---

## Directory Structure

```
manga-dotnet/
├── Directory.Build.props          # Global: net10.0, nullable, implicit usings
├── manga-dotnet.slnx              # Solution file (XML format)
├── appsettings.json               # Root logging config
├── appsettings.Development.json   # Dev logging config
│
├── src/
│   ├── Manga.Domain/              # Layer 1: Pure domain logic, no external deps
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs      # GUID Id + domain events
│   │   │   ├── AuditableEntity.cs # Extends BaseEntity: CreatedAt/By, timestamps, soft-delete
│   │   │   ├── IDomainEvent.cs    # MediatR INotification marker
│   │   │   └── ValueObject.cs     # DDD value object base with structural equality
│   │   ├── Interfaces/
│   │   │   ├── IUnitOfWork.cs     # SaveChangesAsync contract
│   │   │   ├── ITokenService.cs   # JWT token generation/validation
│   │   │   ├── IEmailService.cs   # Email sending (SMTP/dev)
│   │   │   └── Repositories/
│   │   │       └── IRepository.cs # Generic CRUD: GetById, GetAll, Find, Add, Update, Remove
│   │   ├── Exceptions/
│   │   │   ├── DomainException.cs    # Base domain exception
│   │   │   └── NotFoundException.cs  # Entity not found
│   │   ├── Entities/               # 16 domain entities
│   │   │   ├── MangaSeries.cs      # Manga catalog entry (title, chapters, metadata)
│   │   │   ├── Chapter.cs          # Chapter of manga series
│   │   │   ├── ChapterPage.cs      # Individual page of chapter
│   │   │   ├── AlternativeTitle.cs # Manga titles in other languages
│   │   │   ├── Genre.cs            # Manga genres/categories
│   │   │   ├── MangaGenre.cs       # Join table: manga to genres
│   │   │   ├── Comment.cs          # User comments on series/chapters
│   │   │   ├── CommentReaction.cs  # Reactions to comments (like/dislike)
│   │   │   ├── Bookmark.cs         # User bookmarks
│   │   │   ├── ReadingHistory.cs   # User reading progress
│   │   │   ├── User.cs             # Platform users
│   │   │   ├── Person.cs           # Authors/artists
│   │   │   ├── Attachment.cs       # File storage (covers, banners, avatars)
│   │   │   └── ViewStat.cs         # Daily view aggregation (anti-bloat)
│   │   ├── Enums/                  # 7 domain enums
│   │   │   ├── SeriesStatus.cs     # (Ongoing, Completed, Hiatus)
│   │   │   ├── MangaBadge.cs       # (Hot, Top, New)
│   │   │   ├── ReactionType.cs     # (Like, Dislike)
│   │   │   ├── UserRole.cs         # (User, Moderator, Admin)
│   │   │   ├── AttachmentType.cs   # (Cover, Banner, Avatar, ChapterPage)
│   │   │   ├── ViewTargetType.cs   # (Series, Chapter)
│   │   │   └── Permission.cs       # (View, Create, Update, Delete, Moderate, Admin)
│   │   ├── Events/                 # Domain events (reserved)
│   │   └── ValueObjects/           # Value objects (reserved)
│   │
│   ├── Manga.Application/         # Layer 2: Use cases, CQRS handlers (depends on Domain)
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IAppDbContext.cs         # EF Core DbContext abstraction
│   │   │   │   ├── IDateTimeProvider.cs     # Testable clock service
│   │   │   │   ├── ICurrentUserService.cs   # Auth user info extraction
│   │   │   │   ├── IFileStorageService.cs   # File storage abstraction
│   │   │   │   └── IImageProcessingService.cs # Image processing abstraction
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs    # MediatR pipeline: FluentValidation
│   │   │   │   ├── LoggingBehavior.cs       # MediatR pipeline: request/response logging
│   │   │   │   └── AuthorizationBehavior.cs # RBAC permission checks
│   │   │   └── Models/
│   │   │       ├── PagedResponse.cs         # Pagination: Items[], PageNumber, TotalPages, HasNextPage
│   │   │       └── Result.cs                # Operation result: Success/Failure, Data, Errors
│   │   ├── Features/                # CQRS handlers by domain (Phase 3-4)
│   │   │   ├── Manga/               # Manga commands & queries
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateMangaCommand.cs
│   │   │   │   │   ├── UpdateMangaCommand.cs
│   │   │   │   │   └── DeleteMangaCommand.cs
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── GetMangaQuery.cs
│   │   │   │   │   ├── ListMangaQuery.cs
│   │   │   │   │   ├── SearchMangaQuery.cs
│   │   │   │   │   └── GetTrendingMangaQuery.cs
│   │   │   │   └── Validators/ (FluentValidation)
│   │   │   ├── Chapter/              # Chapter commands & queries
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateChapterCommand.cs
│   │   │   │   │   └── DeleteChapterCommand.cs
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── GetChapterQuery.cs
│   │   │   │   │   └── ListChaptersQuery.cs
│   │   │   │   └── Validators/
│   │   │   ├── Genre/                # Genre queries
│   │   │   │   └── Queries/
│   │   │   │       └── ListGenresQuery.cs
│   │   │   └── Attachment/           # File upload & serving (Phase 4)
│   │   │       ├── Commands/
│   │   │       │   └── UploadAttachmentCommand.cs
│   │   │       ├── Queries/
│   │   │       │   └── GetAttachmentFileQuery.cs
│   │   │       └── Validators/
│   │   │   └── Views/                # View tracking analytics (Phase 5)
│   │   │       ├── Commands/
│   │   │       │   └── TrackViewCommand.cs
│   │   │       ├── Validators/
│   │   │       └── Handlers/
│   │   └── DependencyInjection.cs   # Registers MediatR, FluentValidation, behaviors, file services, view tracking
│   │
│   ├── Manga.Infrastructure/       # Layer 3: External services, persistence (depends on Application)
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs      # EF Core context, implements IAppDbContext + IUnitOfWork
│   │   │   │                        # DbSets for all 16 entities, soft-delete filter
│   │   │   ├── Interceptors/
│   │   │   │   └── AuditableEntityInterceptor.cs # Populates CreatedAt/By, LastModifiedAt/By, DeletedAt
│   │   │   ├── Configurations/      # 16+ EF Core entity configurations
│   │   │   │   ├── MangaSeriesConfiguration.cs
│   │   │   │   ├── ChapterConfiguration.cs
│   │   │   │   ├── PersonConfiguration.cs
│   │   │   │   ├── AttachmentConfiguration.cs (ThumbnailUrl, ThumbnailStoragePath, ScrambleSeed, ScrambleGridSize)
│   │   │   │   ├── ViewStatConfiguration.cs (composite PK: TargetType, TargetId, ViewDate)
│   │   │   │   └── ...other entity configs
│   │   │   ├── Migrations/
│   │   │   │   ├── 20260215044212_InitialSchema.cs
│   │   │   │   ├── 20260215064500_AddPersonAndAttachment.cs
│   │   │   │   ├── 20260215074337_AddViewStats.cs
│   │   │   │   └── 20260216082000_AddAttachmentThumbnails.cs (Phase 4)
│   │   │   └── Repositories/
│   │   │       └── BaseRepository.cs # EF Core IRepository<T> implementation
│   │   ├── Services/
│   │   │   ├── DateTimeProvider.cs               # System clock (UTC) implementation
│   │   │   ├── TokenService.cs                   # JWT token generation/validation, refresh token rotation
│   │   │   ├── MailKitEmailService.cs            # Real SMTP email sending (prod)
│   │   │   ├── DevEmailService.cs                # Console email logging (dev)
│   │   │   ├── LocalFileStorageService.cs        # IFileStorageService impl - stores to uploads/ (Phase 4)
│   │   │   ├── SkiaSharpImageProcessingService.cs # IImageProcessingService impl - resize, convert, thumbnail (Phase 4)
│   │   │   ├── SkiaSharpImageScrambleService.cs  # IImageScrambleService impl - 8x8 Fisher-Yates scrambling (Phase 6)
│   │   │   ├── RedisViewTrackingService.cs       # IViewTrackingService impl - HyperLogLog unique viewers, daily aggregation (Phase 5)
│   │   │   └── DependencyInjection.cs   # Registers EF Core, Npgsql, interceptors, repos, auth, file, image, scramble, view tracking services
│   │
│   └── Manga.Api/                  # Layer 4: HTTP endpoints, middleware (depends on Application)
│       ├── Program.cs              # Minimal API host: DI, middleware, endpoint mapping, JWT config
│       ├── Middleware/
│       │   └── GlobalExceptionHandler.cs # Maps exceptions to RFC 9457 ProblemDetails
│       ├── Services/
│       │   └── CurrentUserService.cs     # Extracts user from HttpContext.User claims
│       ├── Endpoints/
│       │   ├── HealthEndpoints.cs        # GET /health endpoint
│       │   ├── AuthEndpoints.cs          # Auth endpoints (register, login, refresh, logout, verify-email, forgot-password, reset-password, me)
│       │   ├── MangaEndpoints.cs         # Manga CRUD & search endpoints (Phase 3)
│       │   ├── ChapterEndpoints.cs       # Chapter CRUD endpoints (Phase 3)
│       │   ├── GenreEndpoints.cs         # Genre listing endpoint (Phase 3)
│       │   ├── AttachmentEndpoints.cs    # File upload & serve endpoints (Phase 4)
│       │   └── ViewEndpoints.cs          # View tracking endpoint (Phase 5)
│       ├── .github/workflows/
│       │   └── ci.yml                    # CI/CD pipeline: .NET test, build, push (Phase 5)
│       └── Properties/
│           └── launchSettings.json       # http:5087, https:7123
│
└── tests/
    ├── Manga.Domain.Tests/         # xUnit, references Domain only
    ├── Manga.Application.Tests/    # xUnit, references Application + Domain
    └── Manga.Api.Tests/            # xUnit, references Api + Application + Domain
```

---

## Layer Dependencies

```
Domain
  ↑
  (no dependencies)

Application
  ↑
  (depends on Domain)

Infrastructure
  ↑
  (depends on Application)

Api (Presentation)
  ↑
  (depends on Application + Infrastructure)
```

**Rule**: Never reference Api in other layers. Dependency always flows inward.

---

## Key Files & Responsibilities

### Domain Layer (Manga.Domain)

| File | LOC | Purpose |
|------|-----|---------|
| Common/BaseEntity.cs | 20 | GUID Id, domain events collection |
| Common/AuditableEntity.cs | 20 | Timestamps, soft-delete flag |
| Common/ValueObject.cs | 15 | DDD value object base |
| Exceptions/DomainException.cs | 8 | Domain error base |
| Interfaces/IRepository.cs | 15 | Generic CRUD contract |
| Interfaces/IUnitOfWork.cs | 5 | Persistence contract |
| **Total** | ~83 | |

### Application Layer (Manga.Application)

| File | LOC | Purpose |
|------|-----|---------|
| Common/Interfaces/IAppDbContext.cs | 12 | DbContext abstraction |
| Common/Interfaces/IDateTimeProvider.cs | 8 | Clock service contract |
| Common/Interfaces/ICurrentUserService.cs | 6 | Auth user extraction |
| Common/Models/Result.cs | 25 | Operation result wrapper |
| Common/Models/PagedResponse.cs | 12 | Pagination container |
| Common/Behaviors/ValidationBehavior.cs | 20 | MediatR validation pipeline |
| Common/Behaviors/LoggingBehavior.cs | 15 | MediatR logging pipeline |
| DependencyInjection.cs | 25 | Service registration |
| **Total** | ~123 | |

### Infrastructure Layer (Manga.Infrastructure)

| File | LOC | Purpose |
|------|-----|---------|
| Persistence/AppDbContext.cs | 48 | EF Core context + soft-delete filter |
| Persistence/Repositories/BaseRepository.cs | 35 | IRepository<T> EF Core impl |
| Persistence/Interceptors/AuditableEntityInterceptor.cs | 20 | Auto-populate audit fields |
| Services/DateTimeProvider.cs | 8 | System clock |
| DependencyInjection.cs | 20 | Infrastructure registration |
| **Total** | ~131 | |

### Api Layer (Manga.Api)

| File | LOC | Purpose |
|------|-----|---------|
| Program.cs | 65 | Host builder, middleware, DI, JWT config |
| Middleware/GlobalExceptionHandler.cs | 25 | Exception → ProblemDetails mapping |
| Services/CurrentUserService.cs | 12 | ClaimsPrincipal extraction |
| Endpoints/HealthEndpoints.cs | 8 | GET /health endpoint |
| Endpoints/AuthEndpoints.cs | 85 | Auth endpoints (register, login, refresh, logout, verify-email, forgot-password, reset-password, me) |
| **Total** | ~195 | |

### Test Projects (Phase 3 Additions)

- **Manga.Domain.Tests**: Basic entity tests
- **Manga.Application.Tests**: Command/Query handler tests, FluentValidation tests
- **Manga.Api.Tests**: Integration tests for endpoints, auth tests (48 tests), manga/chapter/genre tests (160 total)

---

## Architectural Patterns

### 1. Clean Architecture
Strict layer separation with dependency inversion. Each layer has well-defined responsibilities.

### 2. CQRS via MediatR
- Commands modify state
- Queries retrieve data
- Handlers implement use cases
- Pipeline behaviors for cross-cutting concerns (validation, logging)

### 3. Repository Pattern
`IRepository<T>` with generic CRUD operations:
```csharp
GetByIdAsync(Guid id)
GetAllAsync(CancellationToken ct)
FindAsync(Expression<Func<T, bool>> predicate)
AddAsync(T entity)
UpdateAsync(T entity)
RemoveAsync(T entity)
```

### 4. Unit of Work Pattern
`IUnitOfWork.SaveChangesAsync()` coordinates entity persistence.

### 5. Domain-Driven Design
- **Entities**: AuditableEntity with identity
- **Value Objects**: Immutable, no identity (reserved structure)
- **Domain Events**: IDomainEvent (MediatR INotification)
- **Aggregates**: Reserved for future domain modeling

### 6. Soft Delete
Global EF Core query filter: `IsDeleted == false` auto-applied to AuditableEntity queries.

### 7. Audit Trail
AuditableEntityInterceptor auto-populates:
- CreatedAt / CreatedBy
- LastModifiedAt / LastModifiedBy
- DeletedAt (on soft delete)

### 8. Exception Mapping
GlobalExceptionHandler catches all exceptions → ProblemDetails (RFC 9457):
- DomainException → 400 Bad Request
- NotFoundException → 404 Not Found
- ValidationException → 422 Unprocessable Entity
- Other → 500 Internal Server Error

### 9. Validation Pipeline
FluentValidation integrated as MediatR behavior:
1. Request reaches ValidationBehavior
2. Validator.ValidateAsync() runs
3. If invalid → ValidationException thrown
4. GlobalExceptionHandler maps to 422 + error details

---

## NuGet Dependencies

### Core Framework
- **Microsoft.AspNetCore.App** (10.0.3): ASP.NET Core runtime
- **System.Reflection.Emit.Lightweight**: Dynamic code generation

### CQRS & Mediator
- **MediatR.Contracts** (2.0.1): IRequest/IRequestHandler contracts
- **MediatR** (14.0.0): Mediator pattern implementation

### Validation
- **FluentValidation** (12.1.1): Fluent validation API
- **FluentValidation.DependencyInjectionExtensions** (12.1.1): Auto-registration

### Persistence
- **Microsoft.EntityFrameworkCore** (10.0.3): ORM core
- **Microsoft.EntityFrameworkCore.Relational** (10.0.3): Relational db abstractions
- **Npgsql.EntityFrameworkCore.PostgreSQL** (10.0.0): PostgreSQL driver
- **Microsoft.EntityFrameworkCore.Design** (10.0.3): Migrations tooling

### Image Processing (Phase 4)
- **SkiaSharp** (3.x): High-performance image processing library (resize, format conversion, thumbnail generation)

### API & Documentation
- **Microsoft.AspNetCore.OpenApi** (10.0.3): OpenAPI schema generation
- **Scalar.AspNetCore** (2.12.39): Beautiful API docs UI (/scalar/v1)

### Testing
- **xunit** (2.9.3): Test framework
- **Microsoft.NET.Test.Sdk** (17.14.1): Test host
- **coverlet.collector** (6.0.4): Code coverage

---

## Build Configuration

### Directory.Build.props
```xml
<TargetFramework>net10.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

**Implications**:
- Nullable reference types mandatory (CS8632 warnings as errors)
- No `using System;` boilerplate required
- All types must explicit `?` for nullability

---

## Startup Sequence (Program.cs)

1. **DI Registration** (layers → services)
   - Application services (MediatR, validation)
   - Infrastructure services (EF Core, Npgsql, interceptors)
   - API services (OpenAPI, exception handler, HttpContext)

2. **Middleware Pipeline**
   - Exception handler (catches all exceptions)
   - Status code pages (standardized error responses)
   - HTTPS redirection
   - Development: OpenAPI + Scalar UI

3. **Endpoint Mapping**
   - Health endpoints
   - (Future: feature endpoints)

4. **Host Startup** (`app.Run()`)

---

## Database Configuration

**Provider**: PostgreSQL 14+ via Npgsql
**Connection String**: `configuration.GetConnectionString("Default")`
**EF Core Version**: 10.0.3

**Lifecycle**:
- Interceptor: Auto-populate audit fields
- Query Filter: Soft-delete filter auto-applied
- SaveChangesAsync: AppDbContext.SaveChangesAsync()

**Reserved Features**:
- Migrations (not yet created)
- Schema (awaiting entity modeling)

---

## Response Format

All API responses follow Result<T> wrapper:

```csharp
// Success
{
  "succeeded": true,
  "data": { /* payload */ },
  "message": "Operation completed successfully"
}

// Failure (validation)
{
  "succeeded": false,
  "data": null,
  "errors": [
    { "field": "Name", "message": "Required" }
  ]
}

// Failure (exception)
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "..."
}
```

---

## Health Check Endpoint

**GET /health**
```json
{
  "status": "healthy"
}
```

Used for load balancer checks, Kubernetes probes.

---

## Environment-Specific Config

| Setting | Development | Production |
|---------|-------------|-----------|
| HTTPS | localhost:7123 | Required |
| HTTP | localhost:5087 | Disabled |
| OpenAPI UI | /scalar/v1 | Disabled |
| Logging | Debug | Warning+ |
| Cors | Permissive | Restrictive |

---

## Key Statistics

| Layer | Files | LOC | Purpose |
|-------|-------|-----|---------|
| Domain | 30 | ~450 | 16 entities, 7 enums, base classes, auth interfaces |
| Application | 35+ | ~650 | 14 handlers (Phase 3), validators, behaviors, auth service |
| Infrastructure | 25+ | ~550 | AppDbContext, 16+ configs, 5 migrations, token/email/view tracking services |
| Api | 10 | ~420 | 8 endpoint groups (auth, manga, chapter, genre, views), middleware |
| **Total Source** | **105+** | **~2,300+** | |
| Tests | 3 | ~1,200+ | 160 total tests (48 auth, 55 Phase 3, 57 others) |

## Database Schema

**Entities**: 16 total
- **Auditable** (14): MangaSeries, Chapter, ChapterPage, AlternativeTitle, Genre, MangaGenre, Comment, CommentReaction, Bookmark, ReadingHistory, User, Person, Attachment
- **Non-Auditable** (1): ViewStat (performance-optimized for analytics)

**Migrations**: 6 applied
1. InitialSchema (Feb 13) — Core entities
2. AddPersonAndAttachment (Feb 13) — Authors/artists + file storage
3. AddViewStats (Feb 15) — Daily view aggregation
4. AddAuthEntities (Feb 15) — RefreshToken entity, auth-specific indexes
5. AddAttachmentThumbnails (Feb 16) — ThumbnailUrl, ThumbnailStoragePath for media thumbnails
6. AddAttachmentScrambleFields (Feb 17) — ScrambleSeed, ScrambleGridSize for anti-leak protection

---

## View Tracking Architecture (Phase 5)

**Components:**
- **IViewTrackingService** interface (Application layer abstraction)
- **RedisViewTrackingService** (Infrastructure layer) — Redis HyperLogLog for unique visitor counting, daily aggregation via EF Core
- **TrackViewCommand** with handler & validator
- **ViewEndpoints.cs** — POST /api/views/track (public endpoint)
- Redis TTL: 30 days per viewing window
- Anonymous viewers tracked via SHA256(IP + UserAgent)
- GetTrendingMangaQuery updated to rank by UniqueViewCount

**Performance:**
- HyperLogLog: ~12KB per 30 days (millions of visitors)
- ViewStat table: ~1KB per day per manga (unbloated)

## Chapter Page Anti-Leak Scrambling (Phase 6)

**Components:**
- **IImageScrambleService** interface (Application layer abstraction)
- **SkiaSharpImageScrambleService** (Infrastructure layer) — 8x8 tile-based Fisher-Yates shuffle using mulberry32 PRNG
- **UploadAttachmentCommand** updated to scramble chapter pages on upload
- **Attachment entity** new fields: `ScrambleSeed` (int), `ScrambleGridSize` (int, default 8)
- **AddAttachmentScrambleFields** migration adds nullable columns for backward compatibility
- Scrambling only applies to chapter page attachments, not covers/avatars/banners
- Mulberry32 PRNG seed ensures deterministic descrambling on frontend

**Security:**
- Prevents visual spoiler leakage via thumbnail caching
- Scramled images unreadable without seed + PRNG algorithm
- Transparent to authorized users via ScrambledPageCanvas component

---

## CI/CD Pipelines (Phase 5)

### GitHub Actions Workflows

#### Backend (.NET 10 + PostgreSQL 17 + Redis 7)
**File:** `manga-dotnet/.github/workflows/ci.yml`
- Trigger: push to main, pull_request
- Services: PostgreSQL 17, Redis 7 (docker containers)
- Steps:
  1. Checkout code
  2. Setup .NET 10
  3. Restore NuGet packages
  4. Build solution
  5. Run all xUnit tests
  6. Upload coverage reports
- Supports future Docker image build

#### Frontend (Node 22 + pnpm)
**File:** `web-manga/.github/workflows/ci.yml`
- Trigger: push to main, pull_request
- Node version: 22
- Package manager: pnpm (fast, lock-file first)
- Steps:
  1. Checkout code
  2. Setup Node 22
  3. Setup pnpm
  4. Install dependencies
  5. Run ESLint
  6. TypeScript build check
  7. Run Vitest unit tests
  8. Upload coverage reports
- Fast-fail on lint/build errors

---

**Generated**: 2026-02-17
**Version**: 1.6 (Phase 6: Chapter Page Anti-Leak Scrambling Complete)
