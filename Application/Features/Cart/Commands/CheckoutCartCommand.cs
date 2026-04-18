using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Cart.Commands;

public sealed record CheckoutCartCommand(
    string? StoreId,
    string CustomerEmail,
    double CustomerLatitude,
    double CustomerLongitude) : IRequest<OrderDto>;

public sealed class CheckoutCartCommandHandler(
    IOrderRepository orderRepository,
    IStoreRepository storeRepository,
    IApplicationDbContext applicationDbContext,
    IUser user,
    IMapper mapper)
    : IRequestHandler<CheckoutCartCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CheckoutCartCommand request, CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new CurrentUserUnavailableException();
        var cart = await orderRepository.GetDraftByUserIdAsync(userId, cancellationToken)
            ?? throw new CartNotFoundException();

        cart.SetCustomerEmail(request.CustomerEmail);
        cart.SetCustomerLocation(request.CustomerLatitude, request.CustomerLongitude);

        var allocatedStore = await ResolveAllocatedStoreAsync(cart, request, cancellationToken);
        await ReserveInventoryAsync(allocatedStore.Id, cart, cancellationToken);

        try
        {
            cart.Checkout(allocatedStore.Id);
        }
        catch (InvalidOperationException)
        {
            throw new EmptyCartCheckoutException();
        }

        await applicationDbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<OrderDto>(cart);
    }

    private async Task<Store> ResolveAllocatedStoreAsync(
        Domain.Entities.Order cart,
        CheckoutCartCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.StoreId))
        {
            var selectedStore = await storeRepository.GetByIdAsync(request.StoreId, cancellationToken);

            if (selectedStore is null || !selectedStore.IsActive)
            {
                throw new InvalidOrderOperationException($"Store '{request.StoreId}' was not found.");
            }

            if (!await CanStoreFulfillOrderAsync(selectedStore.Id, cart, cancellationToken))
            {
                throw new UnableToAllocateStoreException($"Store '{selectedStore.Name}' cannot fulfill the current cart.");
            }

            return selectedStore;
        }

        var stores = await storeRepository.GetAllAsync(cancellationToken);

        foreach (var candidateStore in stores
            .Where(store => store.IsActive)
            .OrderBy(store => CalculateDistanceKilometers(
                request.CustomerLatitude,
                request.CustomerLongitude,
                store.Latitude,
                store.Longitude))
            .ThenBy(store => store.Name))
        {
            if (await CanStoreFulfillOrderAsync(candidateStore.Id, cart, cancellationToken))
            {
                return candidateStore;
            }
        }

        throw new UnableToAllocateStoreException("No active store can fulfill the current cart.");
    }

    private async Task<bool> CanStoreFulfillOrderAsync(
        string storeId,
        Domain.Entities.Order cart,
        CancellationToken cancellationToken)
    {
        var inventoryByKey = await GetRelevantInventoryByKeyAsync(storeId, cart, cancellationToken);

        foreach (var item in cart.Items)
        {
            if (!inventoryByKey.TryGetValue((item.ProductId, item.ProductVariantId), out var inventoryItem))
            {
                return false;
            }

            if (inventoryItem.AvailableQuantity < item.Quantity)
            {
                return false;
            }
        }

        return true;
    }

    private async Task ReserveInventoryAsync(
        string storeId,
        Domain.Entities.Order cart,
        CancellationToken cancellationToken)
    {
        var inventoryByKey = await GetRelevantInventoryByKeyAsync(storeId, cart, cancellationToken);

        foreach (var item in cart.Items)
        {
            if (!inventoryByKey.TryGetValue((item.ProductId, item.ProductVariantId), out var inventoryItem))
            {
                throw new UnableToAllocateStoreException("The allocated store is missing inventory records for one or more order items.");
            }

            try
            {
                inventoryItem.Reserve(item.Quantity);
            }
            catch (InvalidOperationException exception)
            {
                throw new UnableToAllocateStoreException(exception.Message);
            }
        }
    }

    private async Task<Dictionary<(string ProductId, string? ProductVariantId), Domain.Entities.InventoryItem>> GetRelevantInventoryByKeyAsync(
        string storeId,
        Domain.Entities.Order cart,
        CancellationToken cancellationToken)
    {
        var productIds = cart.Items
            .Select(item => item.ProductId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var inventoryItems = await applicationDbContext.InventoryItems
            .Where(inventoryItem =>
                inventoryItem.StoreId == storeId &&
                productIds.Contains(inventoryItem.ProductId))
            .ToListAsync(cancellationToken);

        return inventoryItems.ToDictionary(
            inventoryItem => (inventoryItem.ProductId, inventoryItem.ProductVariantId),
            inventoryItem => inventoryItem);
    }

    private static double CalculateDistanceKilometers(
        double originLatitude,
        double originLongitude,
        double destinationLatitude,
        double destinationLongitude)
    {
        const double earthRadiusKilometers = 6371d;

        static double ToRadians(double degrees) => degrees * (Math.PI / 180d);

        var latitudeDifference = ToRadians(destinationLatitude - originLatitude);
        var longitudeDifference = ToRadians(destinationLongitude - originLongitude);

        var originLatitudeRadians = ToRadians(originLatitude);
        var destinationLatitudeRadians = ToRadians(destinationLatitude);

        var a =
            Math.Sin(latitudeDifference / 2d) * Math.Sin(latitudeDifference / 2d) +
            Math.Cos(originLatitudeRadians) * Math.Cos(destinationLatitudeRadians) *
            Math.Sin(longitudeDifference / 2d) * Math.Sin(longitudeDifference / 2d);

        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
        return earthRadiusKilometers * c;
    }
}
