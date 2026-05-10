using GalleryApi.Domain.Interfaces;
using GalleryApi.Infrastructure.Options;
using GalleryApi.Infrastructure.Persistence;
using GalleryApi.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GalleryApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Tietokanta (SQLite kehityksessä, voidaan vaihtaa SQL Serveriin tuotannossa)
        services.AddDbContext<GalleryDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("Default") ?? "Data Source=gallery.db"));

        // Repositoriot
        services.AddScoped<IAlbumRepository, AlbumRepository>();
        services.AddScoped<IPhotoRepository, PhotoRepository>();

        // TILAPÄINEN rekisteröinti — sovellus käynnistyy, mutta kuvien lataus ei vielä toimi.
        // TODO (Vaihe 5): Toteuta LocalStorageService (palauttaa NotImplementedException tähän asti).
        // TODO (Vaihe 6): Korvaa tämä rivi ehdollisella logiikalla käyttäen StorageOptions-vakioita:
        //
        //   var provider = configuration[$"{StorageOptions.SectionName}:Provider"]
        //       ?? StorageOptions.LocalProvider;
        //   if (provider == StorageOptions.AzureProvider)
        //       services.AddScoped<IStorageService, AzureBlobStorageService>();
        //   else
        //       services.AddScoped<IStorageService, LocalStorageService>();
        var provider = configuration[$"{StorageOptions.SectionName}:Provider"]
            ?? StorageOptions.LocalProvider;
        if (provider == StorageOptions.AzureProvider)
            services.AddScoped<IStorageService, AzureBlobStorageService>(); // Osa 2
        else
            services.AddScoped<IStorageService, LocalStorageService>();     // Osa 1 — tämä nyt

        return services;
    }
}
