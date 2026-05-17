using FastEndpoints;
using FluentValidation;

namespace backend.Features.Accounts.Create;

public sealed class CreateAccountValidator : Validator<CreateAccountRequest>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(x => x.InitialBalance)
            .PrecisionScale(18, 2, true);
    }
}
