namespace Api.Features.Product;

using Api.Common.Extensions;
using Application.Abstractions.Messaging;
using Application.Features.Products.CreateProduct;
using Application.Features.Products.GetAllProducts;
using Application.Features.Products.GetProductById;
using Application.Features.Products.UpdateProduct;
using Application.Features.Products.DeleteProduct;
using Domain.Common;

public static class ProductEndpoints
{
    private const string GetByIdRouteName = "GetProductById";

    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", GetAll)
            .WithName("GetAllProducts")
            .WithSummary("Get all products");

        group.MapPost("/", Create)
            .WithName("CreateProduct")
            .WithSummary("Create a new product");

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateProduct")
            .WithSummary("Update a product");

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteProduct")
            .WithSummary("Delete a product");

        group.MapGet("/{id:guid}", GetById)
            .WithName(GetByIdRouteName)
            .WithSummary("Get product by ID");
    }

    private static async Task<IResult> GetAll(
        IQueryHandler<GetAllProductsQuery, Result<List<GetAllProductsResponse>>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetAllProductsQuery();

        var result = await handler.HandleAsync(
            query,
            cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> Create(
        CreateProductCommand command,
        ICommandHandler<CreateProductCommand, Result<CreateProductResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            command,
            cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                result.Value,
                GetByIdRouteName,
                new { id = result.Value!.Id })
            : result.ToProblemDetails();
    }

    private static async Task<IResult> GetById(
        Guid id,
        IQueryHandler<GetProductByIdQuery, Result<GetProductByIdResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);

        var result = await handler.HandleAsync(
            query,
            cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateProductRequest request,
        ICommandHandler<UpdateProductCommand, Result<UpdateProductResponse>> handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Sku,
            request.Description,
            request.Price,
            request.StockQuantity);

        var result = await handler.HandleAsync(
            command,
            cancellationToken);

        return result.Match(
            onSuccess: value => TypedResults.Ok(value),
            onFailure: _ => result.ToProblemDetails());
    }

    private static async Task<IResult> Delete(
        Guid id,
        ICommandHandler<DeleteProductCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new DeleteProductCommand(id),
            cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    public sealed record UpdateProductRequest(
        string Name,
        string Sku,
        string? Description,
        decimal Price,
        int StockQuantity);
}
