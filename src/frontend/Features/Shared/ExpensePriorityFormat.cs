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

    /// <summary>Hex colors for charts; aligned with <see cref="GetChipColor"/> (Error / Warning / Default).</summary>
    public static string GetChartColor(ExpensePriority priority) =>
        priority switch
        {
            ExpensePriority.Essential => Colors.Red.Default,
            ExpensePriority.Important => Colors.Orange.Default,
            ExpensePriority.Discretionary => Colors.Gray.Default,
            _ => Colors.Gray.Lighten1
        };
}
