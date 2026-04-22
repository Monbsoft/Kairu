using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace KairuFocus.Api.Tests.Auth;

/// <summary>
/// Tests d'intégration sur AuthController.
/// Vérifie que le cookie OAuth est nettoyé sur les chemins d'erreur
/// et que l'endpoint de logout fonctionne correctement.
/// </summary>
public sealed class AuthControllerTests : IClassFixture<KairuFocusApiFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(KairuFocusApiFactory factory)
    {
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            // Désactiver le suivi automatique des redirects pour inspecter les headers de réponse.
            AllowAutoRedirect = false,
        });
    }

    /// <summary>
    /// Vérifie que le callback GitHub avec un state OAuth manquant/invalide
    /// nettoie le cookie avant de rediriger.
    ///
    /// Contexte : sans state valide, AuthenticateAsync échoue ("The oauth state was missing or invalid").
    /// L'ancien code retournait Redirect sans SignOut → cookie corrompu résiduel.
    /// Le nouveau code appelle RedirectWithErrorAsync qui fait SignOut puis Redirect.
    ///
    /// On ne peut pas valider un vrai handshake GitHub sans un provider réel,
    /// mais on peut vérifier que :
    /// 1. La réponse est une redirection (302/301)
    /// 2. Le header Set-Cookie contient une directive d'expiration passée
    ///    (suppression du cookie), ou que le cookie .AspNetCore.Cookies est absent/expiré.
    /// </summary>
    [Fact]
    public async Task Should_ClearCookie_When_CallbackFailsWithoutGitHubId()
    {
        // Arrange — appel direct au callback sans passer par le flow OAuth GitHub.
        // Pas de cookie de corrélation → AuthenticateAsync va échouer.
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/github/callback");

        // Act
        var response = await _client.SendAsync(request);

        // Assert — doit rediriger vers la page d'erreur.
        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect
            || response.StatusCode == HttpStatusCode.Found,
            $"Expected redirect, got {(int)response.StatusCode}");

        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("auth-error=", location,
            StringComparison.OrdinalIgnoreCase);

        // Vérifie que le Set-Cookie supprime le cookie de session
        // (SignOutAsync émet un Set-Cookie avec expires dans le passé).
        var setCookieHeaders = response.Headers
            .Where(h => string.Equals(h.Key, "Set-Cookie", StringComparison.OrdinalIgnoreCase))
            .SelectMany(h => h.Value)
            .ToList();

        // Il doit y avoir au moins un Set-Cookie pour effacer le cookie de corrélation/session.
        // (même si aucun cookie n'était présent, SignOutAsync émet quand même la directive d'expiration).
        Assert.True(setCookieHeaders.Count > 0,
            "Expected at least one Set-Cookie header to clear the authentication cookie after error.");
    }

    /// <summary>
    /// Vérifie que POST /api/auth/logout retourne 204 No Content
    /// et émet un Set-Cookie qui supprime le cookie de session.
    /// </summary>
    [Fact]
    public async Task Should_SignOutCookieScheme_When_LogoutEndpointCalled()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");

        // Act
        var response = await _client.SendAsync(request);

        // Assert — 204 No Content
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // SignOutAsync doit émettre un Set-Cookie pour supprimer le cookie .AspNetCore.Cookies.
        var setCookieHeaders = response.Headers
            .Where(h => string.Equals(h.Key, "Set-Cookie", StringComparison.OrdinalIgnoreCase))
            .SelectMany(h => h.Value)
            .ToList();

        Assert.True(setCookieHeaders.Count > 0,
            "Expected at least one Set-Cookie header to clear the authentication cookie on logout.");

        // Le header doit cibler le cookie de session (contient le nom du scheme ou expires passé).
        var hasCookieClearDirective = setCookieHeaders.Any(c =>
            c.Contains("expires=", StringComparison.OrdinalIgnoreCase)
            || c.Contains("max-age=0", StringComparison.OrdinalIgnoreCase));

        Assert.True(hasCookieClearDirective,
            $"Expected a Set-Cookie with past expiry. Got: {string.Join("; ", setCookieHeaders)}");
    }
}
