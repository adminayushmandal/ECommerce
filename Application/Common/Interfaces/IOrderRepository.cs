using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetDraftByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Order>> GetSubmittedByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(string orderId, CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(string userId, string orderId, CancellationToken cancellationToken);
    Task AddAsync(Order order, CancellationToken cancellationToken);
}
