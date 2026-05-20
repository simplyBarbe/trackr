using FluentValidation;

namespace backend.Common.Pagination;

public static class PaginationValidationExtensions
{
    public static void AddPaginationRules<T>(this AbstractValidator<T> validator)
        where T : PagedRequest
    {
        validator.RuleFor(x => x.Page).GreaterThan(0);
        validator.RuleFor(x => x.PageSize).InclusiveBetween(1, PaginationDefaults.MaxPageSize);
    }
}
