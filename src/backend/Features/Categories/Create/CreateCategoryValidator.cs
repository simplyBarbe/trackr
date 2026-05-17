using FastEndpoints;
using FluentValidation;

namespace backend.Features.Categories.Create;

public sealed class CreateCategoryValidator : Validator<CreateCategoryRequest>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Kind)
            .IsInEnum();
    }
}
