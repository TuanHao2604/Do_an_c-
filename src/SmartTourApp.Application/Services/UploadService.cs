using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.Application.Services;

/// <summary>
/// Local file storage service. Saves uploads to wwwroot/uploads/.
/// Replace with Supabase/Azure/S3 for production.
/// </summary>
public class UploadService : IUploadService
{
    private readonly string _uploadRoot;

    public UploadService(IWebHostEnvironmentAccessor env)
    {
        _uploadRoot = Path.Combine(env.WebRootPath, "uploads");
        Directory.CreateDirectory(Path.Combine(_uploadRoot, "images"));
        Directory.CreateDirectory(Path.Combine(_uploadRoot, "audios"));
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType)
    {
        var uniqueName = $"images/{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var fullPath = Path.Combine(_uploadRoot, uniqueName);
        await using var fs = File.Create(fullPath);
        await fileStream.CopyToAsync(fs);
        return $"/uploads/{uniqueName}";
    }

    public async Task<string> UploadAudioAsync(Stream fileStream, string fileName, string contentType)
    {
        var uniqueName = $"audios/{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var fullPath = Path.Combine(_uploadRoot, uniqueName);
        await using var fs = File.Create(fullPath);
        await fileStream.CopyToAsync(fs);
        return $"/uploads/{uniqueName}";
    }

    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return Task.FromResult(false);
        var relativePath = fileUrl.TrimStart('/').Replace("uploads/", "");
        var fullPath = Path.Combine(_uploadRoot, relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}

/// <summary>
/// Simple accessor for IWebHostEnvironment.WebRootPath to avoid
/// direct coupling to ASP.NET Core in the Application layer.
/// </summary>
public interface IWebHostEnvironmentAccessor
{
    string WebRootPath { get; }
}
