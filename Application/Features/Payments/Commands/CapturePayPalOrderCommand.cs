using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Payments.Commands;

public sealed record CapturePayPalOrderCommand(string PayPalOrderId) : IRequest<PayPalCaptureDto>;

public sealed class CapturePayPalOrderCommandHandler(
    IPaymentService paymentService,
    IPayPalCheckoutGateway payPalGateway,
    IApplicationDbContext applicationDbContext,
    IUser user)
    : IRequestHandler<CapturePayPalOrderCommand, PayPalCaptureDto>
{
    public async Task<PayPalCaptureDto> Handle(CapturePayPalOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new CurrentUserUnavailableException();
        var payment = await paymentService.GetByProviderOrderIdAsync(PaymentProvider.PayPal, request.PayPalOrderId, cancellationToken)
            ?? throw new PaymentNotFoundException(request.PayPalOrderId);

        var order = await applicationDbContext.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == payment.OrderId && order.UserId == userId, cancellationToken)
            ?? throw new OrderNotFoundException(payment.OrderId);

        var capture = await payPalGateway.CaptureOrderAsync(
            request.PayPalOrderId,
            $"{payment.Id}-capture",
            cancellationToken);

        if (!string.Equals(capture.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            payment.MarkFailed($"PayPal capture returned status '{capture.Status}'.");
            await applicationDbContext.SaveChangesAsync(cancellationToken);
            throw new InvalidPaymentOperationException($"PayPal capture returned status '{capture.Status}'.");
        }

        payment.MarkCaptured(capture.PayPalCaptureId ?? request.PayPalOrderId, DateTimeOffset.UtcNow);
        payment.Order.ConfirmPayment();

        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return new PayPalCaptureDto(
            payment.Id,
            payment.OrderId,
            payment.Order.OrderNumber,
            request.PayPalOrderId,
            payment.ProviderCaptureId,
            payment.Status,
            payment.Order.Status,
            payment.Amount,
            payment.Currency);
    }
}
