using Microsoft.Kiota.Abstractions;
using Trackr.Api.Models;

namespace frontend.Features.RecurringTransactions;

internal static class RecurringFormat
{
    public static string FormatFrequency(RecurringTransactionResponse rule)
    {
        if (rule.Frequency is null)
            return "";

        return rule.Frequency switch
        {
            RecurrenceFrequency.Weekly => $"Weekly on {rule.DayOfWeek}",
            RecurrenceFrequency.Biweekly => $"Every 2 weeks (from {FormatDate(rule.StartOn)})",
            RecurrenceFrequency.Monthly => $"Monthly on day {rule.DayOfMonth}",
            RecurrenceFrequency.Yearly => $"Yearly on {rule.Month}/{rule.DayOfMonth}",
            _ => rule.Frequency.ToString() ?? ""
        };
    }

    public static string FormatDate(Date? date) =>
        date is null ? "" : new DateTime(date.Value.Year, date.Value.Month, date.Value.Day).ToString("d");

    public static Date? ToApiDate(DateTime? value) =>
        value is null ? null : new Date(value.Value.Year, value.Value.Month, value.Value.Day);

    public static DateTime? FromApiDate(Date? value) =>
        value is null ? null : new DateTime(value.Value.Year, value.Value.Month, value.Value.Day);
}
