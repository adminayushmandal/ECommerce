using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.Products.Commands;

public sealed record UpdateProductCommand(
    string ProductId,
    string CategoryId,
    string Sku,
    string Name,
    string Slug,
    string Description,
    decimal BasePrice,
    string ImageUrl,
    bool IsActive) : IRequest<ProductDto>;

public sealed class UpdateProductCommandHandler(
    ICategoryRepository categoryRepository,
    IProductRepository productRepository,
    IApplicationDbContext applicationDbContext,
    IMapper mapper)
    : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        if (!await categoryRepository.ExistsAsync(request.CategoryId, cancellationToken))
        {
            throw new CategoryNotFoundException(request.CategoryId);
        }

        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        if (await productRepository.ExistsBySkuAsync(request.Sku, request.ProductId, cancellationToken))
        {
            throw new DuplicateProductSkuException(request.Sku);
        }

        if (await productRepository.ExistsBySlugAsync(request.Slug, request.ProductId, cancellationToken))
        {
            throw new DuplicateProductSlugException(request.Slug);
        }

        product.UpdateDetails(
            request.CategoryId,
            request.Sku,
            request.Name,
            request.Slug,
            request.Description,
            request.BasePrice,
            request.ImageUrl);

        product.SetActive(request.IsActive);

        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return mapper.Map<ProductDto>(product);
    }
}
