using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Products.Commands;

public sealed record DeleteProductCommand(string ProductId) : IRequest<bool>;

public sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IApplicationDbContext applicationDbContext)
    : IRequestHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        productRepository.Remove(product);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
