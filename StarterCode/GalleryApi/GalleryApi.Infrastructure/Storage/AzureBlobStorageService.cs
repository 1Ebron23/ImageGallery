using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GalleryApi.Domain.Interfaces;
using GalleryApi.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace GalleryApi.Infrastructure.Storage;

// TODO (Part 2, Vaihe 2): Toteuta Azure Blob Storage -integraatio.
//
// Tarvitset NuGet-paketit:
//   dotnet add package Azure.Storage.Blobs
//   dotnet add package Azure.Identity
//
// Azure.Identity tarjoaa DefaultAzureCredential-luokan, joka toimii
// automaattisesti sekä lokaalisti (Azure CLI -kirjautuminen) että
// Azuressa (Managed Identity).
//
// Konstruktori:
//   public AzureBlobStorageService(IOptions<StorageOptions> options)
//   {
//       var accountName = options.Value.AccountName;
//       var containerName = options.Value.ContainerName;
//       var serviceClient = new BlobServiceClient(
//           new Uri($"https://{accountName}.blob.core.windows.net"),
//           new DefaultAzureCredential());
//       _containerClient = serviceClient.GetBlobContainerClient(containerName);
//   }

public class AzureBlobStorageService : IStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(IOptions<StorageOptions> options)
    {
        var accountName = options.Value.AccountName;
        var containerName = options.Value.ContainerName;

        var serviceClient = new BlobServiceClient(
            new Uri($"https://{accountName}.blob.core.windows.net"),
            new DefaultAzureCredential());

        _containerClient = serviceClient.GetBlobContainerClient(containerName);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, Guid albumId)
    {
        var blobName = $"{albumId}/{fileName}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(
            fileStream,
            new BlobHttpHeaders { ContentType = contentType });

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string fileName, Guid albumId)
    {
        var blobName = $"{albumId}/{fileName}";
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}