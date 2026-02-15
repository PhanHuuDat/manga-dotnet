# Code Standards & Guidelines

## C# Style Guide

### Language Features

**Nullable Reference Types**: MANDATORY
- All string parameters must be `string` (not nullable) or `string?` (nullable)
- This is enforced via `<Nullable>enable</Nullable>` in Directory.Build.props
- Suppress warnings only with `#pragma warning disable ...` (justify inline)

```csharp
// Good: explicit nullability
public class User
{
    public string Name { get; set; } // Non-null
    public string? Bio { get; set; } // Can be null
}

// Bad: ignore nullability
public class User
{
    public string? Name { get; set; } // Should be non-null, always set
}
```

**Implicit Usings**: ENABLED
- No `using System;` required
- Common namespaces auto-imported
- Explicitly add uncommon namespaces only

**Target Framework**: .NET 10.0 (no downgrades)
- Use modern C# 14 features (records, required init, collection expressions)
- Avoid obsolete patterns (ConfigureAwait not needed)

---

### Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | PascalCase, hierarchical | `Manga.Domain.Entities` |
| Class/Record | PascalCase | `UserAggregate`, `CreateUserCommand` |
| Interface | PascalCase with `I` prefix | `IRepository<T>`, `IUnitOfWork` |
| Method | PascalCase | `GetUserById`, `ValidateEmail` |
| Property | PascalCase | `FirstName`, `IsActive` |
| Private field | camelCase with `_` prefix | `_domainEvents`, `_dateTimeProvider` |
| Parameter | camelCase | `userId`, `cancellationToken` |
| Constant | PascalCase | `DefaultPageSize = 20` |
| Local variable | camelCase | `isValid`, `userCount` |
| Generic type | PascalCase | `T`, `TKey`, `TResult` |

```csharp
public class UserRepository : IRepository<User>
{
    private readonly IAppDbContext _dbContext;
    private const int DefaultPageSize = 20;

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FindAsync([userId], cancellationToken: ct);
        return user;
    }
}
```

---

### File Organization

**File-per-type rule**: One public class/interface/record per file, named after type.

```
src/Manga.Domain/
├── Entities/
│   ├── User.cs           # public class User
│   ├── Manga.cs          # public class Manga
│   └── Chapter.cs        # public class Chapter
├── ValueObjects/
│   ├── Rating.cs         # public record Rating
│   └── SeriesStatus.cs   # public enum SeriesStatus
├── Interfaces/
│   └── IRepository.cs    # public interface IRepository<T>
```

**File Header**: Include namespace, then class definition (no extra spacing).

```csharp
namespace Manga.Domain.Entities;

public class User : AuditableEntity
{
    public required string Name { get; set; }
    public string? Bio { get; set; }
}
```

---

### Class & Record Design

**Entity Guidelines**:
- Inherit from `BaseEntity` (GUID Id) or `AuditableEntity` (timestamps + soft-delete)
- Use `required` for mandatory properties
- Use `?` for optional properties
- Leverage init-only setters where mutations aren't needed

```csharp
public class Manga : AuditableEntity
{
    public required string Title { get; set; }
    public required string Author { get; set; }
    public string? Description { get; set; }
    public SeriesStatus Status { get; set; }
    public DateOnly ReleaseDate { get; init; }

    public void UpdateStatus(SeriesStatus newStatus)
    {
        Status = newStatus;
    }
}
```

**Value Object Guidelines**:
- Use `public record` for immutability
- Implement structural equality (records do this automatically)
- No public setters

```csharp
public record Rating(int Value, int MaxValue = 10)
{
    public bool IsValid => Value >= 0 && Value <= MaxValue;
}
```

---

### Method Guidelines

**Async Methods**:
- Return `Task` (void) or `Task<T>` (with result)
- Always include `CancellationToken ct = default` parameter
- Name async methods with `Async` suffix
- Use `await` consistently, no `.Result` or `.Wait()`

```csharp
public async Task<User> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
{
    var user = await _repository.GetByIdAsync(userId, ct);
    return user ?? throw new NotFoundException(nameof(User), userId);
}

public async Task SendEmailAsync(string email, CancellationToken ct = default)
{
    await _emailService.SendAsync(email, ct);
}
```

**Method Signature Best Practices**:
- Max 4 parameters; use objects for more
- Place cancellation token last
- Use explicit return types (no `var` for public methods)

```csharp
// Good
public async Task<PagedResponse<User>> SearchAsync(
    string searchTerm,
    int pageNumber,
    int pageSize,
    CancellationToken ct = default)
{
    // ...
}

// Bad: too many params
public async Task<PagedResponse<User>> SearchAsync(
    string searchTerm, int pageNumber, int pageSize,
    string sortBy, bool ascending, int maxResults, CancellationToken ct)
```

---

### Exception Handling

**Custom Exceptions**:
- Inherit from `DomainException` or `ApplicationException`
- Use `NotFoundException` for entity lookups
- Include meaningful error messages

```csharp
public class MangaNotFoundException : NotFoundException
{
    public MangaNotFoundException(Guid mangaId)
        : base($"Manga with ID '{mangaId}' not found.")
    {
    }
}

// Usage
public async Task<Manga> GetMangaAsync(Guid id, CancellationToken ct = default)
{
    var manga = await _repository.GetByIdAsync(id, ct);
    return manga ?? throw new MangaNotFoundException(id);
}
```

**Try-Catch Usage**:
- Only catch exceptions you can handle
- Never swallow exceptions silently
- Rethrow with additional context if needed

```csharp
// Good: catch specific exception, log, rethrow
try
{
    await _emailService.SendAsync(email, ct);
}
catch (EmailException ex)
{
    _logger.LogError(ex, "Failed to send email to {Email}", email);
    throw new ApplicationException("Email delivery failed", ex);
}

// Bad: catch all
try { /* ... */ }
catch (Exception) { }
```

---

### Constructor Guidelines

**Dependency Injection**:
- Use constructor injection only
- Mark dependencies as `private readonly`
- Use primary constructors when appropriate (C# 12+)

```csharp
// C# 12+ style (preferred)
public class UserService(IRepository<User> userRepository, ILogger<UserService> logger)
{
    public async Task<User> GetUserAsync(Guid id, CancellationToken ct)
    {
        return await userRepository.GetByIdAsync(id, ct);
    }
}

// Traditional style (also acceptable)
public class UserService
{
    private readonly IRepository<User> _userRepository;

    public UserService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }
}
```

---

### Code Comments & Documentation

**XML Documentation (for public APIs)**:
```csharp
/// <summary>
/// Retrieves a manga by its unique identifier.
/// </summary>
/// <param name="id">The manga ID.</param>
/// <param name="ct">Cancellation token.</param>
/// <returns>The manga, or null if not found.</returns>
/// <exception cref="ArgumentException">If id is empty.</exception>
public async Task<Manga?> GetMangaAsync(Guid id, CancellationToken ct = default)
{
    ArgumentException.ThrowIfEqual(id, Guid.Empty);
    return await _repository.GetByIdAsync(id, ct);
}
```

**Inline Comments** (use sparingly; prefer self-documenting code):
```csharp
// Good: explains why, not what
public async Task RefreshAsync(CancellationToken ct = default)
{
    // Clear cache before refresh to prevent stale reads during rate limit window
    _cache.Clear();
    await _dataService.SyncAsync(ct);
}

// Bad: explains the obvious
public string GetName()
{
    // Return the name
    return _name;
}
```

---

## Architecture Standards

### Layer Responsibilities

| Layer | Responsibility | Dependencies |
|-------|----------------|--------------|
| **Domain** | Business rules, entities, interfaces | None |
| **Application** | Use cases, commands, queries, validation | Domain |
| **Infrastructure** | Persistence, external services | Application |
| **Api** | HTTP endpoints, middleware | Application, Infrastructure |

**Golden Rule**: Never skip layers. Always depend inward.

### Clean Architecture Rules

1. **No circular dependencies**: A → B → A is forbidden
2. **No cross-layer queries**: Don't query Infrastructure from Domain
3. **No framework leakage**: Entity Framework types never escape Infrastructure
4. **Explicit contracts**: Use interfaces for external access
5. **Dependency inversion**: High-level modules don't depend on low-level details

### CQRS Pattern

**Commands** (write operations):
- One class per operation
- Inherit from `IRequest` (MediatR)
- Implement `IRequestHandler<TCommand, TResponse>`
- Use `[FluentValidation.Attributes.Validator]` for validation

```csharp
public record CreateUserCommand(string Name, string Email)
    : IRequest<Result<Guid>>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IRepository<User> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<Guid>> Handle(
        CreateUserCommand request,
        CancellationToken ct)
    {
        var user = new User { Name = request.Name, Email = request.Email };
        await _repository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success(user.Id);
    }
}
```

**Queries** (read operations):
- One class per read operation
- Inherit from `IRequest<TResponse>`
- Implement `IRequestHandler<TQuery, TResponse>`

```csharp
public record GetUserQuery(Guid Id) : IRequest<Result<UserDto>>;

public class GetUserQueryHandler
    : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    private readonly IRepository<User> _repository;

    public async Task<Result<UserDto>> Handle(
        GetUserQuery request,
        CancellationToken ct)
    {
        var user = await _repository.GetByIdAsync(request.Id, ct);
        if (user == null)
            return Result.Failure<UserDto>("User not found");

        return Result.Success(new UserDto(user.Id, user.Name));
    }
}
```

### Repository Pattern

**Generic Repository Interface** (already defined):
```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task RemoveAsync(T entity);
}
```

**Concrete Repository Implementation**:
```csharp
public class UserRepository(AppDbContext dbContext)
    : BaseRepository<User>(dbContext), IRepository<User>
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await DbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }
}
```

### Entity Lifecycle

**Create**:
```csharp
var user = new User { Name = "John", Email = "john@example.com" };
await _repository.AddAsync(user);
await _unitOfWork.SaveChangesAsync(ct);
// AuditableEntityInterceptor auto-populates CreatedAt, CreatedBy
```

**Update**:
```csharp
user.Name = "Jane";
await _repository.UpdateAsync(user);
await _unitOfWork.SaveChangesAsync(ct);
// Interceptor auto-populates LastModifiedAt, LastModifiedBy
```

**Soft Delete**:
```csharp
user.IsDeleted = true;
await _repository.UpdateAsync(user);
await _unitOfWork.SaveChangesAsync(ct);
// Global filter: future queries skip deleted entities
```

---

## Testing Standards

### Unit Testing

**Test File Naming**: `{ClassUnderTest}Tests.cs`
```
src/Manga.Domain.Tests/
├── Entities/
│   └── UserTests.cs
├── ValueObjects/
│   └── RatingTests.cs
```

**Test Method Naming**: `{Method}_{Scenario}_{Expected}`
```csharp
[Fact]
public void UpdateStatus_WithValidStatus_UpdatesStatusSuccessfully()
{
    // Arrange
    var manga = new Manga { Title = "One Piece" };

    // Act
    manga.UpdateStatus(SeriesStatus.OnGoing);

    // Assert
    Assert.Equal(SeriesStatus.OnGoing, manga.Status);
}
```

**AAA Pattern** (Arrange-Act-Assert):
```csharp
[Theory]
[InlineData(0)]
[InlineData(11)]
public void Rating_WithInvalidValue_ThrowsException(int value)
{
    // Arrange
    var maxValue = 10;

    // Act & Assert
    Assert.Throws<ArgumentException>(() => new Rating(value, maxValue));
}
```

**Coverage Target**: 80%+ for new code

---

## Performance Standards

### Query Optimization

- **Index frequently-filtered columns**: CreatedAt, IsDeleted, Foreign Keys
- **Eager load related entities**: Use `.Include()` for navigation properties
- **Avoid N+1 queries**: Load collections explicitly, not in loops

```csharp
// Bad: N+1 queries
var users = await _repository.GetAllAsync(ct);
foreach (var user in users)
{
    var mangaList = await _mangaRepository.FindAsync(m => m.UserId == user.Id, ct);
}

// Good: single query with Include
var users = await _dbContext.Users
    .Include(u => u.Manga)
    .ToListAsync(ct);
```

### Response Times

- **API responses**: Target p95 < 200ms
- **Database queries**: Profile queries > 100ms
- **List endpoints**: Always paginate (max 100 items)

### Caching

- Use Redis for frequently-accessed, rarely-modified data
- TTL: 1 hour for user data, 24 hours for catalog data
- Invalidate cache on mutations

---

## Security Standards

### Input Validation

- Validate all API inputs via FluentValidation
- Reject XSS payloads in text fields
- Sanitize file uploads

```csharp
public class CreateMangaCommandValidator : AbstractValidator<CreateMangaCommand>
{
    public CreateMangaCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .Matches(@"^[a-zA-Z0-9\s\-:',\.]+$"); // No HTML

        RuleFor(x => x.FileName)
            .NotEmpty()
            .Matches(@"^[a-zA-Z0-9\._\-]+$");
    }
}
```

### Authentication & Authorization

- Use JWT tokens with short expiry (15 minutes)
- Refresh tokens: 7 days
- Always include `[Authorize]` on protected endpoints

```csharp
[HttpGet("{id}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> GetUser(Guid id, CancellationToken ct)
{
    // ...
}
```

### Sensitive Data

- Never log passwords, tokens, or PII
- Use `[SensitiveData]` attribute on DTOs containing sensitive info
- Mask credit card numbers in logs

```csharp
[Serializable]
public class LoginCommand : IRequest<Result<AuthToken>>
{
    public string Email { get; set; }

    [SensitiveData]
    public string Password { get; set; }
}
```

---

## Deployment Standards

### Build Configuration

- Target: net10.0
- Configuration: Debug/Release
- Nullable: enable
- TreatWarningsAsErrors: true (in Release)

### Connection Strings

- Never commit connection strings
- Use environment variables: `MANGA_CONNECTION_STRING`
- Prod: Use Azure KeyVault or AWS Secrets Manager

```csharp
var connectionString = configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string not found");
```

### Logging

- Min level: Debug (Dev), Warning (Prod)
- Include correlation IDs for request tracing
- Log exceptions with full stack traces

---

**Version**: 1.0
**Last Updated**: 2026-02-13
