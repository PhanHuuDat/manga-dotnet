# Codebase Summary: manga-dotnet

## Overview

Manga-dotnet is a .NET 10 Clean Architecture REST API with CQRS pattern for manga platform management. Foundational infrastructure complete; 16 domain entities defined with migrations applied.

**Total LOC (source only)**: ~850+ lines across 4 layers + 3 test projects (domain entities added)

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
│   │   │   │   ├── IAppDbContext.cs       # EF Core DbContext abstraction
│   │   │   │   ├── IDateTimeProvider.cs   # Testable clock service
│   │   │   │   └── ICurrentUserService.cs # Auth user info extraction
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs  # MediatR pipeline: FluentValidation
│   │   │   │   └── LoggingBehavior.cs     # MediatR pipeline: request/response logging
│   │   │   └── Models/
│   │   │       ├── PagedResponse.cs        # Pagination: Items[], PageNumber, TotalPages, HasNextPage
│   │   │       └── Result.cs               # Operation result: Success/Failure, Data, Errors
│   │   ├── Features/                # Empty (reserved for command/query folders)
│   │   └── DependencyInjection.cs   # Registers MediatR, FluentValidation, behaviors
│   │
│   ├── Manga.Infrastructure/       # Layer 3: External services, persistence (depends on Application)
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs      # EF Core context, implements IAppDbContext + IUnitOfWork
│   │   │   │                        # DbSets for all 16 entities, soft-delete filter
│   │   │   ├── Interceptors/
│   │   │   │   └── AuditableEntityInterceptor.cs # Populates CreatedAt/By, LastModifiedAt/By, DeletedAt
│   │   │   ├── Configurations/      # 14+ EF Core entity configurations
│   │   │   │   ├── MangaSeriesConfiguration.cs
│   │   │   │   ├── ChapterConfiguration.cs
│   │   │   │   ├── PersonConfiguration.cs
│   │   │   │   ├── AttachmentConfiguration.cs
│   │   │   │   ├── ViewStatConfiguration.cs (composite PK: TargetType, TargetId, ViewDate)
│   │   │   │   └── ...other entity configs
│   │   │   ├── Migrations/
│   │   │   │   ├── 20260215044212_InitialSchema.cs
│   │   │   │   ├── 20260215064500_AddPersonAndAttachment.cs
│   │   │   │   └── 20260215074337_AddViewStats.cs
│   │   │   └── Repositories/
│   │   │       └── BaseRepository.cs # EF Core IRepository<T> implementation
│   │   ├── Services/
│   │   │   ├── DateTimeProvider.cs           # System clock (UTC) implementation
│   │   │   ├── TokenService.cs               # JWT token generation/validation, refresh token rotation
│   │   │   ├── MailKitEmailService.cs        # Real SMTP email sending (prod)
│   │   │   └── DevEmailService.cs            # Console email logging (dev)
│   │   └── DependencyInjection.cs   # Registers EF Core, Npgsql, interceptors, repos, auth services
│   │
│   └── Manga.Api/                  # Layer 4: HTTP endpoints, middleware (depends on Application)
│       ├── Program.cs              # Minimal API host: DI, middleware, endpoint mapping, JWT config
│       ├── Middleware/
│       │   └── GlobalExceptionHandler.cs # Maps exceptions to RFC 9457 ProblemDetails
│       ├── Services/
│       │   └── CurrentUserService.cs     # Extracts user from HttpContext.User claims
│       ├── Endpoints/
│       │   ├── HealthEndpoints.cs        # GET /health endpoint
│       │   └── AuthEndpoints.cs          # Auth endpoints (register, login, refresh, logout, verify-email, forgot-password, reset-password, me)
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

### Test Projects

- **Manga.Domain.Tests**: ~10 LOC (stubs)
- **Manga.Application.Tests**: ~10 LOC (stubs)
- **Manga.Api.Tests**: ~10 LOC (stubs)

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
| Application | 18 | ~320 | Auth/email handlers, validation, behaviors, auth service |
| Infrastructure | 24+ | ~500 | AppDbContext, 14+ configs, 4 migrations, token/email services |
| Api | 6 | ~195 | Auth endpoints (8), health, middleware, auth config |
| **Total Source** | **78+** | **~1,465+** | |
| Tests | 3 | ~1,200+ | 48 auth tests (xUnit) |

## Database Schema

**Entities**: 16 total
- **Auditable** (14): MangaSeries, Chapter, ChapterPage, AlternativeTitle, Genre, MangaGenre, Comment, CommentReaction, Bookmark, ReadingHistory, User, Person, Attachment
- **Non-Auditable** (1): ViewStat (performance-optimized for analytics)

**Migrations**: 4 applied
1. InitialSchema (Feb 13) — Core entities
2. AddPersonAndAttachment (Feb 13) — Authors/artists + file storage
3. AddViewStats (Feb 15) — Daily view aggregation
4. AddAuthEntities (Feb 15) — RefreshToken entity, auth-specific indexes

---

**Generated**: 2026-02-15
**Version**: 1.2 (Authentication & Authorization Completed)
