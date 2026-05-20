using FastEndpoints;
using FluentValidation;

namespace backend.Features.Accounts.List;

public sealed class ListAccountsValidator : Validator<ListAccountsRequest>
{
    public ListAccountsValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type is not null);
    }
}
