using Trackr.Api.Models;

namespace frontend.Features.Shared;

public static class TransactionTypeFormat
{
    public static string GetLabel(TransactionType type) =>
        type switch
        {
            TransactionType.Income => "Income",
            TransactionType.Expense => "Expense",
            TransactionType.Transfer => "Transfer",
            _ => type.ToString()
        };
}
