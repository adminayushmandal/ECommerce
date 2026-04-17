using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Cart.Commands;

public sealed record AddCartItemCommand(
    string ProductId,
    string? ProductVariantId,
    int Quantity,
    string CustomerEmail,
    double CustomerLatitude,
    double CustomerLongitude) : IRequest<CartDto>;

public sealed class AddCartItemCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IProductVariantRepository productVariantRepository,
    IApplicationDbContext applicationDbContext,
    IUser user,
    IMapper mapper)
    : IRequestHandler<AddCartItemCommand, CartDto>
{
    public async Task<CartDto> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new CurrentUserUnavailableException();
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        ProductVariant? productVariant = null;

        if (!string.IsNullOrWhiteSpace(request.ProductVariantId))
        {
            productVariant = await productVariantRepository.GetByIdAsync(request.ProductVariantId, cancellationToken)
                ?? throw new ProductVariantNotFoundException(request.ProductVariantId);

            if (!string.Equals(productVariant.ProductId, request.ProductId, StringComparison.Ordinal))
            {
                throw new ProductVariantProductMismatchException(request.ProductId, productVariant.Id);
            }
        }

        var cart = await orderRepository.GetDraftByUserIdAsync(userId, cancellationToken);
        if (cart is null)
        {
            cart = new Order(userId, request.CustomerEmail, request.CustomerLatitude, request.CustomerLongitude);
            await orderRepository.AddAsync(cart, cancellationToken);
        }
        else
        {
            cart.SetCustomerEmail(request.CustomerEmail);
            cart.SetCustomerLocation(request.CustomerLatitude, request.CustomerLongitude);
        }

        cart.AddItem(product, productVariant, request.Quantity);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return mapper.Map<CartDto>(cart);
    }
}
