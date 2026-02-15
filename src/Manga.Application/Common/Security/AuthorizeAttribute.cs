namespace Manga.Application.Common.Security;

/// <summary>
/// Marks a MediatR request as requiring authentication.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AuthorizeAttribute : Attribute;
