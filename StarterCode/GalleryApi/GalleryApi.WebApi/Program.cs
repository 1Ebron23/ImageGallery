using Azure.Identity;
using GalleryApi.Application;
using GalleryApi.Infrastructure;
using GalleryApi.Infrastructure.Moderation;
using GalleryApi.Infrastructure.Options;
using GalleryApi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var keyVaultUrl = builder.Configuration["KeyVault:VaultUrl"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}

// ============================================================
// ONGELMA: API-avain on kovakoodattu suoraan lähdekoodiin!
//
// Tämä tarkoittaa:
//   - Avain näkyy kaikille Git-repositorion käyttäjille
//   - Avain päätyy versionhallintaan ja säilyy siellä ikuisesti
//   - Jos repositorio on julkinen, avain on kaikille näkyvillä
//
// Tehtäväsi Vaiheessa 3 (README-Part1.md): Korvaa tämä User Secrets -ratkaisulla.
// Tehtäväsi Vaiheessa 4 (README-Part1.md): Korvaa tämä Options Pattern -ratkaisulla.
// ============================================================

builder.Services.Configure<ModerationServiceOptions>(
    builder.Configuration.GetSection(ModerationServiceOptions.SectionName));

// ModerationServiceClient saa asetukset DI:n kautta
builder.Services.AddSingleton<ModerationServiceClient>();

// Konfiguraatio-osiot (Options Pattern)
builder.Services.Configure<StorageOptions>(
    builder.Configuration.GetSection(StorageOptions.SectionName));

// Sovellus- ja infrastruktuurikerrokset
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Gallery API",
        Version = "v1",
        Description = "Kuvakirjasto API — Clean Architecture -esimerkki"
    });
});

var app = builder.Build();

// Tietokanta — luodaan automaattisesti jos ei ole olemassa
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GalleryDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Staattisten tiedostojen jako — tarjoilee wwwroot-kansion sisällön
// Tarvitaan paikallisesti tallennettujen kuvien näyttämiseen
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

app.Run();
