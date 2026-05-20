using FastEndpoints;
using FluentValidation;
using backend.Common.Pagination;

namespace backend.Features.Transactions.List;

public sealed class ListTransactionsValidator : Validator<ListTransactionsRequest>
{
    public ListTransactionsValidator()
    {
        this.AddPaginationRules();

        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type is not null);
    }
}
