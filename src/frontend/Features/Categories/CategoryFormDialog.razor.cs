using Microsoft.AspNetCore.Components;
using MudBlazor;
using Trackr.Api.Models;

namespace frontend.Features.Categories;

public partial class CategoryFormDialog : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public IReadOnlyList<CategoryResponse> ParentCandidates { get; set; } = [];

    [Parameter]
    public CategoryResponse? Category { get; set; }

    [Parameter]
    public string SubmitText { get; set; } = "Create";

    private MudForm? _form;
    private string _name = string.Empty;
    private CategoryKind _kind = CategoryKind.Expense;
    private string _parentId = string.Empty;
    private int _sortOrder;

    private IEnumerable<CategoryResponse> ParentOptions =>
        ParentCandidates
            .Where(c => c.IsArchived != true && c.Kind == _kind)
            .Where(c => c.Id != Category?.Id)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name);

    protected override void OnParametersSet()
    {
        if (Category is null)
            return;

        _name = Category.Name ?? string.Empty;
        _kind = Category.Kind ?? CategoryKind.Expense;
        _parentId = Category.ParentId ?? string.Empty;
        _sortOrder = Category.SortOrder ?? 0;
    }

    private void OnKindChanged(CategoryKind kind)
    {
        _kind = kind;
        if (!ParentOptions.Any(c => c.Id == _parentId))
            _parentId = string.Empty;
    }

    private void OnParentIdChanged(string parentId)
    {
        _parentId = parentId;
    }

    private async Task SubmitAsync()
    {
        if (_form is null)
            return;

        await _form.ValidateAsync();
        if (!_form.IsValid)
            return;

        var result = new CategoryFormResult(
            _name.Trim(),
            _kind,
            string.IsNullOrWhiteSpace(_parentId) ? null : _parentId,
            _sortOrder);

        MudDialog.Close(DialogResult.Ok(result));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

public sealed record CategoryFormResult(
    string Name,
    CategoryKind Kind,
    string? ParentId,
    int SortOrder);
