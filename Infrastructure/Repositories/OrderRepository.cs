using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class OrderRepository(ApplicationDbContext dbContext) : IOrderRepository
{
    public Task<Order?> GetDraftByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        return dbContext.Orders
            .Include(order => order.Items)
            .Include(order => order.AllocatedStore)
            .FirstOrDefaultAsync(
                order => order.UserId == userId && order.Status == Domain.Enums.OrderStatus.Draft,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .Include(order => order.AllocatedStore)
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetSubmittedByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .Include(order => order.AllocatedStore)
            .Where(order => order.UserId == userId && order.Status != Domain.Enums.OrderStatus.Draft)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Order?> GetByIdAsync(string orderId, CancellationToken cancellationToken)
    {
        return dbContext.Orders
            .Include(order => order.Items)
            .Include(order => order.AllocatedStore)
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);
    }

    public Task<Order?> GetByIdAsync(string userId, string orderId, CancellationToken cancellationToken)
    {
        return dbContext.Orders
            .Include(order => order.Items)
            .Include(order => order.AllocatedStore)
            .FirstOrDefaultAsync(order => order.UserId == userId && order.Id == orderId, cancellationToken);
    }

    public Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        return dbContext.Orders.AddAsync(order, cancellationToken).AsTask();
    }
}
