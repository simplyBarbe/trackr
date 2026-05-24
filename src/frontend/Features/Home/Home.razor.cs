using frontend.Features.Shared;
using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.Kiota.Abstractions;
using MudBlazor;
using Trackr.Api;
using Trackr.Api.Models;

namespace frontend.Features.Home;

public partial class Home : ComponentBase
{
    private const int AccountsLookupPageSize = 200;
    private const int MaxChartCategories = 7;

    [Inject]
    private TrackrApiClient TrackrApi { get; set; } = null!;

    private QueryState<IReadOnlyList<AccountResponse>> _accountsQuery =
        QueryState<IReadOnlyList<AccountResponse>>.Loading();

    private QueryState<GetTransactionSummaryResponse> _summaryQuery =
        QueryState<GetTransactionSummaryResponse>.Loading();

    private QueryState<IReadOnlyList<ExpenseByCategoryItem>> _expensesByCategoryQuery =
        QueryState<IReadOnlyList<ExpenseByCategoryItem>>.Loading();

    private Date? _fromFilter;
    private Date? _toFilter;
    private DateTime? _fromPicker;
    private DateTime? _toPicker;
    private DateRangePreset _activePreset = DateRangePreset.ThisMonth;

    private double[] _categoryChartData = [];
    private string[] _categoryChartLabels = [];
    private double[] _priorityChartData = [];
    private string[] _priorityChartLabels = [];
    private PieChartOptions _priorityPieChartOptions = new() { ShowLegend = true };

    private bool IsInitialLoading =>
        _accountsQuery.IsLoading || _summaryQuery.IsLoading || _expensesByCategoryQuery.IsLoading;

    protected override async Task OnInitializedAsync()
    {
        ApplyCurrentMonthDefault();
        await Task.WhenAll(LoadAccountsAsync(), LoadDashboardDataAsync());
    }

    private async Task LoadAccountsAsync()
    {
        _accountsQuery = await QueryState<IReadOnlyList<AccountResponse>>.RunAsync(async () =>
        {
            var response = await TrackrApi.Accounts.GetAsync(configuration =>
            {
                configuration.QueryParameters.Page = 1;
                configuration.QueryParameters.PageSize = AccountsLookupPageSize;
            });

            return (IReadOnlyList<AccountResponse>)(response?.Items ?? []);
        });
    }

    private async Task LoadDashboardDataAsync()
    {
        await Task.WhenAll(LoadSummaryAsync(), LoadExpensesByCategoryAsync());
        UpdateCharts();
    }

    private async Task LoadSummaryAsync()
    {
        if (_summaryQuery.Data is not null)
            _summaryQuery = QueryState<GetTransactionSummaryResponse>.Fetching(_summaryQuery.Data);
        else
            _summaryQuery = QueryState<GetTransactionSummaryResponse>.Loading();

        _summaryQuery = await QueryState<GetTransactionSummaryResponse>.RunAsync(async () =>
        {
            var response = await TrackrApi.Transactions.Summary.GetAsync(configuration =>
            {
                if (_fromFilter is not null)
                    configuration.QueryParameters.From = _fromFilter;

                if (_toFilter is not null)
                    configuration.QueryParameters.To = _toFilter;
            });

            return response ?? new GetTransactionSummaryResponse();
        });
    }

    private async Task LoadExpensesByCategoryAsync()
    {
        if (_expensesByCategoryQuery.Data is not null)
            _expensesByCategoryQuery = QueryState<IReadOnlyList<ExpenseByCategoryItem>>.Fetching(
                _expensesByCategoryQuery.Data);
        else
            _expensesByCategoryQuery = QueryState<IReadOnlyList<ExpenseByCategoryItem>>.Loading();

        _expensesByCategoryQuery = await QueryState<IReadOnlyList<ExpenseByCategoryItem>>.RunAsync(async () =>
        {
            var response = await TrackrApi.Transactions.Summary.ExpensesByCategory.GetAsync(configuration =>
            {
                if (_fromFilter is not null)
                    configuration.QueryParameters.From = _fromFilter;

                if (_toFilter is not null)
                    configuration.QueryParameters.To = _toFilter;
            });

            return (IReadOnlyList<ExpenseByCategoryItem>)(response?.Items ?? []);
        });
    }

    private async Task ApplyPresetAsync(DateRangePreset preset)
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

        await ReloadDashboardAsync();
    }

    private async Task OnFromPickerChanged(DateTime? value)
    {
        _fromPicker = value;
        _fromFilter = ToApiDate(value);
        _activePreset = DateRangePreset.Custom;
        await ReloadDashboardAsync();
    }

    private async Task OnToPickerChanged(DateTime? value)
    {
        _toPicker = value;
        _toFilter = ToApiDate(value);
        _activePreset = DateRangePreset.Custom;
        await ReloadDashboardAsync();
    }

    private async Task ReloadDashboardAsync()
    {
        await LoadDashboardDataAsync();
    }

    private void UpdateCharts()
    {
        BuildCategoryChart();
        BuildPriorityChart();
    }

    private void BuildCategoryChart()
    {
        var items = _expensesByCategoryQuery.Data ?? [];
        if (items.Count == 0)
        {
            _categoryChartData = [];
            _categoryChartLabels = [];
            return;
        }

        var top = items.Take(MaxChartCategories).ToList();
        var otherTotal = items.Skip(MaxChartCategories).Sum(i => i.TotalAmount ?? 0m);
        var labels = top.Select(i => i.CategoryName ?? "").ToList();
        var data = top.Select(i => (double)(i.TotalAmount ?? 0m)).ToList();

        if (otherTotal > 0)
        {
            labels.Add("Other");
            data.Add((double)otherTotal);
        }

        _categoryChartLabels = labels.ToArray();
        _categoryChartData = data.ToArray();
    }

    private void BuildPriorityChart()
    {
        var summary = _summaryQuery.Data;
        if (summary is null)
        {
            _priorityChartData = [];
            _priorityChartLabels = [];
            _priorityPieChartOptions = new PieChartOptions { ShowLegend = true };
            return;
        }

        var slices = new (ExpensePriority Priority, double Value)[]
        {
            (ExpensePriority.Essential, (double)(summary.TotalEssentialExpense ?? 0m)),
            (ExpensePriority.Important, (double)(summary.TotalImportantExpense ?? 0m)),
            (ExpensePriority.Discretionary, (double)(summary.TotalDiscretionaryExpense ?? 0m))
        }.Where(s => s.Value > 0).ToArray();

        if (slices.Length == 0)
        {
            _priorityChartData = [];
            _priorityChartLabels = [];
            _priorityPieChartOptions = new PieChartOptions { ShowLegend = true };
            return;
        }

        _priorityChartLabels = slices.Select(s => ExpensePriorityFormat.GetLabel(s.Priority)).ToArray();
        _priorityChartData = slices.Select(s => s.Value).ToArray();
        _priorityPieChartOptions = new PieChartOptions
        {
            ShowLegend = true,
            ChartPalette = slices.Select(s => ExpensePriorityFormat.GetChartColor(s.Priority)).ToArray()
        };
    }

    private decimal GetCategoryExpenseTotal()
    {
        var fromSummary = _summaryQuery.Data?.TotalExpense ?? 0m;
        if (fromSummary > 0)
            return fromSummary;

        return (_expensesByCategoryQuery.Data ?? []).Sum(i => i.TotalAmount ?? 0m);
    }

    private static string FormatPercent(decimal amount, decimal total) =>
        total <= 0 ? "0 %" : $"{amount / total * 100:0.#} %";

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

    private static string FormatBalance(AccountResponse account) =>
        MoneyFormat.Format(account.Balance);

    private enum DateRangePreset
    {
        ThisMonth,
        LastMonth,
        Last30Days,
        YearToDate,
        Custom
    }
}
