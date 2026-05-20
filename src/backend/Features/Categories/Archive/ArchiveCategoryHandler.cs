using backend.Common.Results;
using backend.Data;
using backend.Features.Categories.Shared;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Categories.Archive;

public sealed class ArchiveCategoryHandler(AppDbContext db)
{
    public async Task<Result> HandleAsync(ArchiveCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
            return Result.Failure(Error.NotFound("Category not found."));

        if (category.IsArchived)
            return Result.Success();

        var hasActiveChildren = await db.Categories
            .Where(c => c.ParentId == category.Id)
            .Active()
            .AnyAsync(cancellationToken);

        if (hasActiveChildren)
            return Result.Failure(Error.Validation("Cannot archive a category with active child categories."));

        category.IsArchived = true;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
