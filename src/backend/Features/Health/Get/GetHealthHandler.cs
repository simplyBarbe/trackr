using backend.Common.Results;
using backend.Data;

namespace backend.Features.Health.Get;

public sealed class GetHealthHandler(AppDbContext db)
{
    public async Task<Result<GetHealthResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        try
        {
            var canConnect = await db.Database.CanConnectAsync(cancellationToken);
            var database = canConnect ? "ok" : "degraded";
            var status = canConnect ? "ok" : "degraded";
            return Result<GetHealthResponse>.Success(new GetHealthResponse(status, database));
        }
        catch (Exception ex)
        {
            return Result<GetHealthResponse>.Failure(
                Error.Unexpected($"Database health check failed: {ex.Message}"));
        }
    }
}
