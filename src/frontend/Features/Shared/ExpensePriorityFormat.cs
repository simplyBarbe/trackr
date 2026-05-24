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

    public static Variant GetChipVariant(ExpensePriority? priority) => Variant.Outlined;

    public static string GetChipClass(ExpensePriority? priority) =>
        priority switch
        {
            ExpensePriority.Essential => "trackr-priority-chip--essential",
            ExpensePriority.Important => "trackr-priority-chip--important",
            ExpensePriority.Discretionary => "trackr-priority-chip--discretionary",
            _ => ""
        };

    /// <summary>Hex colors for charts; aligned with priority semantics.</summary>
    public static string GetChartColor(ExpensePriority priority) =>
        priority switch
        {
            ExpensePriority.Essential => "#dc2626",
            ExpensePriority.Important => "#d97706",
            ExpensePriority.Discretionary => "#64748b",
            _ => "#94a3b8"
        };
}
