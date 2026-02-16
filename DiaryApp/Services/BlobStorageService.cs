using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace DiaryApp.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string containerName = "imagenes");
        Task<bool> DeleteImageAsync(string blobUrl, string containerName = "imagenes");
    }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string containerName = "imagenes")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders 
                { 
                    ContentType = file.ContentType 
                });
            }

            return blobClient.Uri.ToString();
        }

        public async Task<bool> DeleteImageAsync(string blobUrl, string containerName = "imagenes")
        {
            if (string.IsNullOrEmpty(blobUrl))
                return false;

            try
            {
                var uri = new Uri(blobUrl);
                var blobName = Path.GetFileName(uri.LocalPath);
                
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                
                return await blobClient.DeleteIfExistsAsync();
            }
            catch
            {
                return false;
            }
        }
    }
}