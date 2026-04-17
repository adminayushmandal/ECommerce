using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.Products.Queries;

public sealed record GetProductsQuery() : IRequest<IReadOnlyList<ProductDto>>;

public sealed class GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
    : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetListAsync(cancellationToken);
        return mapper.Map<IReadOnlyList<ProductDto>>(products);
    }
}
