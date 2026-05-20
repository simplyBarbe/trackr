namespace backend.Common;

internal static class SqlLike
{
    /// <summary>
    /// Builds a LIKE pattern for substring match (%term%) with % and _ escaped for use with EF.Functions.Like(..., escapeChar: "\\").
    /// </summary>
    internal static string ContainsPattern(string trimmedTerm)
    {
        return "%" + Escape(trimmedTerm) + "%";
    }

    private static string Escape(string term) =>
        term.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
