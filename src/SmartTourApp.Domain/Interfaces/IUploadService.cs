namespace SmartTourApp.Domain.Interfaces;

public interface IUploadService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType);
    Task<string> UploadAudioAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteFileAsync(string fileUrl);
}
