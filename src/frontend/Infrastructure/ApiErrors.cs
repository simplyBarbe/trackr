using Microsoft.Kiota.Abstractions;
using Trackr.Api.Models;

namespace frontend.Infrastructure;

public static class ApiErrors
{
    public static string GetMessage(Exception ex) => ex switch
    {
        FastEndpointsErrorResponse fe => fe.Message,
        ApiException api => api.Message,
        _ => ex.Message
    };
}
