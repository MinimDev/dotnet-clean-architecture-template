using Microsoft.AspNetCore.Authorization;

namespace CleanArchitecture.Infrastructure.Identity.Security;

/// <summary>
/// Atribut otorisasi deklaratif untuk memvalidasi hak akses (Permission) pada Controller atau Action
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class HasPermissionAttribute : AuthorizeAttribute
{
    public string Permission { get; }

    public HasPermissionAttribute(string permission) : base(policy: permission)
    {
        Permission = permission;
    }
}
