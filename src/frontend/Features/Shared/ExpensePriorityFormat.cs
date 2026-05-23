using MudBlazor;
using Trackr.Api.Models;

namespace frontend.Features.Shared;

public static class ExpensePriorityFormat
{
    public static string GetLabel(ExpensePriority? priority) =>
        priority switch
        {
            ExpensePriority.Essential => "Essential",
            ExpensePriority.Important => "Important",
            ExpensePriority.Discretionary => "Discretionary",
            _ => "—"
        };

    public static Color GetChipColor(ExpensePriority? priority) =>
        priority switch
        {
            ExpensePriority.Essential => Color.Error,
            ExpensePriority.Important => Color.Warning,
            ExpensePriority.Discretionary => Color.Default,
            _ => Color.Default
        };
}
