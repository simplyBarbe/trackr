using FastEndpoints;
using FluentValidation;

namespace backend.Features.RecurringTransactions.Update;

public sealed class UpdateRecurringTransactionValidator : Validator<UpdateRecurringTransactionRequest>
{
    public UpdateRecurringTransactionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RecurringTransactionTemplateValidator.ApplyTemplateRules(this);
    }
}
