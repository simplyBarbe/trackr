using backend.Common;
using backend.Data.Entities;
using backend.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Categories.Shared;

public static class CategoryQueryExtensions
{
    public static IQueryable<Category> Active(this IQueryable<Category> query)
        => query.Where(c => !c.IsArchived);

    public static IQueryable<Category> OfKind(this IQueryable<Category> query, CategoryKind? kind)
        => kind is null ? query : query.Where(c => c.Kind == kind);

    public static IQueryable<Category> MatchingName(this IQueryable<Category> query, string? name)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return query;

        var pattern = SqlLike.ContainsPattern(trimmed);
        return query.Where(c => EF.Functions.Like(c.Name, pattern, "\\"));
    }
}
