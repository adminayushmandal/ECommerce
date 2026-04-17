using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.ProductVariants.Queries;

public sealed record GetProductVariantByIdQuery(string ProductId, string ProductVariantId) : IRequest<ProductVariantDto>;

public sealed class GetProductVariantByIdQueryHandler(IProductVariantRepository productVariantRepository, IMapper mapper)
    : IRequestHandler<GetProductVariantByIdQuery, ProductVariantDto>
{
    public async Task<ProductVariantDto> Handle(GetProductVariantByIdQuery request, CancellationToken cancellationToken)
    {
        var productVariant = await productVariantRepository.GetByIdAsync(request.ProductId, request.ProductVariantId, cancellationToken)
            ?? throw new ProductVariantNotFoundException(request.ProductVariantId);

        return mapper.Map<ProductVariantDto>(productVariant);
    }
}
