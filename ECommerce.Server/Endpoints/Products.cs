using Application.Common.Models;
using Application.Features.Products.Commands;
using Application.Features.Products.Queries;
using Application.Features.ProductVariants.Commands;
using Application.Features.ProductVariants.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Constants;

namespace ECommerce.Server.Endpoints;

public sealed class Products : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetProducts)
            .WithSummary("List products")
            .WithDescription("Returns all products with their category information and variants.")
            .Produces<IReadOnlyList<ProductDto>>();

        groupBuilder.MapGet(GetProductById, "{id}")
            .WithSummary("Get product")
            .WithDescription("Returns a single product by id with its variants.")
            .Produces<ProductDto>();

        groupBuilder.MapPost(CreateProduct)
            .RequireAuthorization(Contracts.Products.Create)
            .WithSummary("Create product")
            .WithDescription("Creates a new product in the catalog.")
            .Produces<ProductDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        groupBuilder.MapPut(UpdateProduct, "{id}")
            .RequireAuthorization(Contracts.Products.Update)
            .WithSummary("Update product")
            .WithDescription("Updates an existing product in the catalog.")
            .Produces<ProductDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        groupBuilder.MapDelete(DeleteProduct, "{id}")
            .RequireAuthorization(Contracts.Products.Delete)
            .WithSummary("Delete product")
            .WithDescription("Deletes an existing product and its variants.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        groupBuilder.MapGet(GetProductVariants, "{productId}/variants")
            .WithSummary("List product variants")
            .WithDescription("Returns all variants for a specific product.")
            .Produces<IReadOnlyList<ProductVariantDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        groupBuilder.MapGet(GetProductVariantById, "{productId}/variants/{variantId}")
            .WithSummary("Get product variant")
            .WithDescription("Returns a single variant for a specific product.")
            .Produces<ProductVariantDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        groupBuilder.MapPost(CreateProductVariant, "{productId}/variants")
            .RequireAuthorization(Contracts.ProductVariants.Create)
            .WithSummary("Create product variant")
            .WithDescription("Creates a variant under an existing product.")
            .Produces<ProductVariantDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        groupBuilder.MapPut(UpdateProductVariant, "{productId}/variants/{variantId}")
            .RequireAuthorization(Contracts.ProductVariants.Update)
            .WithSummary("Update product variant")
            .WithDescription("Updates an existing product variant.")
            .Produces<ProductVariantDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        groupBuilder.MapDelete(DeleteProductVariant, "{productId}/variants/{variantId}")
            .RequireAuthorization(Contracts.ProductVariants.Delete)
            .WithSummary("Delete product variant")
            .WithDescription("Deletes a product variant from an existing product.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<IReadOnlyList<ProductDto>>> GetProducts(
        string? storeId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var products = await sender.Send(new GetProductsQuery(storeId), cancellationToken);
        return TypedResults.Ok(products);
    }

    private static async Task<Ok<ProductDto>> GetProductById(
        string id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var product = await sender.Send(new GetProductByIdQuery(id), cancellationToken);
        return TypedResults.Ok(product);
    }

    private static async Task<Created<ProductDto>> CreateProduct(
        CreateProductRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var product = await sender.Send(
            new CreateProductCommand(
                request.CategoryId,
                request.Sku,
                request.Name,
                request.Slug,
                request.Description,
                request.BasePrice,
                request.ImageUrl,
                request.IsActive),
            cancellationToken);

        return TypedResults.Created($"/api/Products/{product.Id}", product);
    }

    private static async Task<Ok<ProductDto>> UpdateProduct(
        string id,
        UpdateProductRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var product = await sender.Send(
            new UpdateProductCommand(
                id,
                request.CategoryId,
                request.Sku,
                request.Name,
                request.Slug,
                request.Description,
                request.BasePrice,
                request.ImageUrl,
                request.IsActive),
            cancellationToken);

        return TypedResults.Ok(product);
    }

    private static async Task<NoContent> DeleteProduct(
        string id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteProductCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<IReadOnlyList<ProductVariantDto>>> GetProductVariants(
        string productId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var variants = await sender.Send(new GetProductVariantsQuery(productId), cancellationToken);
        return TypedResults.Ok(variants);
    }

    private static async Task<Ok<ProductVariantDto>> GetProductVariantById(
        string productId,
        string variantId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var variant = await sender.Send(new GetProductVariantByIdQuery(productId, variantId), cancellationToken);
        return TypedResults.Ok(variant);
    }

    private static async Task<Created<ProductVariantDto>> CreateProductVariant(
        string productId,
        CreateProductVariantRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var variant = await sender.Send(
            new CreateProductVariantCommand(
                productId,
                request.Sku,
                request.Name,
                request.AttributeSummary,
                request.ImageUrl,
                request.PriceOverride,
                request.IsActive),
            cancellationToken);

        return TypedResults.Created($"/api/Products/{productId}/variants/{variant.Id}", variant);
    }

    private static async Task<Ok<ProductVariantDto>> UpdateProductVariant(
        string productId,
        string variantId,
        UpdateProductVariantRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var variant = await sender.Send(
            new UpdateProductVariantCommand(
                productId,
                variantId,
                request.Sku,
                request.Name,
                request.AttributeSummary,
                request.ImageUrl,
                request.PriceOverride,
                request.IsActive),
            cancellationToken);

        return TypedResults.Ok(variant);
    }

    private static async Task<NoContent> DeleteProductVariant(
        string productId,
        string variantId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteProductVariantCommand(productId, variantId), cancellationToken);
        return TypedResults.NoContent();
    }

    public sealed record CreateProductRequest(
        string CategoryId,
        string Sku,
        string Name,
        string Slug,
        string Description,
        decimal BasePrice,
        string ImageUrl,
        bool IsActive = true);

    public sealed record UpdateProductRequest(
        string CategoryId,
        string Sku,
        string Name,
        string Slug,
        string Description,
        decimal BasePrice,
        string ImageUrl,
        bool IsActive = true);

    public sealed record CreateProductVariantRequest(
        string Sku,
        string Name,
        string? AttributeSummary,
        string ImageUrl,
        decimal? PriceOverride,
        bool IsActive = true);

    public sealed record UpdateProductVariantRequest(
        string Sku,
        string Name,
        string? AttributeSummary,
        string ImageUrl,
        decimal? PriceOverride,
        bool IsActive = true);
}
