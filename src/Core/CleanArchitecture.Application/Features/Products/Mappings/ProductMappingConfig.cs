using CleanArchitecture.Application.Features.Products.Queries.GetProductsList;
using CleanArchitecture.Domain.Entities;
using Mapster;

namespace CleanArchitecture.Application.Features.Products.Mappings;

public class ProductMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDto>()
            // Contoh custom mapping: menggabungkan string atau memformat angka
            .Map(dest => dest.PriceDisplay, src => $"Rp {src.Price:N0}")
            // Jika ada properti lain yang butuh logika khusus, bisa ditambahkan di sini
            // Mapster akan otomatis memetakan properti lain yang namanya sama (Id, Name, Stock, dll)
            .IgnoreNullValues(true);

        config.NewConfig<Product, CleanArchitecture.Application.Features.Products.Queries.GetProductById.ProductDetailDto>()
            .Map(dest => dest.PriceDisplay, src => $"Rp {src.Price:N0}")
            .IgnoreNullValues(true);
    }
}
