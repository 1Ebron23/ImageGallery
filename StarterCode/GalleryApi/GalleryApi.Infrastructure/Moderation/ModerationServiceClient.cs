using GalleryApi.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace GalleryApi.Infrastructure.Moderation;

/// <summary>
/// Simuloitu sisällöntarkistuspalvelu. Oikeassa sovelluksessa tämä
/// kutsuisi ulkoista AI-pohjaista moderointipalvelua.
/// </summary>
public class ModerationServiceClient
{
    private readonly ModerationServiceOptions _options;

    // TODO (Vaihe 4): Muuta konstruktori ottamaan IOptions<ModerationServiceOptions> parametrina
    //   sijaan string apiKey.
    //
    // Nykyinen (ONGELMA):
    //   public ModerationServiceClient(string apiKey) { _apiKey = apiKey; }
    //
    // Muutettu (Options Pattern):
    //   public ModerationServiceClient(IOptions<ModerationServiceOptions> options)
    //   {
    //       _apiKey = options.Value.ApiKey;
    //   }
    //
    // Muista muuttaa myös _apiKey-kentän tyyppi (tai poista se ja käytä _options.ApiKey suoraan).
    public ModerationServiceClient(IOptions<ModerationServiceOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Tarkistaa onko kuvan sisältö turvallinen.
    /// Simuloitu toteutus — palauttaa aina true.
    /// </summary>
    public Task<bool> IsContentSafeAsync(Stream imageStream, string contentType)
    {
        // Simuloitu tarkistus: oikeassa toteutuksessa lähettäisi kuvan
        // moderointipalvelun API:lle _apiKey:tä käyttäen
        return Task.FromResult(true);
    }
}
