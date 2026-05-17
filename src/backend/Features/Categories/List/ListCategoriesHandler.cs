using backend.Common.Results;
using backend.Data;
using backend.Features.Categories;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Categories.List;

public sealed class ListCategoriesHandler(AppDbContext db)
{
    public async Task<Result<ListCategoriesResponse>> HandleAsync(
        ListCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        var query = db.Categories.AsNoTracking();

        if (request.Kind is not null)
            query = query.Where(c => c.Kind == request.Kind);

        if (!request.IncludeArchived)
            query = query.Where(c => !c.IsArchived);

        var categories = await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var items = categories.Select(CategoryMapping.ToResponse).ToList();
        return Result<ListCategoriesResponse>.Success(new ListCategoriesResponse(items));
    }
}
