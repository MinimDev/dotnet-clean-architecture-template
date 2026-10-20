using Microsoft.AspNetCore.Authorization;

namespace CleanArchitecture.Infrastructure.Identity.Security;

/// <summary>
/// Requirement otorisasi yang merepresentasikan sebuah hak akses spesifik (permission)
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
