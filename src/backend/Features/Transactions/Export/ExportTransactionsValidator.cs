using FastEndpoints;
using FluentValidation;

namespace backend.Features.Transactions.Export;

public sealed class ExportTransactionsValidator : Validator<ExportTransactionsRequest>
{
    public ExportTransactionsValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type is not null);

        RuleFor(x => x.Priority)
            .IsInEnum()
            .When(x => x.Priority is not null);
    }
}
