using backend.Common.Results;
using backend.Data;
using backend.Features.Categories;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Categories.Get;

public sealed class GetCategoryHandler(AppDbContext db)
{
    public async Task<Result<CategoryResponse>> HandleAsync(
        GetCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
            return Result<CategoryResponse>.Failure(Error.NotFound("Category not found."));

        return Result<CategoryResponse>.Success(CategoryMapping.ToResponse(category));
    }
}
