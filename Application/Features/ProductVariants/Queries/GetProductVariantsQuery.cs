using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.ProductVariants.Queries;

public sealed record GetProductVariantsQuery(string ProductId) : IRequest<IReadOnlyList<ProductVariantDto>>;

public sealed class GetProductVariantsQueryHandler(
    IProductRepository productRepository,
    IProductVariantRepository productVariantRepository,
    IMapper mapper)
    : IRequestHandler<GetProductVariantsQuery, IReadOnlyList<ProductVariantDto>>
{
    public async Task<IReadOnlyList<ProductVariantDto>> Handle(GetProductVariantsQuery request, CancellationToken cancellationToken)
    {
        if (await productRepository.GetByIdAsync(request.ProductId, cancellationToken) is null)
        {
            throw new ProductNotFoundException(request.ProductId);
        }

        var productVariants = await productVariantRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
        return mapper.Map<IReadOnlyList<ProductVariantDto>>(productVariants);
    }
}
