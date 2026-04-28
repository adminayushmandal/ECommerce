using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Services;

public sealed class OrderCheckoutService(
    IStoreRepository storeRepository,
    IApplicationDbContext applicationDbContext)
{
    public async Task<Store> ResolveAndReserveInventoryAsync(
        Order cart,
        string? storeId,
        double customerLatitude,
        double customerLongitude,
        CancellationToken cancellationToken)
    {
        var allocatedStore = await ResolveAllocatedStoreAsync(
            cart,
            storeId,
            customerLatitude,
            customerLongitude,
            cancellationToken);

        await ReserveInventoryAsync(allocatedStore.Id, cart, cancellationToken);
        return allocatedStore;
    }

    public async Task ReleaseInventoryReservationAsync(Order order, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(order.AllocatedStoreId))
        {
            return;
        }

        var inventoryByKey = await GetRelevantInventoryByKeyAsync(order.AllocatedStoreId, order, cancellationToken);

        foreach (var item in order.Items)
        {
            if (!inventoryByKey.TryGetValue((item.ProductId, item.ProductVariantId), out var inventoryItem))
            {
                throw new InvalidOrderOperationException("Allocated inventory records could not be found for the order.");
            }

            inventoryItem.Release(item.Quantity);
        }
    }

    private async Task<Store> ResolveAllocatedStoreAsync(
        Order cart,
        string? storeId,
        double customerLatitude,
        double customerLongitude,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(storeId))
        {
            var selectedStore = await storeRepository.GetByIdAsync(storeId, cancellationToken);

            if (selectedStore is null || !selectedStore.IsActive)
            {
                throw new InvalidOrderOperationException($"Store '{storeId}' was not found.");
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
                customerLatitude,
                customerLongitude,
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
        Order cart,
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
        Order cart,
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

    private async Task<Dictionary<(string ProductId, string? ProductVariantId), InventoryItem>> GetRelevantInventoryByKeyAsync(
        string storeId,
        Order cart,
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
