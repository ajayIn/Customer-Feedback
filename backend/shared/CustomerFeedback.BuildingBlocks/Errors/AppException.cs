namespace CustomerFeedback.BuildingBlocks.Errors;

public sealed class AppException : Exception
{
    public int Status { get; }
    public string Title { get; }
    public IDictionary<string, string[]>? Errors { get; }

    public AppException(int status, string title, string detail, IDictionary<string, string[]>? errors = null)
        : base(detail)
    {
        Status = status;
        Title = title;
        Errors = errors;
    }

    public static AppException Validation(IDictionary<string, string[]> errors) =>
        new(400, "Validation failed", "One or more fields are invalid.", errors);

    public static AppException Unauthorized(string detail = "Authentication is required.") =>
        new(401, "Unauthorized", detail);

    public static AppException Forbidden(string detail = "You are not allowed to perform this action.") =>
        new(403, "Forbidden", detail);

    public static AppException NotFound(string detail = "The requested resource was not found.") =>
        new(404, "Not found", detail);

    public static AppException Conflict(string detail) =>
        new(409, "Conflict", detail);
}
