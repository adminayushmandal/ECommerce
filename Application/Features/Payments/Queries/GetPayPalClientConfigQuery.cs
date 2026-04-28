using Application.Common.Models;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Payments.Queries;

public sealed record GetPayPalClientConfigQuery() : IRequest<PayPalClientConfigDto>;

public sealed class GetPayPalClientConfigQueryHandler(IPayPalCheckoutGateway payPalGateway)
    : IRequestHandler<GetPayPalClientConfigQuery, PayPalClientConfigDto>
{
    public Task<PayPalClientConfigDto> Handle(GetPayPalClientConfigQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new PayPalClientConfigDto(payPalGateway.ClientId, payPalGateway.CurrencyCode, "capture"));
    }
}
