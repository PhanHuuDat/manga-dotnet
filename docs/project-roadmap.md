# Project Roadmap

## Overview

Manga-dotnet development roadmap spanning 6 phases over ~3-4 months. Each phase builds on the previous; dependencies tracked.

**Current Phase**: Foundation (100% Complete)
**Next Phase**: Domain Modeling (Estimated start: Week 2)

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

### Phase 2: Domain Modeling (In Planning)

**Status**: Pending
**Duration**: 2-3 sprints (6-9 weeks)
**Estimated Start**: 2026-02-20
**Estimated Completion**: 2026-04-10

**Goals**:
- Define all core business entities
- Model domain value objects
- Create domain events
- Establish repository interfaces
- Achieve 80%+ test coverage for domain logic

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
- [ ] All entities inherit from AuditableEntity
- [ ] All value objects defined and tested
- [ ] All domain events defined
- [ ] Repository interfaces created
- [ ] EF Core entity configurations written
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

### Phase 3: API Implementation

**Status**: Blocked (awaiting Phase 2)
**Duration**: 2 sprints (6 weeks)
**Estimated Start**: 2026-04-11
**Estimated Completion**: 2026-05-23

**Goals**:
- Implement all CRUD endpoints for domain entities
- Create MediatR handlers for all use cases
- Build search/filter/sort capabilities
- Establish pagination standard
- Achieve >90% API test coverage

**Endpoints to Implement**:

#### Manga Management
- POST /api/manga (CreateMangaCommand)
- GET /api/manga (ListMangaQuery, paginated, filterable)
- GET /api/manga/{id} (GetMangaQuery)
- PUT /api/manga/{id} (UpdateMangaCommand)
- DELETE /api/manga/{id} (DeleteMangaCommand)
- GET /api/manga/{id}/chapters (GetMangaChaptersQuery)

#### Chapter Management
- POST /api/manga/{mangaId}/chapters (CreateChapterCommand)
- GET /api/chapters/{id} (GetChapterQuery)
- PUT /api/chapters/{id} (UpdateChapterCommand)
- DELETE /api/chapters/{id} (DeleteChapterCommand)

#### User Library
- POST /api/library (AddMangaToLibraryCommand)
- GET /api/library (GetUserLibraryQuery, paginated)
- PUT /api/library/{id} (UpdateLibraryEntryCommand)
- DELETE /api/library/{id} (RemoveFromLibraryCommand)

#### Reading Progress
- POST /api/progress (RecordProgressCommand)
- GET /api/progress/{mangaId} (GetProgressQuery)

#### Search & Discovery
- GET /api/manga/search?q=... (SearchMangaQuery)
- GET /api/manga/trending (GetTrendingMangaQuery)
- GET /api/manga/genre/{genre} (GetMangaByGenreQuery)

**Acceptance Criteria**:
- [ ] All endpoints implement Result<T> wrapper
- [ ] All endpoints validated input
- [ ] OpenAPI documentation generated
- [ ] Integration tests for all endpoints (>90% coverage)
- [ ] p95 response time <200ms for GET, <500ms for POST/PUT/DELETE
- [ ] Pagination works consistently across all list endpoints
- [ ] Sorting/filtering tested thoroughly
- [ ] Error responses in ProblemDetails format

**Dependencies**:
- Phase 2: Domain entities complete
- Phase 2: Entity configurations complete

**Next**: → Phase 4: Authentication & Authorization

---

### Phase 4: Authentication & Authorization

**Status**: Blocked (awaiting Phase 3)
**Duration**: 1.5 sprints (5 weeks)
**Estimated Start**: 2026-05-24
**Estimated Completion**: 2026-06-27

**Goals**:
- Implement JWT authentication
- Enable user registration & login
- Establish role-based authorization
- Secure all endpoints appropriately
- Zero auth-related security vulnerabilities

**Components**:

#### AuthService
```csharp
public interface IAuthService
{
    Task<Result<AuthToken>> LoginAsync(LoginCommand cmd, CancellationToken ct);
    Task<Result<Guid>> RegisterAsync(RegisterCommand cmd, CancellationToken ct);
    Task<Result<AuthToken>> RefreshAsync(string refreshToken, CancellationToken ct);
    Task<Result> LogoutAsync(Guid userId, CancellationToken ct);
}
```

#### Endpoints
- POST /api/auth/register (RegisterCommand)
- POST /api/auth/login (LoginCommand)
- POST /api/auth/refresh (RefreshCommand)
- POST /api/auth/logout (LogoutCommand) [Authorized]
- GET /api/auth/me (GetCurrentUserQuery) [Authorized]

#### JWT Implementation
- Token expiry: 15 minutes
- Refresh token TTL: 7 days
- Claims: userId, username, roles
- Signing algorithm: HS256
- Secret: stored in KeyVault (prod) / appsettings (dev)

#### Authorization Policies
```csharp
[Authorize] // requires any authenticated user
public Task GetMyLibraryAsync(...)

[Authorize(Roles = "Admin")]
public Task DeleteMangaAsync(...)

[Authorize(Roles = "Admin,Moderator")]
public Task ApproveChapterAsync(...)
```

**Acceptance Criteria**:
- [ ] JWT tokens generated & validated correctly
- [ ] Token refresh works without re-authentication
- [ ] Password hashed with PBKDF2 or Argon2
- [ ] Email uniqueness enforced
- [ ] [Authorize] attributes protect endpoints
- [ ] Role-based authorization working
- [ ] Auth tests >95% coverage
- [ ] No security vulnerabilities (OWASP top 10)
- [ ] Audit logging for login/logout events

**Risks**:
- Token compromise (mitigation: short expiry, refresh rotation)
- Password storage (mitigation: industry-standard hashing)

**Dependencies**:
- Phase 3: API endpoints complete
- Phase 3: Current user extraction functional

**Next**: → Phase 5: Frontend Integration

---

### Phase 5: Frontend Integration

**Status**: Blocked (awaiting Phase 4)
**Duration**: 2 sprints (6 weeks)
**Estimated Start**: 2026-06-28
**Estimated Completion**: 2026-08-08

**Goals**:
- Enable single-page app (React/Vue/Angular) consumption
- Optimize API for frontend performance
- Implement image upload capability
- Add WebSocket for real-time updates
- Achieve <300ms p95 response time

**Features**:

#### CORS Configuration
```csharp
// Development: allow localhost:3000, localhost:5173
// Production: allow specific frontend domain only
```

#### File Upload (Cover Images)
- POST /api/manga/{id}/cover (multipart/form-data)
- Validate image type (PNG, JPG, WebP)
- Max size: 2MB
- Store in blob storage (Azure/S3)

#### WebSocket Support (Future)
- Real-time chapter notifications
- Live reading progress sync
- Chat/comments (if enabled)

#### Response Optimization
- Pagination: 20 items default
- Selective field inclusion via query params
- Compression: gzip enabled
- Caching headers: ETag, Last-Modified

#### Rate Limiting
- 100 requests/minute per IP (unauthenticated)
- 1000 requests/minute per user (authenticated)
- Cache popular queries (trending, top-rated)

**Acceptance Criteria**:
- [ ] Frontend can authenticate & consume API
- [ ] CORS configured correctly per environment
- [ ] File upload endpoint tested with real images
- [ ] Response times meet targets (<300ms p95)
- [ ] Caching strategy reduces database load 30%+
- [ ] Rate limiting prevents abuse
- [ ] Zero CORS-related bugs

**Dependencies**:
- Phase 4: Authentication complete
- Phase 4: Authorization working

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
| 2: Domain Modeling | Planned | 2026-02-20 | 2026-04-10 | 7 weeks |
| 3: API Implementation | Planned | 2026-04-11 | 2026-05-23 | 6 weeks |
| 4: Auth & Authz | Planned | 2026-05-24 | 2026-06-27 | 5 weeks |
| 5: Frontend Integration | Planned | 2026-06-28 | 2026-08-08 | 6 weeks |
| 6: Deployment & DevOps | Planned | 2026-08-09 | 2026-09-12 | 5 weeks |
| **Total** | | 2026-02-06 | 2026-09-12 | **33 weeks** |

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

### Phase 3 (API Implementation)
- [ ] All CRUD endpoints working
- [ ] API response time p95 <200ms
- [ ] OpenAPI docs complete & accurate
- [ ] >90% endpoint test coverage

### Phase 4 (Auth)
- [ ] JWT authentication working
- [ ] Authorization policies enforced
- [ ] Zero security vulnerabilities
- [ ] Auth test coverage >95%

### Phase 5 (Frontend Integration)
- [ ] React/Vue/Angular app can consume API
- [ ] Response time p95 <300ms
- [ ] File uploads working
- [ ] CORS configured per environment

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

**Document Version**: 1.0
**Last Updated**: 2026-02-13
**Created By**: Documentation Team
