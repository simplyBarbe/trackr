using backend.Application.Rules;
using backend.Common.Results;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Categories.Update;

public sealed class UpdateCategoryHandler(AppDbContext db)
{
    public async Task<Result<CategoryResponse>> HandleAsync(
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
            return Result<CategoryResponse>.Failure(Error.NotFound("Category not found."));

        var notArchived = CategoryRules.ValidateNotArchived(category);
        if (!notArchived.IsSuccess)
            return Result<CategoryResponse>.Failure(notArchived.Error!);

        var parentValidation = await CategoryRules.ValidateParentAsync(
            db,
            request.Kind,
            category.Id,
            request.ParentId,
            cancellationToken);

        if (!parentValidation.IsSuccess)
            return Result<CategoryResponse>.Failure(parentValidation.Error!);

        category.Name = request.Name;
        category.Kind = request.Kind;
        category.Priority = request.Priority;
        category.ParentId = request.ParentId;
        category.SortOrder = request.SortOrder;

        await db.SaveChangesAsync(cancellationToken);

        if (category.ParentId is not null)
            await db.Entry(category).Reference(c => c.Parent).LoadAsync(cancellationToken);

        return Result<CategoryResponse>.Success(CategoryMapping.ToResponse(category));
    }
}
