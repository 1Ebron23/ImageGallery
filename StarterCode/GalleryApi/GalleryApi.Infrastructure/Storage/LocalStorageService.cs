using GalleryApi.Domain.Interfaces;
using GalleryApi.Infrastructure.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace GalleryApi.Infrastructure.Storage;

public class LocalStorageService : IStorageService
{
    private readonly string _basePath;


    // TODO (Vaihe 5): Lisää konstruktori ja toteutus tähän.
    //
    // Tarvitset seuraavat injektiot:
    //   IWebHostEnvironment env       — antaa ContentRootPath (sovelluksen juurihakemisto)
    //   IOptions<StorageOptions> opts — antaa Storage:BasePath konfiguraatiosta
    //
    // Esimerkki konstruktorista:
    public LocalStorageService(IWebHostEnvironment env, IOptions<StorageOptions> opts)
    {
        _basePath = Path.Combine(env.ContentRootPath, opts.Value.BasePath);
    }
    //
    // UploadAsync-metodi:
    //   1. Muodosta kansio: Path.Combine(_basePath, albumId.ToString())
    //   2. Luo kansio: Directory.CreateDirectory(albumDir)
    //   3. Muodosta tiedostopolku: Path.Combine(albumDir, fileName)
    //   4. Kirjoita Stream tiedostoon:
    //        using var output = File.Create(filePath);
    //        await fileStream.CopyToAsync(output);
    //   5. Palauta URL: $"/uploads/{albumId}/{fileName}"
    //
    // DeleteAsync-metodi:
    //   1. Muodosta polku: Path.Combine(_basePath, albumId.ToString(), fileName)
    //   2. Tarkista onko tiedosto olemassa: File.Exists(filePath)
    //   3. Poista: File.Delete(filePath)

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, Guid albumId)
    {
        // Luo albumikohtainen kansio
        var albumDir = Path.Combine(_basePath, albumId.ToString());
        Directory.CreateDirectory(albumDir);

        // Kirjoita tiedosto
        var filePath = Path.Combine(albumDir, fileName);
        using var output = File.Create(filePath);
        await fileStream.CopyToAsync(output);

        // Palauta URL — UseStaticFiles() tarjoilee wwwroot/-kansion
        return $"/uploads/{albumId}/{fileName}";
    }

    public Task DeleteAsync(string fileName, Guid albumId)
    {
        var filePath = Path.Combine(_basePath, albumId.ToString(), fileName);
        if (File.Exists(filePath))
            File.Delete(filePath);
        return Task.CompletedTask;
    }
}
