using AutoMapper;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile untuk mapping configurations
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product mappings akan ditambahkan ketika kita create DTOs
        // Contoh:
        // CreateMap<Product, ProductDto>();
        // CreateMap<CreateProductCommand, Product>();
    }
}
