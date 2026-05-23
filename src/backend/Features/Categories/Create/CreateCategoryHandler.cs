using backend.Application.Rules;
using backend.Common.Results;
using backend.Features.Categories;
using backend.Data;
using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Categories.Create;

public sealed class CreateCategoryHandler(AppDbContext db)
{
    public async Task<Result<CategoryResponse>> HandleAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var categoryId = Guid.NewGuid();

        var parentValidation = await CategoryRules.ValidateParentAsync(
            db,
            request.Kind,
            categoryId,
            request.ParentId,
            cancellationToken);

        if (!parentValidation.IsSuccess)
            return Result<CategoryResponse>.Failure(parentValidation.Error!);

        var category = new Category
        {
            Id = categoryId,
            Name = request.Name,
            Kind = request.Kind,
            Priority = request.Priority,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);

        if (category.ParentId is not null)
            await db.Entry(category).Reference(c => c.Parent).LoadAsync(cancellationToken);

        return Result<CategoryResponse>.Success(CategoryMapping.ToResponse(category));
    }
}
