using FastEndpoints;
using FluentValidation;

namespace backend.Features.RecurringTransactions.Create;

public sealed class CreateRecurringTransactionValidator : Validator<CreateRecurringTransactionRequest>
{
    public CreateRecurringTransactionValidator()
    {
        RecurringTransactionTemplateValidator.ApplyTemplateRules(this);
    }
}
