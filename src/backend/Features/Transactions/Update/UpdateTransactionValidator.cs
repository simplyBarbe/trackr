using FastEndpoints;
using FluentValidation;

namespace backend.Features.Transactions.Update;

public sealed class UpdateTransactionValidator : Validator<UpdateTransactionRequest>
{
    public UpdateTransactionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.OccurredOn).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
