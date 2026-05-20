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

        if (!request.IncludeArchived)
            query = query.Active();

        query = query
            .OfKind(request.Kind)
            .MatchingName(request.Name);

        var categories = await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var items = categories.Select(CategoryMapping.ToResponse).ToList();
        return Result<ListCategoriesResponse>.Success(new ListCategoriesResponse(items));
    }
}
