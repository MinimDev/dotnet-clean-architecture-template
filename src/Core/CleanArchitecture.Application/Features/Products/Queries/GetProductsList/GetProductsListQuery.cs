using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Domain.Enums;
using MediatR;

namespace CleanArchitecture.Application.Features.Products.Queries.GetProductsList;

/// <summary>
///     Query untuk mendapatkan list products dengan pagination
/// </summary>
public record GetProductsListQuery : IRequest<Result<PaginatedList<ProductDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public ProductStatus? Status { get; init; }
    public string? SearchTerm { get; init; }
}

/// <summary>
///     DTO untuk Product
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}