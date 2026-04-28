using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Payments.Queries;

public sealed record GetStoreManagerPaymentsQuery(string? StoreId = null) : IRequest<IReadOnlyList<StoreManagerPaymentDto>>;

public sealed class GetStoreManagerPaymentsQueryHandler(IApplicationDbContext applicationDbContext)
    : IRequestHandler<GetStoreManagerPaymentsQuery, IReadOnlyList<StoreManagerPaymentDto>>
{
    public async Task<IReadOnlyList<StoreManagerPaymentDto>> Handle(
        GetStoreManagerPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var query = applicationDbContext.Payments
            .AsNoTracking()
            .Where(payment => payment.Order.Status != OrderStatus.Draft);

        if (!string.IsNullOrWhiteSpace(request.StoreId))
        {
            query = query.Where(payment => payment.Order.AllocatedStoreId == request.StoreId);
        }

        return await query
            .OrderByDescending(payment => payment.CapturedAt ?? payment.CreatedAt)
            .ThenByDescending(payment => payment.CreatedAt)
            .Select(payment => new StoreManagerPaymentDto(
                payment.Id,
                payment.OrderId,
                payment.Order.OrderNumber,
                payment.Order.CustomerEmail,
                payment.Order.AllocatedStoreId,
                payment.Order.AllocatedStore != null ? payment.Order.AllocatedStore.Name : null,
                payment.Provider,
                payment.ProviderOrderId,
                payment.ProviderCaptureId,
                payment.Status,
                payment.Order.Status,
                payment.Amount,
                payment.Currency,
                payment.RefundedAmount,
                payment.FailureReason,
                payment.CreatedAt,
                payment.CapturedAt))
            .Take(100)
            .ToArrayAsync(cancellationToken);
    }
}
