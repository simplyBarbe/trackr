using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace frontend.Features.Home;

public partial class Home : ComponentBase
{
    [Inject]
    private IHealthProbe HealthProbe { get; set; } = null!;

    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    private QueryState<HealthProbeResult> _query = QueryState<HealthProbeResult>.Loading();

    private string _apiBaseUrl = string.Empty;
    private string _clientMode = "Kiota";

    protected override async Task OnInitializedAsync()
    {
        _apiBaseUrl = Configuration["ApiBaseUrl"] ?? string.Empty;

        _query = await QueryState<HealthProbeResult>.RunAsync(async () =>
        {
            var health = await HealthProbe.GetAsync();
            return health ?? throw new InvalidOperationException("API unreachable.");
        });
    }
}
