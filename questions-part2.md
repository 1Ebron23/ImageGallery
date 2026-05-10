# Kysymykset — Osa 2: Azure-julkaisu

Vastaa kysymyksiin omin sanoin. Lyhyet, selkeät vastaukset riittävät.

---

## Azure Blob Storage

**1.** Mitä eroa on `LocalStorageService.UploadAsync`:n ja `AzureBlobStorageService.UploadAsync`:n palauttamilla URL-arvoilla? Miksi ne eroavat?

> Vastauksesi:
> LocalStorageService palautaa suhteelisen URL:n esim. `/uploads/albumId/photo.jpg`, joka on palvelimen sisällä. AzureBlobStorageService palautaa täyden URL:n esim. `https://stgallery....blob.core.windows.net/photos/albumId/photo.jpg`. Ne eroavat koska paikallinen tallennus käyttää palvelimen levyä, mutta Azure käyttää pilvi-infrastruktuuria missä tiedostot on osoitettu verkon kautta. Paikallisesti voimme käyttää suhteelisia reittejä, mutta pilvessä tarvitaan täydet URI:t.


---

**2.** `AzureBlobStorageService` luo `BlobServiceClient`:n käyttäen `DefaultAzureCredential()` eikä yhteysmerkkijonoa. Mitä etua tästä on? Mitä `DefaultAzureCredential` tekee eri ympäristöissä?

> Vastauksesi:

> DefaultAzureCredential:n etuna on että se hoitaa tunnistautumisen automaattisesti riippuen ympäristöstä. Kehityskonella se käyttää Azure CLI:ä tai VS:n kirjautumista, mutta Azuressa se käyttää Managed Identitya. Näin samaa koodia voidaan käyttää sekä lokaalissa että tuotannon ympäristössä. Yhteysmerkkijonojen käyttäminen olisi vaarallisempaa koska ne olisi täytynyt tallentaa konfiguraatioihin missä ne vois vuotaa.


---

**3.** Blob Container luodaan `--public-access blob` -asetuksella. Mitä tämä tarkoittaa: mitä pystyy tekemään ilman tunnistautumista, ja mikä vaatii Managed Identityn?

> Vastauksesi:

> `--public-access blob` -asetus tarkoittaa että blobien URL:t ovat julkisesti luettavissa selaimessa ilman tunnistautumista. Käyttäjät voivat siis avata kuvat suoraan selaimessa. Kuitenkin tiedostojen kirjoittaminen ja poistaminen vaatii edelleen tunnistautumista Managed Identityn kautta, joten vain sovellus pystyy muokkaamaan tiedostoja. Tämä on hyvä koska käyttäjät voi nähdä kuvat, mutta eivät voi poistaa tai muuttaa niitä.


---

## Application Settings

**4.** Application Settings ylikirjoittavat `appsettings.json`:n arvot. Selitä tämä mekanismi: miten se toimii ja miksi se on hyödyllistä eri ympäristöjä varten?

> Vastauksesi:

> Application Settings on ympäristömuuttujat joita ASP.NET Core lukee automaattisesti käynnistyksen yhteydessä. Ne ylikirjoittavat `appsettings.json`:n arvot samalla nimellä. Tämä on hyödyllistä koska kehityskonella voidaan käyttää erilaisia asetuksia kuin tuotantoympäristössä, eikä tarvitse muokata koodia tai konfiguraatiotiedostoja. Esimerkiksi paikallisesti voidaan käyttää `LocalStorageService` mutta Azuressa `AzureBlobStorageService` vain muuttamalla asetuksia.


---

**5.** Application Settingsissa käytetään `Storage__Provider` (kaksi alaviivaa), mutta koodissa luetaan `configuration["Storage:Provider"]` (kaksoispiste). Miksi?

> Vastauksesi:

> Kaksi alaviivaa käytetään Application Settingsissa koska ympäristömuuttujissa ei voi käyttää kaksoispistettä. ASP.NET Core konfiguraation järjestelmä tulkitsee kaksi alaviivaa hierarkiseksi erottimeksi, samalla tavalla kun se tulkitsee kaksoispistettä `appsettings.json`:ssa. Näin `Storage__Provider` ympäristömuuttujan muuttaa automatisesti `configuration["Storage:Provider"]` arvon.


---

**6.** Mitkä konfiguraatioarvot soveltuvat Application Settingsiin, ja mitkä eivät? Anna esimerkki kummastakin tässä tehtävässä.

> Vastauksesi:

> Application Settingsiin soveltuu arvot joita ei olla salaisuuksiksi, kuten `Storage__Provider = "azure"` tai `Storage__AccountName = "stgallery..."` jotka näkyy Portalissa selväkielisenä. Mitkä eivät sovellu ovat salaisuudet esim. `ModerationService:ApiKey` tai tietokanta salasanat joita ei saa tallentaa julkisesti näkyviin paikkoihin. Nämä pitää tallentaa Key Vaultiin joka on turvallisempi.


---

## Managed Identity ja RBAC

**7.** Selitä omin sanoin: mitä tarkoittaa "System-assigned Managed Identity"? Mitä tapahtuu tälle identiteetille, jos App Service poistetaan?

> Vastauksesi:

> System-assigned Managed Identity on Azure:n antama tunnistus App Service:lle, kuin henkilötodistus joita sovellus käyttää kirjautumiseen. Se sidotaan suoraan App Serviceen ja se voidaan ottaa pois vain yksittäisillä rivillä koodissa. Jos App Service poistetaan, niin sen Managed Identity myös poistetaan automaattisesti, koska se on vain siihen App Serviceen liitetty. Näin hallintaa on helpompi, kuin jos pitäisi manuaalisesti poistaa identiteetit.


---

**8.** App Servicelle annettiin `Storage Blob Data Contributor` -rooli Storage Accountin tasolle — ei koko subscriptionin tasolle. Miksi tämä on parempi tapa? Mikä periaate tähän liittyy?

> Vastauksesi:

> Roolin antaminen Storage Accountin tasolle on parempi koska se rajoittaa App Servicen pääsyn vain yhteen Storage Accountiin, eikä koko subscriptioni. Jos joku murtoaa App Servicen, hän voi käyttää vain yhtä Storage Accountia, ei kaikkia muita resursseja. Tämä periaate on nimeltä "Least Privilege" — sovellukselle annetaan vain ne oikeudet jotka se tarvitsee työskennellä, ei enemmän. Näin turvallisuus paranee.


---


