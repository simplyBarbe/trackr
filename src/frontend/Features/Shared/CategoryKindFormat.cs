using Trackr.Api.Models;

namespace frontend.Features.Shared;

public static class CategoryKindFormat
{
    public static string GetLabel(CategoryKind kind) =>
        kind switch
        {
            CategoryKind.Income => "Income",
            CategoryKind.Expense => "Expense",
            _ => kind.ToString()
        };
}
