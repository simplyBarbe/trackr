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

    private bool _loading = true;
    private HealthProbeResult? _health;
    private string _apiBaseUrl = string.Empty;
    private string _clientMode = "Kiota";

    protected override async Task OnInitializedAsync()
    {
        _apiBaseUrl = Configuration["ApiBaseUrl"] ?? string.Empty;

        _health = await HealthProbe.GetAsync();
        _loading = false;
    }
}
