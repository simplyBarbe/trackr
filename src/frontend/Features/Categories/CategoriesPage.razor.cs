using Microsoft.AspNetCore.Components;
using Microsoft.Kiota.Abstractions;
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

    private bool _loading = true;
    private bool _mutating;
    private string? _error;
    private IReadOnlyList<BackendFeaturesCategoriesCategoryResponse> _rows = [];

    private bool _includeArchived;
    private string _kindQuery = string.Empty;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        _loading = true;
        _error = null;
        try
        {
            var response = await Api.Api.Categories.GetAsync(configuration =>
                {
                    configuration.QueryParameters.IncludeArchived = _includeArchived;
                    if (!string.IsNullOrEmpty(_kindQuery))
                        configuration.QueryParameters.Kind = _kindQuery;
                },
                cancellationToken);

            var items = response?.Items ?? [];
            _rows = items;
        }
        catch (ApiException ex)
        {
            _error = ex.Message;
            _rows = [];
        }
        catch (Exception ex)
        {
            _error = ex.Message;
            _rows = [];
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task OnKindQueryChanged(string value)
    {
        _kindQuery = value;
        await LoadAsync();
    }

    private async Task OnIncludeArchivedChanged(bool value)
    {
        _includeArchived = value;
        await LoadAsync();
    }

    private async Task OpenCreateDialogAsync()
    {
        var parameters = new DialogParameters<CategoryFormDialog>
        {
            { x => x.ParentCandidates, _rows },
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

    private async Task OpenEditDialogAsync(BackendFeaturesCategoriesCategoryResponse category)
    {
        if (string.IsNullOrWhiteSpace(category.Id))
            return;

        var parameters = new DialogParameters<CategoryFormDialog>
        {
            { x => x.ParentCandidates, _rows },
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
        _mutating = true;
        _error = null;
        try
        {
            var request = new BackendFeaturesCategoriesCreateCreateCategoryRequest
            {
                Name = form.Name,
                Kind = form.Kind,
                ParentId = form.ParentId,
                SortOrder = form.SortOrder
            };

            await Api.Api.Categories.PostAsync(request);
            await LoadAsync();
        }
        catch (FastEndpointsErrorResponse ex)
        {
            _error = ex.Message;
        }
        catch (ApiException ex)
        {
            _error = ex.Message;
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _mutating = false;
        }
    }

    private async Task UpdateAsync(string categoryId, CategoryFormResult form)
    {
        _mutating = true;
        _error = null;
        try
        {
            var request = new BackendFeaturesCategoriesUpdateUpdateCategoryRequest
            {
                Name = form.Name,
                Kind = form.Kind,
                ParentId = form.ParentId,
                SortOrder = form.SortOrder
            };

            await Api.Api.Categories[categoryId].PutAsync(request);
            await LoadAsync();
        }
        catch (FastEndpointsErrorResponse ex)
        {
            _error = ex.Message;
        }
        catch (ApiException ex)
        {
            _error = ex.Message;
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _mutating = false;
        }
    }
}
