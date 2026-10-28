using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Products.CreateProduct;

using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public sealed class CreateProductHandler(
    IAppDbContext dbContext,
    IValidator<CreateProductCommand> validator)
    : ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    public async Task<Result<CreateProductResponse>> HandleAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(
            command,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result<CreateProductResponse>.Failure(
                Error.Validation(
                    "Product.Validation",
                    string.Join(", ",
                        validationResult.Errors
                            .Select(e => e.ErrorMessage))));
        }

        var skuTaken = await dbContext.Products
            .AnyAsync(p => p.Sku == command.Sku, cancellationToken);

        if (skuTaken)
        {
            return Result<CreateProductResponse>.Failure(
                Error.Conflict(
                    "Product.SkuConflict",
                    $"A product with SKU '{command.Sku}' already exists."));
        }

        var product = new Product
        {
            Name = command.Name,
            Sku = command.Sku,
            Description = command.Description,
            Price = command.Price,
            StockQuantity = command.StockQuantity,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<CreateProductResponse>.Success(
            new CreateProductResponse(
                product.Id,
                product.Name,
                product.Sku,
                product.Description,
                product.Price,
                product.StockQuantity,
                product.CreatedAt));
    }
}