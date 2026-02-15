namespace Manga.Application.Common.Security;

/// <summary>
/// Marks a MediatR request as requiring a specific permission.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute(string permission) : AuthorizeAttribute
{
    public string Permission { get; } = permission;
}
