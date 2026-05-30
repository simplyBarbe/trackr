using backend.Features.RecurringTransactions.Shared;
using FluentValidation;

namespace backend.Features.RecurringTransactions;

public static class RecurringTransactionTemplateValidator
{
    public static void ApplyTemplateRules<T>(AbstractValidator<T> validator)
        where T : RecurringTransactionTemplateRequest
    {
        validator.RuleFor(x => x.Type).IsInEnum();
        validator.RuleFor(x => x.AccountId).NotEmpty();
        validator.RuleFor(x => x.Amount).GreaterThan(0).PrecisionScale(18, 2, true);
        validator.RuleFor(x => x.Description).MaximumLength(500);
        validator.RuleFor(x => x.Frequency).IsInEnum();
        validator.RuleFor(x => x.StartOn).NotEmpty();
        validator.RuleFor(x => x.Priority).IsInEnum().When(x => x.Priority is not null);
        validator.RuleFor(x => x.EndOn)
            .GreaterThanOrEqualTo(x => x.StartOn)
            .When(x => x.EndOn is not null);
    }
}
