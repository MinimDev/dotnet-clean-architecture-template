using FluentValidation;

namespace CleanArchitecture.Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Validator untuk DeleteProductCommand
/// </summary>
public class DeleteProductCommandValidator: AbstractValidator<DeleteProductByIdCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotNull().WithMessage("Product id is required");
    }

}