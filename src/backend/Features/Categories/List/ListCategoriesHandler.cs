using backend.Common.Results;
using backend.Data;
using backend.Features.Categories;
using backend.Features.Categories.Shared;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Categories.List;

public sealed class ListCategoriesHandler(AppDbContext db)
{
    public async Task<Result<ListCategoriesResponse>> HandleAsync(
        ListCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        var query = db.Categories.AsNoTracking();

        if (request.IncludeArchived != true)
            query = query.Active();

        query = query
            .OfKind(request.Kind)
            .MatchingName(request.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var categories = await query
            .Include(c => c.Parent)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = categories.Select(CategoryMapping.ToResponse).ToList();
        return Result<ListCategoriesResponse>.Success(
            new ListCategoriesResponse(items, request.Page, request.PageSize, totalCount));
    }
}
