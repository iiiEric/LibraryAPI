using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;

namespace LibraryAPI.Services
{
    public class AzureFileStorageService : IFileStorageService
    {
        private readonly string _connectionString;

        public AzureFileStorageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureStorageConnection")!;
        }

        public async Task Delete(string? path, string container)
        {
            if (string.IsNullOrEmpty(path))
                return;

            var client = new BlobContainerClient(_connectionString, container);
            await client.CreateIfNotExistsAsync();
            var fileName = Path.GetFileName(path);
            var blobClient = client.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<string> Store(string container, IFormFile file)
        {
            var client = new BlobContainerClient(_connectionString, container);
            await client.CreateIfNotExistsAsync();
            client.SetAccessPolicy(PublicAccessType.Blob);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var blobClient = client.GetBlobClient(fileName);
            var blobHttpHeaders = new BlobHttpHeaders()
            {
                ContentType = file.ContentType
            };
            await blobClient.UploadAsync(file.OpenReadStream(), blobHttpHeaders);
            return blobClient.Uri.ToString();
        }
    }
}
