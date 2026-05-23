using FastEndpoints;
using FluentValidation;

namespace backend.Features.Transactions.Summary;

public sealed class GetTransactionSummaryValidator : Validator<GetTransactionSummaryRequest>
{
    public GetTransactionSummaryValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type is not null);
    }
}
