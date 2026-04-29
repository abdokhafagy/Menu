namespace Menu.UI.Services;

/// <summary>
/// Exception thrown by ApiService when an HTTP error occurs.
/// </summary>
public sealed class ApiServiceException : Exception
{
    public int StatusCode { get; }
    public string? ResponseContent { get; }

    public ApiServiceException(int statusCode, string? message = null, string? responseContent = null)
        : base(message ?? $"API request failed with status code {statusCode}")
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }
}
