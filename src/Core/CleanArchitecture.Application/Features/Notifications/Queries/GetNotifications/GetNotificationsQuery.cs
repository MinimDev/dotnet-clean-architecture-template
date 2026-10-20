using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Features.Notifications.DTOs;
using MediatR;

namespace CleanArchitecture.Application.Features.Notifications.Queries.GetNotifications;

/// <summary>
/// Query untuk mendapatkan daftar notifikasi pengguna dengan pagination dan filter status unread
/// </summary>
public record GetNotificationsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    bool? UnreadOnly = null) : IRequest<Result<PaginatedList<NotificationDto>>>;
