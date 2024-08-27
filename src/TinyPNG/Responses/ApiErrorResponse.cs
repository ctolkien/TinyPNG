namespace TinyPng.Responses;

/// <summary>
/// Represents an API error response.
/// </summary>
public class ApiErrorResponse
{
    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; }
}
