using backend.Data.Entities;
using backend.Data.Entities.Enums;

namespace backend.Application.Services;

public static class RecurringScheduleService
{
    public static DateOnly ComputeInitialNextOccurrence(RecurringTransaction rule, DateOnly fromDate)
    {
        var earliest = fromDate > rule.StartOn ? fromDate : rule.StartOn;

        return rule.Frequency switch
        {
            RecurrenceFrequency.Weekly => AlignToDayOfWeek(earliest, rule.DayOfWeek!.Value),
            RecurrenceFrequency.Biweekly => AlignBiweekly(earliest, rule.StartOn),
            RecurrenceFrequency.Monthly => AlignMonthly(earliest, rule.DayOfMonth!.Value),
            RecurrenceFrequency.Yearly => AlignYearly(earliest, rule.Month!.Value, rule.DayOfMonth!.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(rule.Frequency))
        };
    }

    public static DateOnly Advance(RecurringTransaction rule, DateOnly currentOccurrence)
    {
        return rule.Frequency switch
        {
            RecurrenceFrequency.Weekly => currentOccurrence.AddDays(7),
            RecurrenceFrequency.Biweekly => currentOccurrence.AddDays(14),
            RecurrenceFrequency.Monthly => AdvanceMonthly(currentOccurrence, rule.DayOfMonth!.Value),
            RecurrenceFrequency.Yearly => AdvanceYearly(currentOccurrence, rule.Month!.Value, rule.DayOfMonth!.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(rule.Frequency))
        };
    }

    public static bool ScheduleFieldsChanged(
        RecurringTransaction existing,
        RecurrenceFrequency frequency,
        DayOfWeek? dayOfWeek,
        int? dayOfMonth,
        int? month,
        DateOnly startOn) =>
        existing.Frequency != frequency
        || existing.DayOfWeek != dayOfWeek
        || existing.DayOfMonth != dayOfMonth
        || existing.Month != month
        || existing.StartOn != startOn;

    private static DateOnly AlignToDayOfWeek(DateOnly from, DayOfWeek dayOfWeek)
    {
        var date = from;
        while (date.DayOfWeek != dayOfWeek)
            date = date.AddDays(1);

        return date;
    }

    private static DateOnly AlignBiweekly(DateOnly from, DateOnly startOn)
    {
        if (from <= startOn)
            return startOn;

        var daysSince = from.DayNumber - startOn.DayNumber;
        var remainder = daysSince % 14;
        return remainder == 0 ? from : from.AddDays(14 - remainder);
    }

    private static DateOnly AlignMonthly(DateOnly from, int dayOfMonth)
    {
        var candidate = WithClampedDay(from.Year, from.Month, dayOfMonth);
        if (candidate >= from)
            return candidate;

        var next = from.AddMonths(1);
        return WithClampedDay(next.Year, next.Month, dayOfMonth);
    }

    private static DateOnly AdvanceMonthly(DateOnly current, int dayOfMonth)
    {
        var next = current.AddMonths(1);
        return WithClampedDay(next.Year, next.Month, dayOfMonth);
    }

    private static DateOnly AlignYearly(DateOnly from, int month, int dayOfMonth)
    {
        var candidate = WithClampedDay(from.Year, month, dayOfMonth);
        if (candidate >= from)
            return candidate;

        return WithClampedDay(from.Year + 1, month, dayOfMonth);
    }

    private static DateOnly AdvanceYearly(DateOnly current, int month, int dayOfMonth)
    {
        return WithClampedDay(current.Year + 1, month, dayOfMonth);
    }

    private static DateOnly WithClampedDay(int year, int month, int dayOfMonth)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var day = Math.Min(dayOfMonth, daysInMonth);
        return new DateOnly(year, month, day);
    }
}
