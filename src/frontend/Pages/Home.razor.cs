using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace frontend.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    private IHealthProbe HealthProbe { get; set; } = null!;

    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    private bool _loading = true;
    private HealthProbeResult? _health;
    private string _apiBaseUrl = string.Empty;
    private string _clientMode = "HttpClient (run scripts/generate-api.ps1 for Kiota)";

    protected override async Task OnInitializedAsync()
    {
        _apiBaseUrl = Configuration["ApiBaseUrl"] ?? string.Empty;

#if KIOTA_GENERATED
        _clientMode = "Kiota";
#endif

        _health = await HealthProbe.GetAsync();
        _loading = false;
    }
}
