namespace DiaryApp.Core.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string containerName = "imagenes");
    Task<bool> DeleteImageAsync(string blobUrl, string containerName = "imagenes");
}