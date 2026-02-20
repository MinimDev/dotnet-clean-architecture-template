using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Products.Queries.GetProductsList;

/// <summary>
/// Handler untuk GetProductsListQuery
/// </summary>
public class GetProductsListQueryHandler : IRequestHandler<GetProductsListQuery, Result<PaginatedList<ProductDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<ProductDto>>> Handle(GetProductsListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Products.AsQueryable();

            // Filter by status
            if (request.Status.HasValue)
            {
                query = query.Where(p => p.Status == request.Status.Value);
            }

            // Search by name or description
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm))
                );
            }

            // Project to DTO
            var productDtos = query.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt
            });

            var paginatedList = await PaginatedList<ProductDto>.CreateAsync(
                productDtos,
                request.PageNumber,
                request.PageSize
            );

            return Result<PaginatedList<ProductDto>>.Success(paginatedList);
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<ProductDto>>.Failure($"An error occurred while retrieving products: {ex.Message}");
        }
    }
}
