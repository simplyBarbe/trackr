using Trackr.Api.Models;

namespace frontend.Features.Accounts;

public static class AccountCardStyles
{
    private static readonly IReadOnlyDictionary<AccountColor, string> AccentHex =
        new Dictionary<AccountColor, string>
        {
            [AccountColor.Primary] = "#059669",
            [AccountColor.Secondary] = "#64748b",
            [AccountColor.Tertiary] = "#0f766e",
            [AccountColor.Info] = "#0284c7",
            [AccountColor.Success] = "#16a34a",
            [AccountColor.Warning] = "#d97706"
        };

    public static string GetBorderLeftStyle(AccountColor? color)
    {
        var hex = color is not null && AccentHex.TryGetValue(color.Value, out var value)
            ? value
            : "#059669";
        return $"border-left: 4px solid {hex};";
    }

    /// <summary>Swatch for color picker in account form dialog.</summary>
    public static string GetSwatchStyle(AccountColor color) =>
        $"background-color: {GetAccentHex(color)}; width: 1rem; height: 1rem; border-radius: 50%; display: inline-block;";

    public static string? GetBalanceStyle(AccountResponse account) =>
        (account.Balance ?? 0m) < 0
            ? "color: var(--mud-palette-error); font-weight: 600;"
            : "font-weight: 600;";

    private static string GetAccentHex(AccountColor color) =>
        AccentHex.TryGetValue(color, out var hex) ? hex : "#059669";
}
