using Domain.Enums;

namespace Application.Common.Models;

public sealed record OrderDto(
    string Id,
    string OrderNumber,
    string UserId,
    string CustomerEmail,
    double CustomerLatitude,
    double CustomerLongitude,
    string? AllocatedStoreId,
    string? AllocatedStoreName,
    DateTimeOffset CreatedAt,
    OrderStatus Status,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items);
