using Asp.Versioning;
using CleanArchitecture.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.WebAPI.Controllers;

/// <summary>
/// API Controller for managing application users. (Admin Only)
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("fixed")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Gets a list of all registered users with their assigned roles.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers()
    {
        // 1. Ambil seluruh list tipe ApplicationUser (Bisa butuh pagination di masa depan)
        var users = await _userManager.Users.ToListAsync();
        
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FullName = user.FullName ?? "",
                Roles = roles.ToList()
            });
        }

        return Ok(userDtos);
    }

    /// <summary>
    /// Updates the main role of a specific user.
    /// In this boilerplate, we assume a user only has one primary role (Admin OR Member).
    /// </summary>
    [HttpPut("{email}/roles")]
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

        // Pastikan role yang dituju itu terdaftar di sistem
        var roleExists = await _roleManager.RoleExistsAsync(request.Role);
        if (!roleExists)
        {
            return BadRequest(new { Message = $"Role '{request.Role}' does not exist." });
        }

        // Kita hapus semua rolenya terlebih dahulu untuk menerapkan Single-Role Policy
        // (Bisa disesuaikan jika ingin Multi-Role)
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        // Assign role baru
        var addResult = await _userManager.AddToRoleAsync(user, request.Role);

        if (!addResult.Succeeded)
        {
            return BadRequest(new { Message = "Failed to update user role.", Errors = addResult.Errors.Select(e => e.Description) });
        }

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
    /// <summary>
    /// The exact Role name to assign to the user (e.g., "Admin" or "Member")
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
