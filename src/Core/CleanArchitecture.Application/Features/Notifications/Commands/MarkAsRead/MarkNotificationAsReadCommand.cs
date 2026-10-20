using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Notifications.Commands.MarkAsRead;

/// <summary>
/// Command untuk menandai satu notifikasi sebagai telah dibaca
/// </summary>
public record MarkNotificationAsReadCommand(Guid Id) : IRequest<Result>;

/// <summary>
/// Handler untuk MarkNotificationAsReadCommand
/// </summary>
public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MarkNotificationAsReadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

            if (notification == null)
            {
                return Result.Failure($"Notification with ID '{request.Id}' was not found.");
            }

            // Validasi otorisasi kepemilikan notifikasi
            if (notification.UserId != null && notification.UserId != userId)
            {
                return Result.Failure("You are not authorized to modify this notification.");
            }

            notification.MarkAsRead();
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to mark notification as read: {ex.Message}");
        }
    }
}
