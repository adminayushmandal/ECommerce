using Application.Common.Models;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Stores;

public sealed class StoreMappingProfile : Profile
{
    public StoreMappingProfile()
    {
        CreateMap<Store, StoreDto>()
            .ForCtorParam(nameof(StoreDto.Id), opt => opt.MapFrom(src => src.Id))
            .ForCtorParam(nameof(StoreDto.Code), opt => opt.MapFrom(src => src.Code))
            .ForCtorParam(nameof(StoreDto.Name), opt => opt.MapFrom(src => src.Name))
            .ForCtorParam(nameof(StoreDto.AddressLine1), opt => opt.MapFrom(src => src.AddressLine1))
            .ForCtorParam(nameof(StoreDto.AddressLine2), opt => opt.MapFrom(src => src.AddressLine2))
            .ForCtorParam(nameof(StoreDto.City), opt => opt.MapFrom(src => src.City))
            .ForCtorParam(nameof(StoreDto.State), opt => opt.MapFrom(src => src.State))
            .ForCtorParam(nameof(StoreDto.Country), opt => opt.MapFrom(src => src.Country))
            .ForCtorParam(nameof(StoreDto.PostalCode), opt => opt.MapFrom(src => src.PostalCode))
            .ForCtorParam(nameof(StoreDto.Latitude), opt => opt.MapFrom(src => src.Latitude))
            .ForCtorParam(nameof(StoreDto.Longitude), opt => opt.MapFrom(src => src.Longitude))
            .ForCtorParam(nameof(StoreDto.IsActive), opt => opt.MapFrom(src => src.IsActive));
    }
}
