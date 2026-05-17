using backend.Common.Results;
using backend.Data;
using backend.Data.Entities;
using backend.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Rules;

public static class CategoryRules
{
    private const int MaxDepth = 10;

    public static Result ValidateNotArchived(Category category)
    {
        if (category.IsArchived)
            return Result.Failure(Error.Validation("Category is archived."));

        return Result.Success();
    }

    public static async Task<Result> ValidateParentAsync(
        AppDbContext db,
        CategoryKind kind,
        Guid categoryId,
        Guid? parentId,
        CancellationToken cancellationToken = default)
    {
        if (parentId is null)
            return Result.Success();

        if (parentId == categoryId)
            return Result.Failure(Error.Validation("A category cannot be its own parent."));

        var parent = await db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == parentId, cancellationToken);

        if (parent is null)
            return Result.Failure(Error.NotFound("Parent category not found."));

        if (parent.IsArchived)
            return Result.Failure(Error.Validation("Parent category is archived."));

        if (parent.Kind != kind)
            return Result.Failure(Error.Validation("Parent category must have the same kind."));

        var depth = 1;
        var currentParentId = parent.ParentId;

        while (currentParentId is not null)
        {
            if (currentParentId == categoryId)
                return Result.Failure(Error.Validation("Category hierarchy cannot contain a cycle."));

            if (depth >= MaxDepth)
                return Result.Failure(Error.Validation($"Category hierarchy cannot exceed {MaxDepth} levels."));

            var ancestor = await db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == currentParentId, cancellationToken);

            if (ancestor is null)
                break;

            currentParentId = ancestor.ParentId;
            depth++;
        }

        return Result.Success();
    }
}
