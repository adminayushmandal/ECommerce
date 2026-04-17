namespace Application.Common.Models;

public sealed record CartDto(
    string? OrderId,
    string? OrderNumber,
    string UserId,
    string CustomerEmail,
    double CustomerLatitude,
    double CustomerLongitude,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items,
    bool IsEmpty);
