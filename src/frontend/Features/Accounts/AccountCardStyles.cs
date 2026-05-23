using Trackr.Api.Models;

namespace frontend.Features.Accounts;

public static class AccountCardStyles
{
    private static readonly IReadOnlyDictionary<AccountColor, string> PaletteNames =
        new Dictionary<AccountColor, string>
        {
            [AccountColor.Primary] = "primary",
            [AccountColor.Secondary] = "secondary",
            [AccountColor.Tertiary] = "tertiary",
            [AccountColor.Info] = "info",
            [AccountColor.Success] = "success",
            [AccountColor.Warning] = "warning"
        };

    public static string GetCardStyle(AccountColor? color)
    {
        var name = color is not null && PaletteNames.TryGetValue(color.Value, out var palette)
            ? palette
            : "primary";
        return $"background-color: var(--mud-palette-{name}); color: var(--mud-palette-{name}-contrast-text);";
    }

    public static string? GetBalanceStyle(AccountResponse account) =>
        (account.Balance ?? 0m) < 0 ? "color: #ffcdd2; font-weight: 600;" : "font-weight: 600;";
}
