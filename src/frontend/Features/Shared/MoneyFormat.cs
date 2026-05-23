using System.Globalization;

namespace frontend.Features.Shared;

public static class MoneyFormat
{
    private static readonly CultureInfo EuroCulture = CultureInfo.GetCultureInfo("it-IT");

    public static string Format(decimal? amount) =>
        (amount ?? 0m).ToString("C", EuroCulture);
}
