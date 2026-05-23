using FastEndpoints;
using FluentValidation;

namespace backend.Features.Accounts.Update;

public sealed class UpdateAccountValidator : Validator<UpdateAccountRequest>
{
    public UpdateAccountValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Color)
            .IsInEnum();

        RuleFor(x => x.InitialBalance)
            .PrecisionScale(18, 2, true);
    }
}
