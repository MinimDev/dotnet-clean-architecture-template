using CleanArchitecture.Application.Common.Models;
using Mediator;

namespace CleanArchitecture.Application.Features.Products.Commands.DeleteProduct;

/// <summary>
///     Command untuk menghapus product
/// </summary>
public record DeleteProductByIdCommand(Guid Id) : IRequest<Result>;
