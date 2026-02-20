using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Domain.Entities;
using MediatR;

using CleanArchitecture.Domain.Exceptions;

namespace CleanArchitecture.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler: IRequestHandler<UpdateProductCommandById, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateProductCommandById request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(request.Id, cancellationToken);
            
            if (product == null)
            {
                return Result.Failure($"Product with ID {request.Id} not found.");
            }

            // Update properties using domain methods
            product.UpdateDetails(request.Name, request.Description);
            product.UpdatePrice(request.Price);
            product.UpdateStock(request.Stock);

            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"An error occurred while updating the product: {ex.Message}");
        }
    }
}