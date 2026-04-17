using Application.Common.Models;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Products;

public sealed class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForCtorParam(nameof(CategoryDto.Id), opt => opt.MapFrom(src => src.Id))
            .ForCtorParam(nameof(CategoryDto.Name), opt => opt.MapFrom(src => src.Name))
            .ForCtorParam(nameof(CategoryDto.Slug), opt => opt.MapFrom(src => src.Slug))
            .ForCtorParam(nameof(CategoryDto.Description), opt => opt.MapFrom(src => src.Description))
            .ForCtorParam(nameof(CategoryDto.ImageUrl), opt => opt.MapFrom(src => src.ImageUrl))
            .ForCtorParam(nameof(CategoryDto.IsActive), opt => opt.MapFrom(src => src.IsActive));

        CreateMap<ProductVariant, ProductVariantDto>()
            .ForCtorParam(nameof(ProductVariantDto.Id), opt => opt.MapFrom(src => src.Id))
            .ForCtorParam(nameof(ProductVariantDto.ProductId), opt => opt.MapFrom(src => src.ProductId))
            .ForCtorParam(nameof(ProductVariantDto.Sku), opt => opt.MapFrom(src => src.Sku))
            .ForCtorParam(nameof(ProductVariantDto.Name), opt => opt.MapFrom(src => src.Name))
            .ForCtorParam(nameof(ProductVariantDto.AttributeSummary), opt => opt.MapFrom(src => src.AttributeSummary))
            .ForCtorParam(nameof(ProductVariantDto.ImageUrl), opt => opt.MapFrom(src => src.ImageUrl))
            .ForCtorParam(nameof(ProductVariantDto.PriceOverride), opt => opt.MapFrom(src => src.PriceOverride))
            .ForCtorParam(nameof(ProductVariantDto.EffectivePrice), opt => opt.MapFrom(src => src.PriceOverride ?? (src.Product != null ? src.Product.BasePrice : 0m)))
            .ForCtorParam(nameof(ProductVariantDto.IsActive), opt => opt.MapFrom(src => src.IsActive));

        CreateMap<Product, ProductDto>()
            .ForCtorParam(nameof(ProductDto.Id), opt => opt.MapFrom(src => src.Id))
            .ForCtorParam(nameof(ProductDto.CategoryId), opt => opt.MapFrom(src => src.CategoryId))
            .ForCtorParam(nameof(ProductDto.CategoryName), opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForCtorParam(nameof(ProductDto.Sku), opt => opt.MapFrom(src => src.Sku))
            .ForCtorParam(nameof(ProductDto.Name), opt => opt.MapFrom(src => src.Name))
            .ForCtorParam(nameof(ProductDto.Slug), opt => opt.MapFrom(src => src.Slug))
            .ForCtorParam(nameof(ProductDto.Description), opt => opt.MapFrom(src => src.Description))
            .ForCtorParam(nameof(ProductDto.ImageUrl), opt => opt.MapFrom(src => src.ImageUrl))
            .ForCtorParam(nameof(ProductDto.BasePrice), opt => opt.MapFrom(src => src.BasePrice))
            .ForCtorParam(nameof(ProductDto.IsActive), opt => opt.MapFrom(src => src.IsActive))
            .ForCtorParam(
                nameof(ProductDto.Variants),
                opt => opt.MapFrom(src => src.Variants.OrderBy(variant => variant.Name).ToArray()));
    }
}
