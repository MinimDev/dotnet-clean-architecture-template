using Asp.Versioning;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Features.Notifications.Commands.MarkAllAsRead;
using CleanArchitecture.Application.Features.Notifications.Commands.MarkAsRead;
using CleanArchitecture.Application.Features.Notifications.DTOs;
using CleanArchitecture.Application.Features.Notifications.Queries.GetNotifications;
using CleanArchitecture.Application.Features.Notifications.Queries.GetUnreadCount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CleanArchitecture.WebAPI.Controllers;

/// <summary>
/// Controller untuk mengelola notifikasi in-app bagi pengguna yang sedang terotentikasi
/// </summary>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("fixed")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Mendapatkan daftar notifikasi untuk pengguna aktif dengan pagination dan filter status unread
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PaginatedList<NotificationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? unreadOnly = null)
    {
        var result = await _mediator.Send(new GetNotificationsQuery(pageNumber, pageSize, unreadOnly));
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Mendapatkan jumlah notifikasi yang belum dibaca (unread count)
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await _mediator.Send(new GetUnreadCountQuery());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Menandai satu notifikasi sebagai telah dibaca
    /// </summary>
    [HttpPatch("{id}/read")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await _mediator.Send(new MarkNotificationAsReadCommand(id));
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Menandai seluruh notifikasi milik pengguna sebagai telah dibaca
    /// </summary>
    [HttpPatch("read-all")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var result = await _mediator.Send(new MarkAllNotificationsAsReadCommand());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
