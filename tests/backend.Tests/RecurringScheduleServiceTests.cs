using backend.Application.Services;
using backend.Data.Entities;
using backend.Data.Entities.Enums;

namespace backend.Tests;

public class RecurringScheduleServiceTests
{
    [Fact]
    public void ComputeInitial_Monthly31st_ClampsFebruary()
    {
        var rule = MonthlyRule(dayOfMonth: 31);
        var next = RecurringScheduleService.ComputeInitialNextOccurrence(rule, new DateOnly(2024, 1, 15));

        Assert.Equal(new DateOnly(2024, 1, 31), next);
        var advanced = RecurringScheduleService.Advance(rule, next);
        Assert.Equal(new DateOnly(2024, 2, 29), advanced);
    }

    [Fact]
    public void Advance_Biweekly_AdvancesBy14Days()
    {
        var startOn = new DateOnly(2024, 1, 1);
        var rule = new RecurringTransaction
        {
            Frequency = RecurrenceFrequency.Biweekly,
            StartOn = startOn,
            DayOfWeek = System.DayOfWeek.Monday
        };

        var next = RecurringScheduleService.ComputeInitialNextOccurrence(rule, startOn);
        Assert.Equal(startOn, next);
        Assert.Equal(new DateOnly(2024, 1, 15), RecurringScheduleService.Advance(rule, next));
    }

    [Fact]
    public void ComputeInitial_Weekly_AlignsToRequestedDay()
    {
        var rule = new RecurringTransaction
        {
            Frequency = RecurrenceFrequency.Weekly,
            StartOn = new DateOnly(2024, 1, 1),
            DayOfWeek = System.DayOfWeek.Friday
        };

        var next = RecurringScheduleService.ComputeInitialNextOccurrence(rule, new DateOnly(2024, 1, 2));
        Assert.Equal(System.DayOfWeek.Friday, next.DayOfWeek);
        Assert.Equal(new DateOnly(2024, 1, 5), next);
    }

    [Fact]
    public void Advance_Yearly_LeapYearFebruary29()
    {
        var rule = new RecurringTransaction
        {
            Frequency = RecurrenceFrequency.Yearly,
            StartOn = new DateOnly(2024, 2, 29),
            Month = 2,
            DayOfMonth = 29
        };

        var next = RecurringScheduleService.ComputeInitialNextOccurrence(rule, new DateOnly(2024, 1, 1));
        Assert.Equal(new DateOnly(2024, 2, 29), next);
        Assert.Equal(new DateOnly(2025, 2, 28), RecurringScheduleService.Advance(rule, next));
    }

    private static RecurringTransaction MonthlyRule(int dayOfMonth) =>
        new()
        {
            Frequency = RecurrenceFrequency.Monthly,
            StartOn = new DateOnly(2024, 1, 1),
            DayOfMonth = dayOfMonth
        };
}
