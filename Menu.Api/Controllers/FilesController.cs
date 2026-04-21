using Menu.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private const long MaxImageSizeBytes = 2 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private readonly IWebHostEnvironment _environment;

    public FilesController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost("images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxImageSizeBytes + 128 * 1024)]
    public async Task<ActionResult<ApiResponse<ImageUploadResponse>>> UploadImage(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<ImageUploadResponse>.FailResponse("Please select an image file."));
        }

        if (file.Length > MaxImageSizeBytes)
        {
            return BadRequest(ApiResponse<ImageUploadResponse>.FailResponse("Image size must be 2 MB or less."));
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return BadRequest(ApiResponse<ImageUploadResponse>.FailResponse("Only JPG, PNG, and WEBP images are allowed."));
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse<ImageUploadResponse>.FailResponse("Invalid image content type."));
        }

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            Directory.CreateDirectory(webRootPath);
        }

        var uploadsFolder = Path.Combine(webRootPath, "uploads");
        Directory.CreateDirectory(uploadsFolder);

        var generatedFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var physicalPath = Path.Combine(uploadsFolder, generatedFileName);

        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(stream, ct);
        }

        var relativeUrl = $"/uploads/{generatedFileName}";
        var absoluteUrl = $"{Request.Scheme}://{Request.Host}{relativeUrl}";

        var response = new ImageUploadResponse(absoluteUrl, generatedFileName, file.Length);
        return Ok(ApiResponse<ImageUploadResponse>.SuccessResponse(response, "Image uploaded successfully."));
    }
}

public sealed record ImageUploadResponse(string Url, string FileName, long Size);
