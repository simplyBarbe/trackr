using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Trackr.Api;
using Trackr.Api.Models;

namespace frontend.Features.Categories;

public partial class CategoriesPage : ComponentBase
{
    [Inject]
    private TrackrApiClient Api { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    private QueryState<PagedList<CategoryResponse>> _query = QueryState<PagedList<CategoryResponse>>.Loading();

    private MutationState _mutation = MutationState.Idle;
    private MudTable<CategoryResponse>? _table;
    private int _tableVersion;

    private bool _includeArchived;
    private string _kindQuery = string.Empty;
    private string _nameDraft = string.Empty;
    private string _nameFilter = string.Empty;

    private async Task<TableData<CategoryResponse>> LoadServerDataAsync(TableState state, CancellationToken cancellationToken)
    {
        if (_query.Data is not null)
            _query = QueryState<PagedList<CategoryResponse>>.Fetching(_query.Data);
        else
            _query = QueryState<PagedList<CategoryResponse>>.Loading();

        var page = state.Page + 1;
        var pageSize = state.PageSize > 0 ? state.PageSize : 50;

        _query = await QueryState<PagedList<CategoryResponse>>.RunAsync(async () =>
        {
            var response = await Api.Api.Categories.GetAsync(configuration =>
                {
                    configuration.QueryParameters.IncludeArchived = _includeArchived;
                    configuration.QueryParameters.Page = page;
                    configuration.QueryParameters.PageSize = pageSize;
                    if (!string.IsNullOrEmpty(_kindQuery))
                        configuration.QueryParameters.Kind = _kindQuery;
                    if (!string.IsNullOrEmpty(_nameFilter))
                        configuration.QueryParameters.Name = _nameFilter;
                },
                cancellationToken);

            return new PagedList<CategoryResponse>(
                response?.Items ?? [],
                response?.Page ?? page,
                response?.PageSize ?? pageSize,
                response?.TotalCount ?? 0);
        });

        if (_query.Data is null)
            return new TableData<CategoryResponse> { Items = [], TotalItems = 0 };

        return new TableData<CategoryResponse>
        {
            Items = _query.Data.Items,
            TotalItems = _query.Data.TotalCount
        };
    }

    private async Task OnKindQueryChanged(string value)
    {
        _kindQuery = value;
        await ResetToFirstPageAndReloadAsync();
    }

    private async Task OnIncludeArchivedChanged(bool value)
    {
        _includeArchived = value;
        await ResetToFirstPageAndReloadAsync();
    }

    private async Task CommitNameFilterAsync()
    {
        var trimmed = (_nameDraft ?? string.Empty).Trim();
        if (string.Equals(trimmed, _nameFilter, StringComparison.Ordinal))
            return;

        _nameFilter = trimmed;
        await ResetToFirstPageAndReloadAsync();
    }

    private Task ApplyNameFilterOnBlur(FocusEventArgs _) => CommitNameFilterAsync();

    private async Task OnNameFilterKeyDown(KeyboardEventArgs e)
    {
        if (e.Key != "Enter")
            return;

        await CommitNameFilterAsync();
    }

    private async Task OpenCreateDialogAsync()
    {
        var parameters = new DialogParameters<CategoryFormDialog>
        {
            { x => x.ParentCandidates, _query.Data?.Items ?? [] },
            { x => x.SubmitText, "Create" }
        };
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            CloseButton = true
        };

        var dialog = await DialogService.ShowAsync<CategoryFormDialog>("New category", parameters, options);
        var result = await dialog.Result;

        if (result is null || result.Canceled || result.Data is not CategoryFormResult form)
            return;

        await CreateAsync(form);
    }

    private async Task OpenEditDialogAsync(CategoryResponse category)
    {
        if (string.IsNullOrWhiteSpace(category.Id))
            return;

        var parameters = new DialogParameters<CategoryFormDialog>
        {
            { x => x.ParentCandidates, _query.Data?.Items ?? [] },
            { x => x.Category, category },
            { x => x.SubmitText, "Save" }
        };
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            CloseButton = true
        };

        var dialog = await DialogService.ShowAsync<CategoryFormDialog>("Edit category", parameters, options);
        var result = await dialog.Result;

        if (result is null || result.Canceled || result.Data is not CategoryFormResult form)
            return;

        await UpdateAsync(category.Id, form);
    }

    private async Task CreateAsync(CategoryFormResult form)
    {
        _mutation = MutationState.Pending();
        _mutation = await MutationState.RunAsync(async () =>
        {
            var request = new CreateCategoryRequest
            {
                Name = form.Name,
                Kind = form.Kind,
                ParentId = form.ParentId,
                SortOrder = form.SortOrder
            };

            await Api.Api.Categories.PostAsync(request);
        });

        if (_mutation.Error is not null)
        {
            Snackbar.Add(_mutation.Error, Severity.Error, c => c.VisibleStateDuration = 5000);
            return;
        }

        if (_table is not null)
            await _table.ReloadServerData();
    }

    private async Task UpdateAsync(string categoryId, CategoryFormResult form)
    {
        _mutation = MutationState.Pending();
        _mutation = await MutationState.RunAsync(async () =>
        {
            var request = new UpdateCategoryRequest
            {
                Name = form.Name,
                Kind = form.Kind,
                ParentId = form.ParentId,
                SortOrder = form.SortOrder
            };

            await Api.Api.Categories[categoryId].PutAsync(request);
        });

        if (_mutation.Error is not null)
        {
            Snackbar.Add(_mutation.Error, Severity.Error, c => c.VisibleStateDuration = 5000);
            return;
        }

        if (_table is not null)
            await _table.ReloadServerData();
    }

    private Task ResetToFirstPageAndReloadAsync()
    {
        _tableVersion++;
        return InvokeAsync(StateHasChanged);
    }
}
