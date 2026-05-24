using FastEndpoints;
using FluentValidation;

namespace backend.Features.Transactions.Summary;

public sealed class GetExpensesByCategoryValidator : Validator<GetExpensesByCategoryRequest>
{
    public GetExpensesByCategoryValidator()
    {
        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From!.Value)
            .When(x => x.From is not null && x.To is not null);
    }
}
