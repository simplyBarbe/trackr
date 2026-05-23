using FastEndpoints;
using FluentValidation;

namespace backend.Features.Categories.Update;

public sealed class UpdateCategoryValidator : Validator<UpdateCategoryRequest>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Kind)
            .IsInEnum();

        RuleFor(x => x.Priority)
            .IsInEnum();
    }
}
