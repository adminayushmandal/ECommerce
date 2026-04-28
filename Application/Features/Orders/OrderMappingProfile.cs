using Application.Common.Models;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Orders;

public sealed class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<OrderItem, OrderItemDto>()
            .ForCtorParam(nameof(OrderItemDto.Id), opt => opt.MapFrom(src => src.Id))
            .ForCtorParam(nameof(OrderItemDto.ProductId), opt => opt.MapFrom(src => src.ProductId))
            .ForCtorParam(nameof(OrderItemDto.ProductVariantId), opt => opt.MapFrom(src => src.ProductVariantId))
            .ForCtorParam(nameof(OrderItemDto.ProductName), opt => opt.MapFrom(src => src.ProductName))
            .ForCtorParam(nameof(OrderItemDto.VariantName), opt => opt.MapFrom(src => src.VariantName))
            .ForCtorParam(nameof(OrderItemDto.Quantity), opt => opt.MapFrom(src => src.Quantity))
            .ForCtorParam(nameof(OrderItemDto.UnitPrice), opt => opt.MapFrom(src => src.UnitPrice))
            .ForCtorParam(nameof(OrderItemDto.LineTotal), opt => opt.MapFrom(src => src.LineTotal));

        CreateMap<Order, OrderDto>()
            .ForCtorParam(nameof(OrderDto.Id), opt => opt.MapFrom(src => src.Id))
            .ForCtorParam(nameof(OrderDto.OrderNumber), opt => opt.MapFrom(src => src.OrderNumber))
            .ForCtorParam(nameof(OrderDto.UserId), opt => opt.MapFrom(src => src.UserId))
            .ForCtorParam(nameof(OrderDto.CustomerEmail), opt => opt.MapFrom(src => src.CustomerEmail))
            .ForCtorParam(nameof(OrderDto.CustomerLatitude), opt => opt.MapFrom(src => src.CustomerLatitude))
            .ForCtorParam(nameof(OrderDto.CustomerLongitude), opt => opt.MapFrom(src => src.CustomerLongitude))
            .ForCtorParam(nameof(OrderDto.AllocatedStoreId), opt => opt.MapFrom(src => src.AllocatedStoreId))
            .ForCtorParam(nameof(OrderDto.AllocatedStoreName), opt => opt.MapFrom(src => src.AllocatedStore != null ? src.AllocatedStore.Name : null))
            .ForCtorParam(nameof(OrderDto.CreatedAt), opt => opt.MapFrom(src => src.CreatedAt))
            .ForCtorParam(nameof(OrderDto.Status), opt => opt.MapFrom(src => src.Status))
            .ForCtorParam(nameof(OrderDto.TotalAmount), opt => opt.MapFrom(src => src.TotalAmount))
            .ForCtorParam(
                nameof(OrderDto.Items),
                opt => opt.MapFrom(src => src.Items.OrderBy(item => item.CreatedAt).ToArray()));

        CreateMap<Order, CartDto>()
            .ForCtorParam(nameof(CartDto.OrderId), opt => opt.MapFrom(src => src.Id))
            .ForCtorParam(nameof(CartDto.OrderNumber), opt => opt.MapFrom(src => src.OrderNumber))
            .ForCtorParam(nameof(CartDto.UserId), opt => opt.MapFrom(src => src.UserId))
            .ForCtorParam(nameof(CartDto.CustomerEmail), opt => opt.MapFrom(src => src.CustomerEmail))
            .ForCtorParam(nameof(CartDto.CustomerLatitude), opt => opt.MapFrom(src => src.CustomerLatitude))
            .ForCtorParam(nameof(CartDto.CustomerLongitude), opt => opt.MapFrom(src => src.CustomerLongitude))
            .ForCtorParam(nameof(CartDto.TotalAmount), opt => opt.MapFrom(src => src.TotalAmount))
            .ForCtorParam(
                nameof(CartDto.Items),
                opt => opt.MapFrom(src => src.Items.OrderBy(item => item.CreatedAt).ToArray()))
            .ForCtorParam(nameof(CartDto.IsEmpty), opt => opt.MapFrom(src => src.Items.Count == 0));
    }
}
