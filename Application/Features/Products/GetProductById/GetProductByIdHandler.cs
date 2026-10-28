using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Products.GetProductById;

using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

public sealed class GetProductByIdHandler(IAppDbContext dbContext)
    : IQueryHandler<GetProductByIdQuery, Result<GetProductByIdResponse>>
{
    public async Task<Result<GetProductByIdResponse>> HandleAsync(
        GetProductByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(
                p => p.Id == query.Id,
                cancellationToken);

        if (product == null)
        {
            return Result<GetProductByIdResponse>.Failure(
                Error.NotFound(
                    "Product.NotFound",
                    $"Product with id '{query.Id}' was not found."));
        }

        return Result<GetProductByIdResponse>.Success(
            new GetProductByIdResponse(
                product.Id,
                product.Name,
                product.Sku,
                product.Description,
                product.Price,
                product.StockQuantity,
                product.CreatedAt));
    }
}
