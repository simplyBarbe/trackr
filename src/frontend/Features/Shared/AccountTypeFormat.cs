using Trackr.Api.Models;

namespace frontend.Features.Shared;

public static class AccountTypeFormat
{
    public static string GetLabel(AccountType type) =>
        type switch
        {
            AccountType.Checking => "Checking",
            AccountType.Savings => "Savings",
            AccountType.Cash => "Cash",
            AccountType.CreditCard => "Credit card",
            _ => type.ToString()
        };
}
