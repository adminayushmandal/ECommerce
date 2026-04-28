using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

internal sealed class PaymentService(ApplicationDbContext dbContext, TimeProvider timeProvider) : IPaymentService
{
    public Task<Payment?> GetByIdAsync(string paymentId, CancellationToken cancellationToken)
    {
        return dbContext.Payments
            .Include(payment => payment.Order)
            .Include(payment => payment.Events)
            .FirstOrDefaultAsync(payment => payment.Id == paymentId, cancellationToken);
    }

    public Task<Payment?> GetByProviderOrderIdAsync(
        PaymentProvider provider,
        string providerOrderId,
        CancellationToken cancellationToken)
    {
        if (provider == PaymentProvider.Unknown)
        {
            throw new InvalidPaymentOperationException("Payment provider is required.");
        }

        var normalizedProviderOrderId = NormalizeProviderReference(providerOrderId, nameof(providerOrderId));

        return dbContext.Payments
            .Include(payment => payment.Order)
            .Include(payment => payment.Events)
            .FirstOrDefaultAsync(
                payment => payment.Provider == provider && payment.ProviderOrderId == normalizedProviderOrderId,
                cancellationToken);
    }

    public async Task<bool> HasProviderEventAsync(string providerEventId, CancellationToken cancellationToken)
    {
        var normalizedProviderEventId = NormalizeProviderReference(providerEventId, nameof(providerEventId));

        return await dbContext.PaymentEvents
            .AnyAsync(paymentEvent => paymentEvent.ProviderEventId == normalizedProviderEventId, cancellationToken);
    }

    public async Task<Payment> CreatePaymentAsync(
        string orderId,
        PaymentProvider provider,
        decimal amount,
        string currency,
        CancellationToken cancellationToken)
    {
        var orderExists = await dbContext.Orders
            .AnyAsync(order => order.Id == orderId, cancellationToken);

        if (!orderExists)
        {
            throw new OrderNotFoundException(orderId);
        }

        var payment = new Payment(orderId, provider, amount, currency);
        await dbContext.Payments.AddAsync(payment, cancellationToken);
        return payment;
    }

    public async Task<Payment> MarkRequiresActionAsync(
        string paymentId,
        string providerOrderId,
        CancellationToken cancellationToken)
    {
        var payment = await GetRequiredPaymentAsync(paymentId, cancellationToken);
        payment.MarkRequiresAction(providerOrderId);
        return payment;
    }

    public async Task<Payment> MarkCapturedAsync(
        string paymentId,
        string providerCaptureId,
        DateTimeOffset? capturedAt,
        CancellationToken cancellationToken)
    {
        var payment = await GetRequiredPaymentAsync(paymentId, cancellationToken);
        payment.MarkCaptured(providerCaptureId, capturedAt ?? timeProvider.GetUtcNow());
        return payment;
    }

    public async Task<Payment> MarkFailedAsync(
        string paymentId,
        string failureReason,
        CancellationToken cancellationToken)
    {
        var payment = await GetRequiredPaymentAsync(paymentId, cancellationToken);
        payment.MarkFailed(failureReason);
        return payment;
    }

    public async Task<Payment> MarkCancelledAsync(
        string paymentId,
        string? reason,
        CancellationToken cancellationToken)
    {
        var payment = await GetRequiredPaymentAsync(paymentId, cancellationToken);
        payment.MarkCancelled(reason);
        return payment;
    }

    public async Task<Payment> RecordRefundAsync(
        string paymentId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var payment = await GetRequiredPaymentAsync(paymentId, cancellationToken);
        payment.RecordRefund(amount);
        return payment;
    }

    public async Task<PaymentEvent> RecordProviderEventAsync(
        string paymentId,
        string providerEventId,
        string eventType,
        DateTimeOffset occurredAt,
        string? rawPayload,
        CancellationToken cancellationToken)
    {
        var normalizedProviderEventId = NormalizeProviderReference(providerEventId, nameof(providerEventId));

        if (await HasProviderEventAsync(normalizedProviderEventId, cancellationToken))
        {
            throw new DuplicatePaymentProviderEventException(normalizedProviderEventId);
        }

        var payment = await GetRequiredPaymentAsync(paymentId, cancellationToken);
        var paymentEvent = payment.AddEvent(normalizedProviderEventId, eventType, occurredAt, rawPayload);

        await dbContext.PaymentEvents.AddAsync(paymentEvent, cancellationToken);
        return paymentEvent;
    }

    private async Task<Payment> GetRequiredPaymentAsync(string paymentId, CancellationToken cancellationToken)
    {
        return await GetByIdAsync(paymentId, cancellationToken)
            ?? throw new PaymentNotFoundException(paymentId);
    }

    private static string NormalizeProviderReference(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Provider reference is required.", parameterName);
        }

        return value.Trim();
    }
}
