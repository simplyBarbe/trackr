using frontend.Features.Shared;
using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Trackr.Api;
using Trackr.Api.Models;

namespace frontend.Features.Home;

public partial class HomeDashboardSection : ComponentBase
{
    private const int MaxChartCategories = 7;

    [Inject]
    private TrackrApiClient TrackrApi { get; set; } = null!;

    [Parameter, EditorRequired]
    public DashboardDateRange DateRange { get; set; } = null!;

    private GetTransactionSummaryResponse _summary = new();
    private IReadOnlyList<ExpenseByCategoryItem> _expenses = [];
    private string? _summaryError;
    private string? _expensesError;
    private bool _initialLoading = true;
    private DashboardDateRange? _loadedForRange;

    private double[] _categoryChartData = [];
    private string[] _categoryChartLabels = [];
    private double[] _priorityChartData = [];
    private string[] _priorityChartLabels = [];
    private PieChartOptions _priorityPieChartOptions = new() { ShowLegend = true };

    protected override async Task OnParametersSetAsync()
    {
        if (_loadedForRange == DateRange)
            return;

        _loadedForRange = DateRange;
        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        await Task.WhenAll(LoadSummaryAsync(), LoadExpensesByCategoryAsync());
        UpdateCharts();
        _initialLoading = false;
    }

    private async Task LoadSummaryAsync()
    {
        try
        {
            var response = await TrackrApi.Transactions.Summary.GetAsync(configuration =>
            {
                DateRange.ApplyTo(
                    v => configuration.QueryParameters.From = v,
                    v => configuration.QueryParameters.To = v);
            });

            _summary = response ?? new GetTransactionSummaryResponse();
            _summaryError = null;
        }
        catch (Exception ex)
        {
            _summaryError = ApiErrors.GetMessage(ex);
        }
    }

    private async Task LoadExpensesByCategoryAsync()
    {
        try
        {
            var response = await TrackrApi.Transactions.Summary.ExpensesByCategory.GetAsync(configuration =>
            {
                DateRange.ApplyTo(
                    v => configuration.QueryParameters.From = v,
                    v => configuration.QueryParameters.To = v);
            });

            _expenses = response?.Items ?? [];
            _expensesError = null;
        }
        catch (Exception ex)
        {
            _expensesError = ApiErrors.GetMessage(ex);
        }
    }

    private void UpdateCharts()
    {
        BuildCategoryChart();
        BuildPriorityChart();
    }

    private void BuildCategoryChart()
    {
        if (_expenses.Count == 0)
        {
            _categoryChartData = [];
            _categoryChartLabels = [];
            return;
        }

        var top = _expenses.Take(MaxChartCategories).ToList();
        var otherTotal = _expenses.Skip(MaxChartCategories).Sum(i => i.TotalAmount ?? 0m);
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
        var slices = new (ExpensePriority Priority, double Value)[]
        {
            (ExpensePriority.Essential, (double)(_summary.TotalEssentialExpense ?? 0m)),
            (ExpensePriority.Important, (double)(_summary.TotalImportantExpense ?? 0m)),
            (ExpensePriority.Discretionary, (double)(_summary.TotalDiscretionaryExpense ?? 0m))
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
        var fromSummary = _summary.TotalExpense ?? 0m;
        if (fromSummary > 0)
            return fromSummary;

        return _expenses.Sum(i => i.TotalAmount ?? 0m);
    }

    private static string FormatPercent(decimal amount, decimal total) =>
        total <= 0 ? "0 %" : $"{amount / total * 100:0.#} %";
}
