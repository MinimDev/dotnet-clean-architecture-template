using Microsoft.AspNetCore.Authorization;

namespace CleanArchitecture.Infrastructure.Identity.Security;

/// <summary>
/// AuthorizationHandler yang memverifikasi apakah pengguna memiliki claim permission yang dibutuhkan,
/// atau memiliki role "Admin" sebagai superadmin bypass.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User == null)
        {
            return Task.CompletedTask;
        }

        // Role "Admin" memiliki akses penuh ke seluruh fitur (Superadmin bypass)
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Evaluasi apakah pengguna memiliki claim permission yang sesuai
        var hasPermission = context.User.Claims.Any(c =>
            string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(c.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
