namespace DiaryApp.Mobile.Services;

/// <summary>
/// Local file storage implementation for when Azure Blob Storage is not configured.
/// Stores files in the app's local data directory.
/// </summary>
public class LocalBlobStorageService : IBlobStorageService
{
    private readonly string _localStoragePath;

    public LocalBlobStorageService()
    {
        // Create a local directory for storing images
        _localStoragePath = Path.Combine(FileSystem.AppDataDirectory, "LocalImages");
        
        // Ensure the directory exists
        if (!Directory.Exists(_localStoragePath))
        {
            Directory.CreateDirectory(_localStoragePath);
        }
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string containerName = "imagenes")
    {
        if (imageStream == null || imageStream.Length == 0)
            throw new ArgumentException("Stream is empty");

        try
        {
            // Create container subdirectory if needed
            var containerPath = Path.Combine(_localStoragePath, containerName);
            if (!Directory.Exists(containerPath))
            {
                Directory.CreateDirectory(containerPath);
            }

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var filePath = Path.Combine(containerPath, uniqueFileName);

            // Save the stream to local file
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            imageStream.Position = 0;
            await imageStream.CopyToAsync(fileStream);

            // Return the local file path as a "file://" URL
            return $"file://{filePath}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error saving file locally: {ex.Message}");
            throw;
        }
    }

    public Task<bool> DeleteImageAsync(string blobUrl, string containerName = "imagenes")
    {
        if (string.IsNullOrEmpty(blobUrl))
            return Task.FromResult(false);

        try
        {
            // Extract file path from file:// URL
            var filePath = blobUrl.Replace("file://", "");
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error deleting file locally: {ex.Message}");
            return Task.FromResult(false);
        }
    }
}