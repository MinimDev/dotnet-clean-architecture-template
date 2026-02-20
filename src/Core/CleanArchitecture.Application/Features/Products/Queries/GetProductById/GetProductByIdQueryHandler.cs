using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Handler untuk GetProductByIdQuery
/// </summary>
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProductDetailDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == request.Id)
                .Select(p => new ProductDetailDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    Status = p.Status.ToString(),
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy,
                    ModifiedAt = p.ModifiedAt,
                    ModifiedBy = p.ModifiedBy
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
            {
                return Result<ProductDetailDto>.Failure($"Product with ID {request.Id} not found");
            }

            return Result<ProductDetailDto>.Success(product);
        }
        catch (Exception ex)
        {
            return Result<ProductDetailDto>.Failure($"An error occurred while retrieving the product: {ex.Message}");
        }
    }
}
