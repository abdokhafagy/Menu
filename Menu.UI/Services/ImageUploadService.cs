using System.Net.Http.Headers;
using System.Net.Http.Json;

using Menu.UI.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace Menu.UI.Services;

public sealed class ImageUploadService
{
    private const long MaxImageSizeBytes = 2 * 1024 * 1024;

    private readonly IHttpClientFactory _httpClientFactory;

    public ImageUploadService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> UploadImageAsync(IBrowserFile file, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("MenuApi");

        await using var fileStream = file.OpenReadStream(MaxImageSizeBytes, ct);
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

        using var formData = new MultipartFormDataContent();
        formData.Add(streamContent, "file", file.Name);

        var response = await client.PostAsync("api/files/images", formData, ct);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<ImageUploadResponse>>(cancellationToken: ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(payload?.Message ?? "Failed to upload image.");
        }

        if (payload is null || !payload.Success || payload.Data is null || string.IsNullOrWhiteSpace(payload.Data.Url))
        {
            throw new InvalidOperationException(payload?.Message ?? "Image upload response was invalid.");
        }

        return payload.Data.Url;
    }

    public static bool IsValidImageType(string contentType)
    {
        return contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase)
               || contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase)
               || contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase)
               || contentType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsValidImageSize(long size)
    {
        return size > 0 && size <= MaxImageSizeBytes;
    }
}

public sealed record ImageUploadResponse(string Url, string FileName, long Size);
