using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Products.DeleteProduct;

using FluentValidation;

public sealed class DeleteProductValidator
    : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Product Id is required.");
    }
}