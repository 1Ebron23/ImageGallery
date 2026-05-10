# Kysymykset — Osa 3: Key Vault ja Infrastructure as Code

Vastaa kysymyksiin omin sanoin. Lyhyet, selkeät vastaukset riittävät.

---

## Key Vault

**1.** Miksi `ModerationService:ApiKey` tallennettiin Key Vaultiin eikä Application Settingsiin? Mitä lisäarvoa Key Vault tuo Application Settingsiin verrattuna?

> Vastauksesi:

> ModerationService:ApiKey on oikea salaisuus, joten se tallennettiin Key Vaultiin turvaavasti. Application Settingsissä se olisi näkynyt selväkielisenä portaalissa kaikille joille on pääsy Azuren portaaliin. Key Vault tuovat seuraavat lisäarvo verrattuna Application Settingsiin:
> - Arvo on salattu lepotilassa ja ei näy portaalissa selväkielisenä
> - Jokaisella päivityksellä luodaan uusi versio, vanhat säilyvät palautusta varten
> - Käyttö on auditoitu ja lokiin kirjataan kuka luki salaisuuden ja milloin
> - Pääsy hallitaan RBAC-rooleilla, jotka antaa rajatun lukuoikeuden vain tarvittaville identiteeteille
> - Salaisuuden arvo voidaan vaihtaa Key Vaultissa ilman koodimuutoksia


---

**2.** Key Vault -salaisuuden nimi on `ModerationService--ApiKey` (kaksi väliviivaa), mutta koodissa se luetaan `configuration["ModerationService:ApiKey"]` (kaksoispiste). Miksi käytetään `--`?

> Vastauksesi:

> Key Vault ei hyväksy `:` tai `__` merkkejä salaisuuksien nimissä. Siksi käytetään kahta väliviivaa (`--`). ASP.NET Core:n Key Vault -provider muuntaa automaattisesti `--` merkit kaksoispisteksi (`:`), jotka vastaavat konfiguraation hierarkiaa. Näin `ModerationService--ApiKey` Key Vaultissa muutetaan `ModerationService:ApiKey`-muodoksi sovelluksessa.




---

**3.** `Program.cs`:ssä Key Vault lisätään konfiguraatiolähteeksi `if (!string.IsNullOrEmpty(keyVaultUrl))`-ehdolla. Miksi tämä ehto on tärkeä? Mitä tapahtuisi ilman sitä?

> Vastauksesi:

> Ehto on tärkeä koska paikallisesti kehitettäessä `KeyVault:VaultUrl` on tyhjä — sitä ei ole asetettu User Secretsiin. Ilman ehtoa sovellus yrityisi yhdistää Key Vaultiin (joka ei olisi paikallisesti käytettävissä) ja loisi virheen. Ehdolla sovellus ohittaa Key Vault -integraation paikallisesti ja käyttää vain User Secretsejä ja appsettings.jsonia. Azuressa arvo on asetettu Application Settingsiin, joten integraatio aktivoidaan ja toimii normaalisti.


---

**4.** Kun sovellus on käynnissä Azuressa, konfiguraation prioriteettijärjestys on: Key Vault → Application Settings → `appsettings.json`. Selitä millä arvolla `ModerationService:ApiKey` lopulta ladataan — ja käy läpi jokainen askel siitä, miten arvo päätyy sovelluksen `IOptions<ModerationServiceOptions>`:iin.

> Vastauksesi:

> Arvo ladataan Key Vaultista koska se on korkein prioriteetti. Seuraavat askeleet:
> 1. Program.cs käynnistyy ja lukee `KeyVault:VaultUrl` Application Settingsista
> 2. `AddAzureKeyVault` kutsutaan, joka yhdistää Managed Identity:n avulla Key Vaultiin
> 3. Key Vault hakee kaikki salaisuudet ja muuntaa `ModerationService--ApiKey` → `ModerationService:ApiKey`-muotoon
> 4. Konfiguraatioputkeen lisätään Key Vaultin arvot, jotka ohittavat Application Settingsin ja appsettings.jsonin arvot
> 5. `Configure<ModerationServiceOptions>(config.GetSection("ModerationService"))` lukee konfiguraatiosta `ModerationService:ApiKey`-avaimen
> 6. IOptions<ModerationServiceOptions>:iin täytetään ApiKey-ominaisuus Key Vaultista haetulla arvolla
> 7. ModerationServiceClient saa IOptions:in injektiona ja käyttää ApiKey-arvoa


---

**5.** Mitä eroa on `Key Vault Secrets User` ja `Key Vault Secrets Officer` -roolien välillä? Miksi annettiin nimenomaan `Secrets User`?

> Vastauksesi:

> `Key Vault Secrets User` antaa vain lukuoikeuden salaisuuksiin. `Key Vault Secrets Officer` antaa täydet oikeudet: lukeminen, kirjoittaminen, poistaminen ja hallintaa. App Servicelle annettiin `Secrets User` koska se tarvitsee vain lukuoikeutta salaisuuksiin — se ei saa luoda, muokata tai poistaa salaisuuksia Key Vaultista. Tämä on pienimmän oikeuden periaate: annetaan vain tarvittavat oikeudet turvallisuuden parantamiseksi.


---

## Infrastructure as Code (Bicep)

**6.** Bicep-templatessa RBAC-roolimääritykset tehdään suoraan (`storageBlobRole`, `keyVaultSecretsRole`). Mitä etua tällä on verrattuna siihen, että ajat erilliset `az role assignment create` -komennot käsin?

> Vastauksesi:

> Kun RBAC-roolimääritykset on suoraan Bicep-templatessa:
> - Kaikki resurssit ja niiden oikeudet luodaan yhdellä deploymentiolla — ei tarvitse muistaa useita erillisiä komentoja
> - Konfiguraatio on dokumentoitu tiedostossa ja versiohallinnassa Gitissä
> - Deployment on toistuva ja johdonmukainen — jokainen uusi deployment luo täsmälleen saman tuloksen
> - Jos roolimäärityksessä on virhe, se tulee esille heti validatessa eikä vasta manuaalisen komennon aikana
> - Ympäristöjen välinen synkronointi on helpompaa, koska kaikki on samassa tiedostossa



---

**7.** Bicep-parametritiedostossa `main.bicepparam` on `param moderationApiKey = ''` — arvo jätetään tyhjäksi. Miksi? Miten oikea arvo annetaan?

> Vastauksesi:

> Arvo jätetään tyhjäksi koska `.bicepparam`-tiedosto menee Gitiin ja versiohistoriaan. Jos siihen tallentaisiin oikea API-avain, se näkyisi kaikille joilla on pääsy repositorioon — turvallisuusriski. Sen sijaan oikea arvo annetaan erikseen deployment-komennossa `--parameters moderationApiKey="sk-oikea-avain"` lipulla, joka ei tallennu tiedostoihin.


---

**8.** Bicep-templatessa `webApp`-resurssin `identity`-lohkossa on `type: 'SystemAssigned'`. Mitä tämä tekee, ja mitä manuaalista komentoa se korvaa?

> Vastauksesi:

> `type: 'SystemAssigned'` aktivoi Managed Identity:n App Servicelle automaattisesti. Se luo järjestelmän hallitseman identiteetin, jonka avulla sovellus voi todentaa itsensä Azuren palveluihin (Blob Storage, Key Vault) käyttäen DefaultAzureCredential:ia. Manuaalisesti tämä tehtäisiin komennolla `az webapp identity assign --name $APP_NAME --resource-group $RESOURCE_GROUP`, mutta Bicepissä se on suoraan resurssin määrittelyssä.


---

**9.** RBAC-roolimäärityksen nimi generoidaan `guid()`-funktiolla:

```bicep
name: guid(storageAccount.id, webApp.identity.principalId, 'StorageBlobDataContributor')
```

Miksi nimi generoidaan näin eikä esimerkiksi kovakoodatulla merkkijonolla? Mitä tapahtuisi jos nimi olisi sama kaikissa deploymenteissa?

> Vastauksesi:

guid()-funktio generoi uniikkin tunnisteen perustuen annettuihin parametreihin. Jos kahdella deploymentilla on eri resurssit tai principalit, ne saavat eri tunnisteita. Jos nimi olisi kovakoodattu ja sama kaikissa deploymenteissa, Azure voisi sekoittaa roolimääritykset tai nousta virhe jos samaa nimeä käytettäisiin eri deploymenteissa. guid()-funktiolla varmistetaan, että jokainen yhdistelmä saa oman uniikkin tunnisteen, ja Bicep pystyy hallitsemaan roolimäärityksiä oikein uusintaa deployatessa.



---

**10.** Olet nyt rakentanut saman infrastruktuurin kahdella tavalla: manuaalisesti (Osat 2 & 3) ja Bicepillä (Osa 3). Kuvaile konkreettisesti yksi tilanne, jossa IaC-lähestymistapa on selvästi manuaalista parempi. Kuvaile myös tilanne, jossa manuaalinen tapa riittää.

> Vastauksesi:

 IaC on parempi: Tilanteessa, jossa sinun täytyy luoda sama infrastruktuuri useille ympäristöille (dev, test, prod). Manuaalisesti sinun täytyisi muistaa ajaa 10+ komentoa jokaiselle ympäristölle, ja virheille on paljon tilaa. Bicepillä luot yhden templaten ja vain vaihdat parametritiedostojen arvoja (.bicepparam), sitten yksi komento ottaa koko infran käyttöön. Kaikissa ympäristöissä on täsmälleen sama konfiguraatio.

Manuaalinen tapa riittää: Kehitysympäristössä, kun kokeile yksittäisen Azure-resurssin asetuksia paikallisesti. Esimerkiksi jos testaat, miten tietty Storage Account -konfiguraatio toimii, voit ajaa muutaman manuaalisen komennon, testailla nopeasti, ja poistaa. Ei tarvitse luoda koko Bicep-templatea, kun testailu on ohi. Manuaalinen tapa on nopeampi pienille, kertaluonteisille tehtäville.