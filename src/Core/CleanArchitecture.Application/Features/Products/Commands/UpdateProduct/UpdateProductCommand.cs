using CleanArchitecture.Application.Common.Models;
using MediatR;

namespace CleanArchitecture.Application.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Command untuk update value by Id
/// </summary> 
/// <param name="Id"></param>
public record UpdateProductCommandById(Guid Id) : IRequest<Result>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int Stock { get; init; }
}
