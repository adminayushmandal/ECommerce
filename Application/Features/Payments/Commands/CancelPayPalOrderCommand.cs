using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Payments.Commands;

public sealed record CancelPayPalOrderCommand(string PayPalOrderId) : IRequest<PayPalCaptureDto>;

public sealed class CancelPayPalOrderCommandHandler(
    IPaymentService paymentService,
    OrderCheckoutService checkoutService,
    IApplicationDbContext applicationDbContext,
    IUser user)
    : IRequestHandler<CancelPayPalOrderCommand, PayPalCaptureDto>
{
    public async Task<PayPalCaptureDto> Handle(CancelPayPalOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new CurrentUserUnavailableException();
        var payment = await paymentService.GetByProviderOrderIdAsync(PaymentProvider.PayPal, request.PayPalOrderId, cancellationToken)
            ?? throw new PaymentNotFoundException(request.PayPalOrderId);

        var order = await applicationDbContext.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == payment.OrderId && order.UserId == userId, cancellationToken)
            ?? throw new OrderNotFoundException(payment.OrderId);

        if (order.Status == OrderStatus.PendingPayment)
        {
            await checkoutService.ReleaseInventoryReservationAsync(order, cancellationToken);
            order.Cancel();
        }

        payment.MarkCancelled("The PayPal checkout was cancelled by the shopper.");
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return new PayPalCaptureDto(
            payment.Id,
            payment.OrderId,
            order.OrderNumber,
            request.PayPalOrderId,
            payment.ProviderCaptureId,
            payment.Status,
            order.Status,
            payment.Amount,
            payment.Currency);
    }
}
