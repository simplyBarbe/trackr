using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.Kiota.Abstractions;

namespace frontend.Features.Home;

public partial class Home : ComponentBase, IDisposable
{
    private Date? _fromFilter;
    private Date? _toFilter;
    private DateTime? _fromPicker;
    private DateTime? _toPicker;
    private DashboardDateRange _dateRange = new();
    private DateRangePreset _activePreset = DateRangePreset.ThisMonth;

    private readonly DebouncedAsync _dateRangePublish = new();

    protected override void OnInitialized()
    {
        ApplyCurrentMonthDefault();
        _dateRange = BuildDateRange();
    }

    private DashboardDateRange BuildDateRange() => new(_fromFilter, _toFilter);

    private Task ApplyPresetAsync(DateRangePreset preset)
    {
        _activePreset = preset;
        switch (preset)
        {
            case DateRangePreset.ThisMonth:
                ApplyCurrentMonthDefault();
                break;
            case DateRangePreset.LastMonth:
                ApplyLastMonth();
                break;
            case DateRangePreset.Last30Days:
                ApplyLast30Days();
                break;
            case DateRangePreset.YearToDate:
                ApplyYearToDate();
                break;
        }

        _dateRange = BuildDateRange();
        return Task.CompletedTask;
    }

    private Task OnFromDateChanged(DateTime? value)
    {
        _fromPicker = value;
        _fromFilter = ToApiDate(value);
        _activePreset = DateRangePreset.Custom;
        return SchedulePublishDateRangeAsync();
    }

    private Task OnToDateChanged(DateTime? value)
    {
        _toPicker = value;
        _toFilter = ToApiDate(value);
        _activePreset = DateRangePreset.Custom;
        return SchedulePublishDateRangeAsync();
    }

    private Task SchedulePublishDateRangeAsync() =>
        _dateRangePublish.InvokeAsync(PublishDateRangeAsync);

    private Task PublishDateRangeAsync()
    {
        _dateRange = BuildDateRange();
        return Task.CompletedTask;
    }

    public void Dispose() => _dateRangePublish.Dispose();

    private void ApplyCurrentMonthDefault()
    {
        var today = DateTime.Today;
        _fromPicker = new DateTime(today.Year, today.Month, 1);
        _toPicker = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
        _fromFilter = ToApiDate(_fromPicker);
        _toFilter = ToApiDate(_toPicker);
    }

    private void ApplyLastMonth()
    {
        var firstOfThisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var lastMonth = firstOfThisMonth.AddMonths(-1);
        _fromPicker = lastMonth;
        _toPicker = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
        _fromFilter = ToApiDate(_fromPicker);
        _toFilter = ToApiDate(_toPicker);
    }

    private void ApplyLast30Days()
    {
        var today = DateTime.Today;
        _fromPicker = today.AddDays(-29);
        _toPicker = today;
        _fromFilter = ToApiDate(_fromPicker);
        _toFilter = ToApiDate(_toPicker);
    }

    private void ApplyYearToDate()
    {
        var today = DateTime.Today;
        _fromPicker = new DateTime(today.Year, 1, 1);
        _toPicker = today;
        _fromFilter = ToApiDate(_fromPicker);
        _toFilter = ToApiDate(_toPicker);
    }

    private static Date? ToApiDate(DateTime? value) =>
        value is null ? null : new Date(value.Value.Year, value.Value.Month, value.Value.Day);

    private enum DateRangePreset
    {
        ThisMonth,
        LastMonth,
        Last30Days,
        YearToDate,
        Custom
    }
}
