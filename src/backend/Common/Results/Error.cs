namespace backend.Common.Results;

public sealed record Error(string Code, string Message, int Status = StatusCodes.Status400BadRequest)
{
    public static Error NotFound(string message) =>
        new("not_found", message, StatusCodes.Status404NotFound);

    public static Error Validation(string message) =>
        new("validation_error", message, StatusCodes.Status400BadRequest);

    public static Error Conflict(string message) =>
        new("conflict", message, StatusCodes.Status409Conflict);

    public static Error Unexpected(string message) =>
        new("unexpected_error", message, StatusCodes.Status500InternalServerError);
}
