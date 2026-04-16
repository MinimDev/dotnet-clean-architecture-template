using CleanArchitecture.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CleanArchitecture.Infrastructure.Identity.Services;

/// <summary>
/// Service untuk generate JWT access tokens dan refresh tokens
/// </summary>
public class TokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityDbContext _identityDbContext;

    public TokenService(
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        IdentityDbContext identityDbContext)
    {
        _configuration = configuration;
        _userManager = userManager;
        _identityDbContext = identityDbContext;
    }

    /// <summary>
    /// Generate short-lived JWT access token
    /// </summary>
    public async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Support both legacy ExpirationInDays and new AccessTokenExpiryMinutes
        var expiryMinutes = _configuration.GetValue<double?>("Jwt:AccessTokenExpiryMinutes")
            ?? _configuration.GetValue<double>("Jwt:ExpirationInDays", 7) * 24 * 60;

        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generate opaque refresh token, persist to DB, and return the token string
    /// </summary>
    public async Task<string> GenerateRefreshTokenAsync(string userId)
    {
        // Revoke existing active tokens for this user (single active session)
        var existingTokens = await _identityDbContext.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var old in existingTokens)
            old.IsRevoked = true;

        var expiryDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);

        var refreshToken = new RefreshToken
        {
            Token = GenerateSecureToken(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow
        };

        _identityDbContext.RefreshTokens.Add(refreshToken);
        await _identityDbContext.SaveChangesAsync();

        return refreshToken.Token;
    }

    /// <summary>
    /// Validate refresh token and issue new access token + refresh token (rotation)
    /// </summary>
    public async Task<(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiry)?> RefreshAsync(string refreshToken)
    {
        var storedToken = await _identityDbContext.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (storedToken == null || !storedToken.IsActive)
            return null;

        // Rotate: revoke old token
        storedToken.IsRevoked = true;

        // Generate new tokens
        var newAccessToken = await GenerateJwtToken(storedToken.User);
        var newRefreshTokenStr = GenerateSecureToken();

        var expiryDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);
        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenStr,
            UserId = storedToken.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow
        };

        _identityDbContext.RefreshTokens.Add(newRefreshToken);
        await _identityDbContext.SaveChangesAsync();

        return (newAccessToken, newRefreshTokenStr, newRefreshToken.ExpiresAt);
    }

    /// <summary>
    /// Revoke a specific refresh token (logout)
    /// </summary>
    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _identityDbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (storedToken == null || storedToken.IsRevoked)
            return false;

        storedToken.IsRevoked = true;
        await _identityDbContext.SaveChangesAsync();
        return true;
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
