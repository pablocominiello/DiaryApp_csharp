using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DiaryApp.Core.Interfaces;

namespace DiaryApp.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string containerName = "imagenes")
        {
            if (imageStream == null || imageStream.Length == 0)
                throw new ArgumentException("Stream is empty");

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            await blobClient.UploadAsync(imageStream, new BlobHttpHeaders
            {
                ContentType = GetContentType(fileName)
            });

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

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }
    }
}