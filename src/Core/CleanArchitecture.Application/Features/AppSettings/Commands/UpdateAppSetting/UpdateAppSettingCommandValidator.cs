using FluentValidation;

namespace CleanArchitecture.Application.Features.AppSettings.Commands.UpdateAppSetting;

/// <summary>
/// Validator untuk UpdateAppSettingCommand
/// </summary>
public class UpdateAppSettingCommandValidator : AbstractValidator<UpdateAppSettingCommand>
{
    public UpdateAppSettingCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Setting key is required.")
            .MaximumLength(200).WithMessage("Setting key must not exceed 200 characters.");

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Setting value cannot be null.")
            .MaximumLength(2000).WithMessage("Setting value must not exceed 2000 characters.");
    }
}
