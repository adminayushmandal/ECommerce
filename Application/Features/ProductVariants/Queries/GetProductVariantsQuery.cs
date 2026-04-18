using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductVariants.Queries;

public sealed record GetProductVariantsQuery(string ProductId) : IRequest<IReadOnlyList<ProductVariantDto>>;

public sealed class GetProductVariantsQueryHandler(
    IApplicationDbContext applicationDbContext,
    IMapper mapper)
    : IRequestHandler<GetProductVariantsQuery, IReadOnlyList<ProductVariantDto>>
{
    public async Task<IReadOnlyList<ProductVariantDto>> Handle(GetProductVariantsQuery request, CancellationToken cancellationToken)
    {
        if (!await applicationDbContext.Products.AnyAsync(product => product.Id == request.ProductId, cancellationToken))
        {
            throw new ProductNotFoundException(request.ProductId);
        }

        return await applicationDbContext.ProductVariants
            .AsNoTracking()
            .Where(productVariant => productVariant.ProductId == request.ProductId)
            .OrderBy(productVariant => productVariant.Name)
            .ProjectToListAsync<ProductVariantDto>(mapper.ConfigurationProvider, cancellationToken);
    }
}
