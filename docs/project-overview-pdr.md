# Manga-DotNet: Project Overview & PDR

## Project Vision

A modern, scalable .NET Web API for manga platform management, built with Clean Architecture principles. Enables users to discover, organize, and track manga content with a robust backend foundation.

## Purpose

Build a foundational REST API that serves a manga reading/management platform with:
- User-centric content discovery and curation
- Manga catalog management (titles, chapters, metadata)
- Reading history and progress tracking
- Personal library/bookmarks management
- Support for future frontend and mobile applications

## Current Status: Domain Modeling Phase (In Progress)

**Scaffolding Complete**: Base layers implemented.
**Domain Entities**: 16 entities defined with migrations applied (InitialSchema, AddPersonAndAttachment, AddViewStats).
**Key Milestone**: Domain model complete; API implementation next.

---

## Product Development Requirements (PDR)

### Phase 1: Foundation & Scaffolding (Done)
- Clean Architecture with 4-layer design
- CQRS pattern via MediatR
- PostgreSQL with EF Core 10
- Validation & exception handling
- Domain event infrastructure

**Completion**: 100%

### Phase 2: Domain Modeling (In Progress - 70% Complete)
**Goal**: Define core business entities and their relationships.

**Completed**:
- [x] 16 entities: MangaSeries, Chapter, ChapterPage, AlternativeTitle, Genre, MangaGenre, Comment, CommentReaction, Bookmark, ReadingHistory, User, Person, Attachment, ViewStat
- [x] 6 enums: SeriesStatus, MangaBadge, ReactionType, UserRole, AttachmentType, ViewTargetType
- [x] EF Core configurations with snake_case naming & indexes
- [x] 3 migrations applied (InitialSchema, AddPersonAndAttachment, AddViewStats)
- [x] Soft-delete global filter on AuditableEntity
- [x] ViewStat anti-bloat strategy (daily time-bucketed aggregation)

**In Progress**:
- Repository interfaces & EF configurations for all entities
- Domain tests (targeting 80%+ coverage)

**Timeline**: 2-3 sprints (Extended to Q1 2026)

### Phase 3: API Implementation
**Goal**: Implement read/write endpoints for core domain.

**Requirements**:
- Manga CRUD endpoints
- Chapter management
- Library management endpoints
- Search & filtering capabilities
- Pagination support

**Acceptance Criteria**:
- All endpoints return consistent Result<T> response
- Validation feedback via ProblemDetails
- OpenAPI docs generated
- Integration tests for all endpoints
- Performance: <200ms for filtered list queries

**Timeline**: 2 sprints

### Phase 4: Authentication & Authorization
**Goal**: Secure platform with user authentication.

**Requirements**:
- JWT-based authentication
- User registration & login
- Role-based authorization
- Auth token refresh mechanism
- Secure password hashing

**Acceptance Criteria**:
- OAuth2/OpenID Connect ready
- Policy-based authorization working
- Audit trail of auth events
- All endpoints secured appropriately
- Rate limiting implemented

**Timeline**: 1.5 sprints

### Phase 5: Frontend Integration
**Goal**: Connect API with user-facing applications.

**Requirements**:
- CORS configuration per environment
- WebSocket support for real-time updates
- File upload for cover images
- Caching strategy (Redis)
- API versioning strategy

**Acceptance Criteria**:
- Single-page app can consume API
- Performance metrics: p95 <300ms
- Zero authentication-related bugs

**Timeline**: 2 sprints

### Phase 6: Deployment & DevOps
**Goal**: Production-ready infrastructure.

**Requirements**:
- Docker containerization
- CI/CD pipeline (GitHub Actions)
- Database migrations automation
- Health checks & monitoring
- Logging aggregation (ELK/Seq)

**Acceptance Criteria**:
- Automated deployments to staging/prod
- All tests pass in CI
- Zero-downtime deployment capable
- Metrics dashboard live

**Timeline**: 1.5 sprints

---

## Success Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| API Response Time | <200ms p95 | Datadog/Application Insights |
| Test Coverage | 80%+ | coverlet reports |
| Uptime | 99.9% | Status page |
| Feature Velocity | 1-2 features/sprint | Sprint board |
| Code Quality | Grade A | SonarQube |
| Security | Zero vulnerabilities | Dependabot + manual audit |

---

## Technical Constraints

- **.NET 10** only (no downgrade)
- **PostgreSQL 14+** (no MySQL)
- **Clean Architecture** strict adherence
- **No monolithic God services** (max 50 LOC per handler)
- **Domain-driven design** for all new entities
- **100% nullable reference types enabled**

---

## Dependencies & Integrations

| Dependency | Version | Purpose |
|------------|---------|---------|
| MediatR | 14.0.0 | CQRS/Command bus |
| FluentValidation | 12.1.1 | Input validation |
| EF Core | 10.0.3 | ORM |
| Npgsql | 10.0.0 | PostgreSQL driver |
| xUnit | 2.9.3 | Unit testing |
| Scalar | 2.12.39 | API docs UI |

---

## Team Roles & Responsibilities

- **Backend Lead**: Architecture decisions, code reviews
- **Domain Expert**: Domain modeling, business logic validation
- **QA Lead**: Test strategy, bug triage
- **DevOps**: CI/CD, monitoring, deployment

---

## Known Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|-----------|
| Scope creep during domain modeling | Delays Phase 3 | Strict sprint planning, feature freeze |
| PostgreSQL performance with large datasets | Read latency | Early indexing strategy, query profiling |
| Auth complexity (OAuth2) | Security gaps | External vendor review, pen testing |
| Schema migration downtime | Service interruption | Blue-green deployment strategy |

---

## Acceptance Criteria Checklist

- [ ] All phase requirements defined & estimated
- [ ] Domain entities designed & reviewed
- [ ] API contracts defined (OpenAPI)
- [ ] Deployment strategy documented
- [ ] Performance benchmarks established
- [ ] Security audit scheduled
- [ ] Team onboarded & trained

---

**Document Version**: 1.1
**Last Updated**: 2026-02-15
**Next Review**: End of Phase 2 (2026-04-10)
