using Asp.Versioning;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Infrastructure.Identity.Models;
using CleanArchitecture.Infrastructure.Identity.Security;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.WebAPI.Controllers;

/// <summary>
/// API Controller for managing application users, roles, and permissions.
/// </summary>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("fixed")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IPermissionService _permissionService;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IPermissionService permissionService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _permissionService = permissionService;
    }

    /// <summary>
    /// Gets a list of all registered users with their assigned roles.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var userDtos = users.Adapt<List<UserDto>>();

        foreach (var dto in userDtos)
        {
            var user = users.First(u => u.Id == dto.Id);
            var roles = await _userManager.GetRolesAsync(user);
            dto.Roles = roles.ToList();
        }

        return Ok(userDtos);
    }

    /// <summary>
    /// Updates the main role of a specific user.
    /// </summary>
    [HttpPut("{email}/roles")]
    [HasPermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRole(string email, [FromBody] UpdateUserRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Role))
        {
             return BadRequest(new { Message = "Role must be specified." });
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }

        var roleExists = await _roleManager.RoleExistsAsync(request.Role);
        if (!roleExists)
        {
            return BadRequest(new { Message = $"Role '{request.Role}' does not exist." });
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        var addResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!addResult.Succeeded)
        {
            return BadRequest(new { Message = "Failed to update user role.", Errors = addResult.Errors.Select(e => e.Description) });
        }

        return NoContent();
    }

    /// <summary>
    /// Gets all system permissions grouped by category/module.
    /// </summary>
    [HttpGet("permissions")]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(IReadOnlyDictionary<string, List<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPermissions()
    {
        var permissions = await _permissionService.GetAllPermissionsGroupedAsync();
        return Ok(permissions);
    }

    /// <summary>
    /// Gets all permissions assigned to a specific role.
    /// </summary>
    [HttpGet("roles/{roleName}/permissions")]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolePermissions(string roleName)
    {
        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            return NotFound(new { Message = $"Role '{roleName}' does not exist." });
        }

        var permissions = await _permissionService.GetPermissionsForRoleAsync(roleName);
        return Ok(permissions);
    }

    /// <summary>
    /// Updates permissions assigned to a specific role.
    /// </summary>
    [HttpPut("roles/{roleName}/permissions")]
    [HasPermission(Permissions.Users.ManagePermissions)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRolePermissions(string roleName, [FromBody] UpdateRolePermissionsRequest request)
    {
        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            return NotFound(new { Message = $"Role '{roleName}' does not exist." });
        }

        await _permissionService.UpdateRolePermissionsAsync(roleName, request.Permissions);
        return NoContent();
    }
}

/// <summary>
/// Data Transfer Object representing an Application User
/// </summary>
public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
}

/// <summary>
/// Request model for updating user role.
/// </summary>
public class UpdateUserRoleRequest
{
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating role permissions.
/// </summary>
public class UpdateRolePermissionsRequest
{
    public List<string> Permissions { get; set; } = new();
}
