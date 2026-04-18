using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands;

public sealed record CancelMyOrderCommand(string OrderId) : IRequest<OrderDto>;

public sealed class CancelMyOrderCommandHandler(
    IOrderRepository orderRepository,
    IApplicationDbContext applicationDbContext,
    IUser user,
    IMapper mapper)
    : IRequestHandler<CancelMyOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CancelMyOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new CurrentUserUnavailableException();
        var order = await orderRepository.GetByIdAsync(userId, request.OrderId, cancellationToken)
            ?? throw new OrderNotFoundException(request.OrderId);

        if (order.Status == Domain.Enums.OrderStatus.Draft)
        {
            throw new InvalidOrderOperationException("Draft carts cannot be cancelled through the orders workflow.");
        }

        if (order.Status == Domain.Enums.OrderStatus.Cancelled)
        {
            throw new InvalidOrderOperationException("The order is already cancelled.");
        }

        await ReleaseAllocatedInventoryAsync(order, cancellationToken);

        try
        {
            order.Cancel();
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOrderOperationException(exception.Message);
        }

        await applicationDbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<OrderDto>(order);
    }

    private async Task ReleaseAllocatedInventoryAsync(Domain.Entities.Order order, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(order.AllocatedStoreId))
        {
            return;
        }

        var productIds = order.Items
            .Select(item => item.ProductId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var inventoryItems = await applicationDbContext.InventoryItems
            .Where(inventoryItem =>
                inventoryItem.StoreId == order.AllocatedStoreId &&
                productIds.Contains(inventoryItem.ProductId))
            .ToListAsync(cancellationToken);

        var inventoryByKey = inventoryItems.ToDictionary(
            inventoryItem => (inventoryItem.ProductId, inventoryItem.ProductVariantId),
            inventoryItem => inventoryItem);

        foreach (var item in order.Items)
        {
            if (!inventoryByKey.TryGetValue((item.ProductId, item.ProductVariantId), out var inventoryItem))
            {
                throw new InvalidOrderOperationException("Allocated inventory records could not be found for the cancelled order.");
            }

            inventoryItem.Release(item.Quantity);
        }
    }
}
