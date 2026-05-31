using Microsoft.Kiota.Abstractions;

namespace frontend.Features.Home;

public sealed record DashboardDateRange(Date? From = null, Date? To = null)
{
    public void ApplyTo(Action<Date?> setFrom, Action<Date?> setTo)
    {
        if (From is not null)
            setFrom(From);

        if (To is not null)
            setTo(To);
    }
}
