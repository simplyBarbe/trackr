using backend.Common.Pagination;
using FastEndpoints;
using FluentValidation;

namespace backend.Features.Categories.List;

public sealed class ListCategoriesValidator : Validator<ListCategoriesRequest>
{
    public ListCategoriesValidator()
    {
        this.AddPaginationRules();

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Kind)
            .IsInEnum()
            .When(x => x.Kind is not null);
    }
}
