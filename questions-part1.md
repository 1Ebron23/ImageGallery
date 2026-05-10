# Kysymykset — Osa 1: Lokaali kehitys

Vastaa kysymyksiin omin sanoin. Lyhyet, selkeät vastaukset riittävät — tarkoitus on osoittaa, että olet ymmärtänyt konseptit.

---

## Clean Architecture

**1.** Selitä omin sanoin: mitä tarkoittaa, että `UploadPhotoUseCase` "ei tiedä" tallennetaanko kuva paikalliselle levylle vai Azureen? Näytä koodirivit, jotka osoittavat tämän.

> Vastauksesi:

> `UploadPhotoUseCase` käyttää `IStorageService`-rajapintaa, joka on abstraktio. Se ei tiedä konkreettista toteutusta — ei tarvitse tietää. Sovellus injektoi joko `LocalStorageService` tai `AzureBlobStorageService` riippuen konfiguraatiosta. Näin sovelluslogiikka on täysin riippumaton tallennusmekanismista.
>
> Koodirivit:
> ```csharp
> public UploadPhotoUseCase(IStorageService storageService, ...) 
> {
>     _storageService = storageService;  // Abstraktio, ei konkreetti toteutus
> }
> 
> // ExecuteAsync:ssa kutsutaan vain rajapintaa
> imageUrl = await _storageService.UploadAsync(...);
> ```

---

**2.** Miksi `IStorageService`-rajapinta on määritelty `GalleryApi.Domain`-kerroksessa, mutta `LocalStorageService` on `GalleryApi.Infrastructure`-kerroksessa? Mitä hyötyä tästä jaosta on?

> Vastauksesi:

> Domain-kerros sisältää sopimukset (rajapinnat) ja entiteetit — nämä kuuluvat ydinbisnekslogiikkaan ja niitä ei saisi riippua teknisistä yksityiskohdista. `IStorageService` on sopimus siitä, miten tiedostoita hallinnoidaan, mutta se ei mainitse tekniikkaa.
>
> Infrastructure-kerros sisältää toteutukset — `LocalStorageService` ja `AzureBlobStorageService` ovat konkreetteja teknisiä ratkaisuja. Näin Clean Architecture ehkäisee että Domain-kerros riippuu teknologiasta. Hyöty: voit korvata tallennustoteutusta muuttamatta sovelluslogiikkaa tai testejä.


---

**3.** Testit käyttävät `Mock<IAlbumRepository>`. Mitä mock-objekti tarkoittaa, ja miksi Clean Architecture tekee tämän testaustavan mahdolliseksi?

> Vastauksesi:

> Mock-objekti on testissä käytetty "tekopöytä", joka toteuttaa rajapintaa mutta ei tee oikeastaan mitään. Se palauttaa vain ennalta määritellyt testidatat. Mock:in avulla voidaan testata käyttötapausta ilman oikeata tietokantaa, jolloin testi on nopea ja riippumaton.
>
> Clean Architecture tekee tämän mahdolliseksi koska käyttötapaukset on koodattu rajapintojen (`IAlbumRepository`, `IStorageService`) kanssa, eivät konkreettisten toteutusten kanssa. Näin testissa voidaan antaa mock-toteutus ilman muutoksia sovelluslogiikkaan.


---

## Salaisuuksien hallinta

**4.** Kovakoodattu API-avain on ongelma, vaikka repositorio olisi yksityinen. Selitä kaksi eri syytä miksi.

> Vastauksesi:

> 1. **Git-historia.** Kun avain kerran commitoidaan, se jää git-historiaan ikuisesti. Vaikka poistaisit sen myöhemmässä commitissa, se löytyy silti `git log`-historian kautta. Kuka tahansa, jolla on pääsy repositorioon (vaikka se olisi myöhemmin joutunut julkiseksi), voi kaivaa sen esiin.
>
> 2. **Kehittäjät.** Jokainen, joka kloonaa projektin, saa avaimen nähtäväksi lähdekoodissa. Jos kehittäjä ottaa kannettavan varastoon tai kirjoittaa koodia kahvilassa, salaisuus on potentiaalisesti paljastettu. Lisäksi kaikki kehittäjät jakavat saman avaimen, joten ei voi jäljittää kuka käytti sitä.


---

**5.** Riittääkö se, että poistat kovakoodatun avaimen myöhemmässä commitissa? Perustele vastauksesi.

> Vastauksesi:

Ei, se ei riitä. Vaikka poistat avaimen koodista, se jää git-historiaan. Kuka tahansa voi ajaa `git log -p` tai `git log --all -S "sk-moderation"` ja löytää avaimen vanhasta versiosta. Avaimen on oltava poistettu historiasta, mikä vaatii `git rebase` tai `git filter-branch` -operaatiota, mutta sekin on riskialtista. Parempi ratkaisu on välittömästi nollata avain palveluntarjoajalla niin että vanha avain ei enää toimi.


---

**6.** Minne User Secrets tallennetaan käyttöjärjestelmässä? (Mainitse sekä Windows- että Linux/macOS-polut.) Miksi tämä sijainti on turvallinen?

> Vastauksesi:


> **Windows:** `%APPDATA%\Microsoft\UserSecrets\gallery-api-dev-secrets\secrets.json`
> 
> **Linux/macOS:** `~/.microsoft/usersecrets/gallery-api-dev-secrets/secrets.json`
>
> Nämä sijainnit ovat turvallisia koska:
> - Ne sijaitsevat käyttäjän kotihakemistossa, johon vain kyseinen käyttäjä pääsee
> - Ne eivät ole versionhallinnassa (git `clone` tai `fetch` ei lataa niitä)
> - Projektin `.csproj`-tunniste (`UserSecretsId`) yhdistää salaisuudet oikeaan projektiin
> - Näin salaisuudet pysyvät paikallisesti kehittäjän koneella, eivätkä päädy jaettuihin palvelimiin


---

## Options Pattern ja konfiguraatio

**7.** Mitä hyötyä on `IOptions<ModerationServiceOptions>`:n käyttämisestä verrattuna siihen, että luetaan arvo suoraan `IConfiguration`-rajapinnalta (`configuration["ModerationService:ApiKey"]`)?

> Vastauksesi:

> `IOptions<T>` tarjoaa merkittäviä etuja:
>
> 1. **Tyyppiturvallisuus.** `IConfiguration` palauttaa merkkijonon, ja väärä avain palauttaa `null`. `IOptions<ModerationServiceOptions>` on vahvasti tyypitetty — kääntäjä huomaa virheet.
> 
> 2. **IntelliSense.** Kun kirjoitat `options.Value.ApiKey`, IDE ehdottaa kenttiä automaattisesti. Merkkijonoavaimilla ei ole IntelliSensea.
>
> 3. **Dokumentaatio.** `ModerationServiceOptions`-luokka on dokumentaatio itsessään — näet heti mitä konfiguraatioita on olemassa. Merkkijonoavaimet ovat hajallaan koodissa.
>
> 4. **Testaaminen.** Testissa voit luoda `ModerationServiceOptions`-olion suoraan, ilman `IConfiguration`-mockkausta.


---

**8.** ASP.NET Core lukee konfiguraation useista lähteistä prioriteettijärjestyksessä. Listaa lähteet korkeimmasta matalimpaan ja selitä, mikä arvo lopulta käytetään, kun sama avain on sekä `appsettings.json`:ssa että User Secretsissä.

> Vastauksesi:

> Prioriteettijärjestys (korkea → matala):
> 1. User Secrets (kehitysmoodissa)
> 2. Ympäristömuuttujat
> 3. `appsettings.Development.json` (kehitysmoodissa)
> 4. `appsettings.json`
> 5. Koodiin kovakoodatut oletusarvot
>
> Jos avain on sekä `appsettings.json`:ssa että User Secretsissä, **User Secrets voittaa** koska se on korkeammalla prioriteettijärjestyksessä. Tämä on tarkoituksellista — User Secrets saa ylikirjoittaa kehitysasetukset. Näin `appsettings.json`:ssa voi olla tyhjä placeholder ja User Secrets tarjoaa oikean arvon.


---

**9.** `DependencyInjection.cs`:ssä valitaan tallennustoteutus näin:

```csharp
var provider = configuration["Storage:Provider"] ?? "local";
if (provider == "azure")
    services.AddScoped<IStorageService, AzureBlobStorageService>();
else
    services.AddScoped<IStorageService, LocalStorageService>();
```

Miksi käytetään konfiguraatioarvoa `env.IsDevelopment()`-tarkistuksen sijaan? Mitä haittaa olisi `if (env.IsDevelopment()) { käytä lokaalia }`-lähestymistavassa?

> Vastauksesi:

var provider = configuration["Storage:Provider"] ?? "local";
if (provider == "azure")
    services.AddScoped<IStorageService, AzureBlobStorageService>();
else
    services.AddScoped<IStorageService, LocalStorageService>();


---

## Tiedostotallennus

**10.** Kun lataat kuvan, `imageUrl`-kentän arvo on `/uploads/abc123-..../photo.jpg`. Miten tähän URL:iin pääsee selaimella? Mihin koodiin tämä perustuu?

> Vastauksesi:
Selaimella pääsee kirjoittamalla https://localhost:PORT/uploads/abc123-..../photo.jpg osoitepalkkiin. Kuvan näkyy koska Program.cs:ssä on app.UseStaticFiles().

UseStaticFiles() kertoo ASP.NET Corelle että wwwroot/-kansion sisältö tulee tarjoilla HTTP:nä. Kun LocalStorageService.UploadAsync() tallenaa tiedoston wwwroot/uploads/abc123.../photo.jpg:iin, selain pääsee siihen osoitteessa /uploads/....

// LocalStorageService.cs
var filePath = Path.Combine(_basePath, albumId.ToString(), fileName);
// _basePath = wwwroot/uploads

// Program.cs
app.UseStaticFiles();  // Tämä tarjoilee wwwroot/-kansion sisällön

---

**11.** Mitä tapahtuu jos yrität ladata tiedoston jonka MIME-tyyppi on `application/pdf`? Missä tiedostossa ja millä koodirivillä tämä käyttäytyminen on määritelty?

> Vastauksesi:

Lataus epäonnistuu ja palvelin palauttaa 400 Bad Request -virheen viestillä "Tiedostotyyppi 'application/pdf' ei ole sallittu."

Tämä on määritelty UploadPhotoUseCase.cs:ssä:

if (!AllowedContentTypes.Contains(request.ContentType))
    return Result<PhotoDto>.Failure(
        $"Tiedostotyyppi '{request.ContentType}' ei ole sallittu. " +
        $"Sallitut tyypit: {string.Join(", ", AllowedContentTypes)}");

---

**12.** `DeletePhotoUseCase` poistaa tiedoston kutsumalla `_storageService.DeleteAsync(photo.FileName, photo.AlbumId)` — ei `photo.ImageUrl`:lla. Miksi?

> Vastauksesi: 

Koska ImageUrl on vain polku, joka ei ole suoraan käytettävissä tiedoston poistamiseen eri tallennustoteutuksissa. Paikallisessa levytallennuksessa URL:ista ei voi suoraan lukea tiedostopolkua.

Tärkeämpi syy: Osassa 2, kun siirrytään Azure Blob Storageen, ImageUrl on julkinen HTTP-URL (esim. https://azureblob.../container/photo.jpg), mutta blob-objektin tunnus on eri (esim. vain photo.jpg tai albumId/photo.jpg). Tallennuspalvelu tarvitsee alkuperäisen tiedostonimen ja albumin ID:n, ei URL:ia.
