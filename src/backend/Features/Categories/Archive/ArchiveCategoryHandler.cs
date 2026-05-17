using backend.Application.Rules;
using backend.Common.Results;
using backend.Data;
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
            .AnyAsync(c => c.ParentId == category.Id && !c.IsArchived, cancellationToken);

        if (hasActiveChildren)
            return Result.Failure(Error.Validation("Cannot archive a category with active child categories."));

        category.IsArchived = true;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
