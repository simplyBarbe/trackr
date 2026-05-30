using backend.Common.Pagination;
using FastEndpoints;
using FluentValidation;

namespace backend.Features.RecurringTransactions.List;

public sealed class ListRecurringTransactionsValidator : Validator<ListRecurringTransactionsRequest>
{
    public ListRecurringTransactionsValidator()
    {
        this.AddPaginationRules();
    }
}
