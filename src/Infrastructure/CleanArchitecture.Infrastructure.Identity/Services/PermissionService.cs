using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CleanArchitecture.Infrastructure.Identity.Services;

/// <summary>
/// Service untuk mengelola pemetaan Role-Permission dengan caching in-memory
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IdentityDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public PermissionService(
        IdentityDbContext context,
        UserManager<ApplicationUser> userManager,
        IMemoryCache cache)
    {
        _context = context;
        _userManager = userManager;
        _cache = cache;
    }

    public async Task<IReadOnlyList<string>> GetPermissionsForRoleAsync(string roleName, CancellationToken ct = default)
    {
        var cacheKey = $"role_perms_{roleName.ToLowerInvariant()}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = CacheDuration;

            // Role Admin selalu memiliki seluruh permission sistem secara default
            if (string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var dbPerms = await _context.RolePermissions
                    .Where(rp => rp.RoleName == roleName)
                    .Select(rp => rp.Permission)
                    .ToListAsync(ct);

                return (IReadOnlyList<string>)Permissions.GetAll().Union(dbPerms).Distinct().ToList();
            }

            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleName == roleName)
                .Select(rp => rp.Permission)
                .ToListAsync(ct);

            return (IReadOnlyList<string>)permissions;
        }) ?? Array.Empty<string>();
    }

    public async Task<IReadOnlyList<string>> GetPermissionsForUserAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Array.Empty<string>();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var role in roles)
        {
            var rolePerms = await GetPermissionsForRoleAsync(role, ct);
            foreach (var perm in rolePerms)
            {
                userPermissions.Add(perm);
            }
        }

        return userPermissions.ToList();
    }

    public async Task UpdateRolePermissionsAsync(string roleName, IEnumerable<string> permissions, CancellationToken ct = default)
    {
        var existing = await _context.RolePermissions
            .Where(rp => rp.RoleName == roleName)
            .ToListAsync(ct);

        _context.RolePermissions.RemoveRange(existing);

        var distinctPerms = permissions.Distinct(StringComparer.OrdinalIgnoreCase);
        foreach (var perm in distinctPerms)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleName = roleName,
                Permission = perm
            });
        }

        await _context.SaveChangesAsync(ct);

        // Invalidate cache
        _cache.Remove($"role_perms_{roleName.ToLowerInvariant()}");
    }

    public Task<IReadOnlyDictionary<string, List<string>>> GetAllPermissionsGroupedAsync(CancellationToken ct = default)
    {
        return Task.FromResult(Permissions.GetGrouped());
    }
}
