using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Enums;
using CleanArchitecture.Domain.Events;
using CleanArchitecture.Domain.Exceptions;

namespace CleanArchitecture.Domain.Entities;

/// <summary>
/// Sample entity: Product
/// Contoh penggunaan: BaseAuditableEntity, ISoftDeletable, Business Logic, Domain Events
/// </summary>
public class Product : BaseAuditableEntity, ISoftDeletable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public ProductStatus Status { get; private set; }
    
    // ISoftDeletable implementation
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Private constructor untuk EF Core
    private Product()
    {
    }

    // Factory method untuk create product
    public static Product Create(string name, string? description, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty");

        if (price < 0)
            throw new DomainException("Product price cannot be negative");

        if (stock < 0)
            throw new DomainException("Product stock cannot be negative");

        var product = new Product
        {
            Name = name,
            Description = description,
            Price = price,
            Stock = stock,
            Status = ProductStatus.Draft
        };

        return product;
    }

    // Business methods
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new DomainException("Product price cannot be negative");

        Price = newPrice;
    }

    public void UpdateStock(int newStock)
    {
        if (newStock < 0)
            throw new DomainException("Product stock cannot be negative");

        Stock = newStock;
    }

    public void Activate()
    {
        if (Status == ProductStatus.Active)
            throw new DomainException("Product is already active");

        Status = ProductStatus.Active;
    }

    public void Deactivate()
    {
        if (Status == ProductStatus.Inactive)
            throw new DomainException("Product is already inactive");

        Status = ProductStatus.Inactive;
    }

    public void Discontinue()
    {
        Status = ProductStatus.Discontinued;
    }

    public void UpdateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty");

        Name = name;
        Description = description;
    }
}
