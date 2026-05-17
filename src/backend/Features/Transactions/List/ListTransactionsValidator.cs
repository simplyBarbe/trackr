using FastEndpoints;
using FluentValidation;

namespace backend.Features.Transactions.List;

public sealed class ListTransactionsValidator : Validator<ListTransactionsRequest>
{
    public ListTransactionsValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);

        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type is not null);
    }
}
