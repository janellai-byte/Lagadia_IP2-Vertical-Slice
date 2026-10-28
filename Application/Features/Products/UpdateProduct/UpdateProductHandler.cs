using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Products.UpdateProduct;

using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateProductHandler(
    IAppDbContext dbContext,
    IValidator<UpdateProductCommand> validator)
    : ICommandHandler<UpdateProductCommand, Result<UpdateProductResponse>>
{
    public async Task<Result<UpdateProductResponse>> HandleAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(
            command,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result<UpdateProductResponse>.Failure(
                Error.Validation(
                    "Product.Validation",
                    string.Join(", ",
                        validationResult.Errors
                            .Select(e => e.ErrorMessage))));
        }

        var product = await dbContext.Products
            .FirstOrDefaultAsync(
                p => p.Id == command.Id,
                cancellationToken);

        if (product is null)
        {
            return Result<UpdateProductResponse>.Failure(
                Error.NotFound(
                    "Product.NotFound",
                    $"Product with Id '{command.Id}' was not found."));
        }

        var skuTaken = await dbContext.Products
            .AnyAsync(
                p => p.Sku == command.Sku && p.Id != command.Id,
                cancellationToken);

        if (skuTaken)
        {
            return Result<UpdateProductResponse>.Failure(
                Error.Conflict(
                    "Product.SkuConflict",
                    $"A product with SKU '{command.Sku}' already exists."));
        }

        product.Name = command.Name;
        product.Sku = command.Sku;
        product.Description = command.Description;
        product.Price = command.Price;
        product.StockQuantity = command.StockQuantity;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<UpdateProductResponse>.Success(
            new UpdateProductResponse(
                product.Id,
                product.Name,
                product.Sku,
                product.Description,
                product.Price,
                product.StockQuantity,
                product.CreatedAt));
    }
}