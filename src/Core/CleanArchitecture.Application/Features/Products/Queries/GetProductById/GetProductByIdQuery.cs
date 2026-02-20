using CleanArchitecture.Application.Common.Models;
using MediatR;

namespace CleanArchitecture.Application.Features.Products.Queries.GetProductById;

/// <summary>
///     Query untuk mendapatkan product by ID
/// </summary>
public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDetailDto>>;

/// <summary>
///     DTO untuk Product Detail
/// </summary>
public class ProductDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}