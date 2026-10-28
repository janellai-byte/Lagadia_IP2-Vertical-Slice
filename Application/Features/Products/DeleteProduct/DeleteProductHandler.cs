using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Products.DeleteProduct;

using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public sealed class DeleteProductHandler(
    IAppDbContext dbContext,
    IValidator<DeleteProductCommand> validator)
    : ICommandHandler<DeleteProductCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(
            command,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure(
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
            return Result.Failure(
                Error.NotFound(
                    "Product.NotFound",
                    $"Product with Id '{command.Id}' was not found."));
        }

        dbContext.Products.Remove(product);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}