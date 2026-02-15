# manga-dotnet

A .NET 10 Clean Architecture REST API for manga platform management.

## Tech Stack

- **.NET 10** / ASP.NET Core Minimal API
- **Entity Framework Core 10** + PostgreSQL (Npgsql)
- **MediatR 14** (CQRS pattern)
- **FluentValidation 12** (input validation pipeline)
- **Scalar** (OpenAPI documentation UI)
- **xUnit** + coverlet (testing & coverage)

## Architecture

Clean Architecture with 4 layers:

```
Manga.Api            → HTTP endpoints, middleware, DI composition root
Manga.Infrastructure → EF Core, PostgreSQL, repository implementations
Manga.Application    → CQRS handlers, validators, pipeline behaviors
Manga.Domain         → Entities, value objects, domain events, interfaces
```

Dependencies flow inward: Api → Infrastructure → Application → Domain.

## Project Structure

```
manga-dotnet/
├── src/
│   ├── Manga.Domain/           # Core domain (zero external deps)
│   ├── Manga.Application/      # Use cases, CQRS, validation
│   ├── Manga.Infrastructure/   # EF Core, Npgsql, repositories
│   └── Manga.Api/              # Minimal API endpoints, middleware
├── tests/
│   ├── Manga.Domain.Tests/
│   ├── Manga.Application.Tests/
│   └── Manga.Api.Tests/
├── docs/                        # Project documentation
├── Directory.Build.props        # Global: net10.0, nullable, implicit usings
└── manga-dotnet.slnx            # Solution file
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- PostgreSQL 14+

### Setup

```bash
# Clone and navigate
cd manga-dotnet

# Set connection string
# Add to src/Manga.Api/appsettings.Development.json:
# "ConnectionStrings": { "Default": "Host=localhost;Database=manga;Username=postgres;Password=..." }

# Run
dotnet run --project src/Manga.Api

# API docs (dev only)
# http://localhost:5087/scalar/v1
```

### Testing

```bash
dotnet test
```

## Key Patterns

- **CQRS**: Commands/Queries dispatched via MediatR with pipeline behaviors
- **Repository + Unit of Work**: Generic `IRepository<T>` + `IUnitOfWork`
- **Soft Delete**: Global EF Core query filter on `IsDeleted`
- **Audit Trail**: Auto-populated timestamps via `AuditableEntityInterceptor`
- **Exception Handling**: `GlobalExceptionHandler` → RFC 9457 ProblemDetails

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | /health | Health check |
| - | /scalar/v1 | API docs (dev only) |

## Current Status

**Phase**: Foundation complete with domain entities in progress — 16 entities defined, 3 migrations applied.

See [docs/project-roadmap.md](docs/project-roadmap.md) for full roadmap.

## Documentation

- [Project Overview & PDR](docs/project-overview-pdr.md)
- [Codebase Summary](docs/codebase-summary.md)
- [Code Standards](docs/code-standards.md)
- [System Architecture](docs/system-architecture.md)
- [Project Roadmap](docs/project-roadmap.md)
