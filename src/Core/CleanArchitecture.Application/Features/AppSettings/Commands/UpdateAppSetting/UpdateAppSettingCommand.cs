using System.Globalization;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.AppSettings.Commands.UpdateAppSetting;

/// <summary>
/// Command untuk memperbarui nilai konfigurasi AppSetting
/// </summary>
public record UpdateAppSettingCommand(string Key, string Value) : IRequest<Result>;

/// <summary>
/// Handler untuk UpdateAppSettingCommand
/// </summary>
public class UpdateAppSettingCommandHandler : IRequestHandler<UpdateAppSettingCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateAppSettingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateAppSettingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var setting = await _context.AppSettings
                .FirstOrDefaultAsync(s => s.Key == request.Key, cancellationToken);

            if (setting == null)
            {
                return Result.Failure($"Setting with key '{request.Key}' was not found.");
            }

            if (setting.IsReadOnly)
            {
                return Result.Failure($"Setting '{request.Key}' is read-only and cannot be modified.");
            }

            // Validasi format sesuai DataType
            switch (setting.DataType.ToLowerInvariant())
            {
                case "boolean":
                    if (!bool.TryParse(request.Value, out _))
                    {
                        return Result.Failure($"Invalid boolean value '{request.Value}' for setting '{request.Key}'. Expected 'true' or 'false'.");
                    }
                    break;

                case "number":
                    if (!decimal.TryParse(request.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    {
                        return Result.Failure($"Invalid numeric value '{request.Value}' for setting '{request.Key}'.");
                    }
                    break;
            }

            setting.UpdateValue(request.Value);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to update setting '{request.Key}': {ex.Message}");
        }
    }
}
