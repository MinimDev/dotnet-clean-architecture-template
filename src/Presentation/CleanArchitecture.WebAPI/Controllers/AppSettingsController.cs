using Asp.Versioning;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Application.Features.AppSettings.Commands.UpdateAppSetting;
using CleanArchitecture.Application.Features.AppSettings.DTOs;
using CleanArchitecture.Application.Features.AppSettings.Queries.GetAppSettingByKey;
using CleanArchitecture.Application.Features.AppSettings.Queries.GetAppSettings;
using CleanArchitecture.Infrastructure.Identity.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CleanArchitecture.WebAPI.Controllers;

/// <summary>
/// API Controller untuk mengelola konfigurasi aplikasi dan Feature Flags secara dinamis
/// </summary>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("fixed")]
public class AppSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Mendapatkan daftar semua konfigurasi aplikasi (opsional filter berdasarkan grup).
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.AppSettings.View)]
    [ProducesResponseType(typeof(Result<List<AppSettingDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAppSettings([FromQuery] string? group = null)
    {
        var result = await _mediator.Send(new GetAppSettingsQuery(group));
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Mendapatkan detail konfigurasi aplikasi berdasarkan Key.
    /// </summary>
    [HttpGet("{key}")]
    [HasPermission(Permissions.AppSettings.View)]
    [ProducesResponseType(typeof(Result<AppSettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppSettingByKey(string key)
    {
        var result = await _mediator.Send(new GetAppSettingByKeyQuery(key));
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Memperbarui nilai konfigurasi aplikasi.
    /// </summary>
    [HttpPut("{key}")]
    [HasPermission(Permissions.AppSettings.Edit)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAppSetting(string key, [FromBody] UpdateAppSettingRequest body)
    {
        var result = await _mediator.Send(new UpdateAppSettingCommand(key, body.Value));
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}

/// <summary>
/// Request model untuk memperbarui nilai AppSetting
/// </summary>
public class UpdateAppSettingRequest
{
    public string Value { get; set; } = string.Empty;
}
