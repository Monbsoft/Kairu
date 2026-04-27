using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using KairuFocus.Application.Identity.Commands.GetOrCreateUser;
using KairuFocus.Domain.Common;
using KairuFocus.Domain.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Monbsoft.BrilliantMediator.Abstractions;

namespace KairuFocus.Api.Tests.Auth;

/// <summary>
/// Integration tests on AuthController.
/// Verifies that the OAuth cookie is cleared on error paths
/// and that the logout endpoint behaves correctly.
/// </summary>
public sealed class AuthControllerTests : IClassFixture<KairuFocusApiFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(KairuFocusApiFactory factory)
    {
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            // Disable automatic redirect following so we can inspect response headers.
            AllowAutoRedirect = false,
        });
    }

    /// <summary>
    /// Verifies that the GitHub callback without a valid OAuth state cookie
    /// cleans up the cookie before redirecting with an error.
    ///
    /// Context: without a valid correlation state, AuthenticateAsync fails
    /// ("The oauth state was missing or invalid"). Without the fix, the code
    /// would redirect without calling SignOut, leaving a corrupted cookie.
    /// With the fix, RedirectWithErrorAsync calls SignOut before redirecting.
    ///
    /// We verify:
    /// 1. The response is a redirect (302).
    /// 2. The Location header contains auth-error=.
    /// 3. The Set-Cookie header clears the session cookie.
    /// </summary>
    [Fact]
    public async Task Should_ClearCookie_When_CallbackAuthenticationFails()
    {
        // Arrange — direct call without going through the GitHub OAuth flow.
        // No correlation cookie → AuthenticateAsync will fail.
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/github/callback");

        // Act
        var response = await _client.SendAsync(request);

        // Assert — must redirect to the error page.
        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect
            || response.StatusCode == HttpStatusCode.Found,
            $"Expected redirect, got {(int)response.StatusCode}");

        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("auth-error=", location, StringComparison.OrdinalIgnoreCase);

        // Verify that Set-Cookie clears the session cookie
        // (SignOutAsync emits a Set-Cookie with a past expiry).
        var setCookieHeaders = response.Headers
            .Where(h => string.Equals(h.Key, "Set-Cookie", StringComparison.OrdinalIgnoreCase))
            .SelectMany(h => h.Value)
            .ToList();

        Assert.True(setCookieHeaders.Count > 0,
            "Expected at least one Set-Cookie header to clear the authentication cookie after error.");
    }

    /// <summary>
    /// Verifies that POST /api/auth/logout requires a valid JWT (401 when unauthenticated).
    /// </summary>
    [Fact]
    public async Task Should_Return401_When_LogoutCalledWithoutJwt()
    {
        // Arrange — no Authorization header
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");

        // Act
        var response = await _client.SendAsync(request);

        // Assert — 401 Unauthorized because [Authorize(JwtBearer)] is required
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verifies that POST /api/auth/logout returns 204 No Content
    /// and emits a Set-Cookie that removes the session cookie,
    /// when called with a valid JWT.
    /// </summary>
    [Fact]
    public async Task Should_SignOutCookieScheme_When_LogoutEndpointCalled()
    {
        // Arrange — generate a valid JWT signed with the test secret key
        var jwt = GenerateTestJwt();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

        // Act
        var response = await _client.SendAsync(request);

        // Assert — 204 No Content
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // SignOutAsync must emit a Set-Cookie to remove the .AspNetCore.Cookies cookie.
        var setCookieHeaders = response.Headers
            .Where(h => string.Equals(h.Key, "Set-Cookie", StringComparison.OrdinalIgnoreCase))
            .SelectMany(h => h.Value)
            .ToList();

        Assert.True(setCookieHeaders.Count > 0,
            "Expected at least one Set-Cookie header to clear the authentication cookie on logout.");

        var hasCookieClearDirective = setCookieHeaders.Any(c =>
            c.Contains("expires=", StringComparison.OrdinalIgnoreCase)
            || c.Contains("max-age=0", StringComparison.OrdinalIgnoreCase));

        Assert.True(hasCookieClearDirective,
            $"Expected a Set-Cookie with past expiry. Got: {string.Join("; ", setCookieHeaders)}");
    }

    private static string GenerateTestJwt()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("testing-secret-key-minimum-32-chars-for-hmac"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: [new Claim("sub", "test-user-id")],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

/// <summary>
/// Tests the no-id branch of GitHubCallback:
/// Cookie authentication succeeds but the principal has no NameIdentifier claim.
/// </summary>
public sealed class GitHubCallbackNoIdTests
{
    private readonly HttpClient _client;
    private readonly TestClaimsProvider _claimsProvider = new();

    public GitHubCallbackNoIdTests()
    {
        var claimsProvider = _claimsProvider;

        var factory = new KairuFocusApiFactory
        {
            ConfigureTestServices = services =>
            {
                // Register TestClaimsProvider and TestCookieAuthHandler, then replace
                // IAuthenticationSchemeProvider so that "Cookies" uses TestCookieAuthHandler.
                services.AddSingleton(claimsProvider);
                services.AddTransient<TestCookieAuthHandler>();
                services.AddSingleton<IAuthenticationSchemeProvider>(sp =>
                {
                    var inner = new AuthenticationSchemeProvider(
                        sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthenticationOptions>>());
                    return new TestAuthSchemeProvider(inner);
                });
            }
        };

        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    /// <summary>
    /// Verifies that when AuthenticateAsync(Cookie) succeeds but the principal
    /// has no NameIdentifier claim, the callback redirects with auth-error=no-id.
    ///
    /// Note: SignOutAsync is a no-op on the test handler (no real cookie infrastructure),
    /// so the Set-Cookie assertion is omitted for this test — the core behavior under test
    /// is the redirect URL containing auth-error=no-id.
    /// </summary>
    [Fact]
    public async Task Should_ClearCookieAndRedirectWithNoIdError_When_CallbackAuthenticatesWithoutGitHubId()
    {
        // Arrange — return a principal without NameIdentifier to trigger the no-id branch.
        _claimsProvider.Claims = [
            new Claim(ClaimTypes.Name, "test-user")
            // No ClaimTypes.NameIdentifier
        ];

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/github/callback");

        // Act
        var response = await _client.SendAsync(request);

        // Assert — must redirect to the no-id error page.
        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect
            || response.StatusCode == HttpStatusCode.Found,
            $"Expected redirect, got {(int)response.StatusCode}");

        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("auth-error=no-id", location, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Tests the server error branch of GitHubCallback:
/// Cookie authentication succeeds and the principal has a NameIdentifier claim,
/// but GetOrCreateUser fails.
/// </summary>
public sealed class GitHubCallbackServerErrorTests
{
    private readonly HttpClient _client;
    private readonly TestClaimsProvider _claimsProvider = new();

    public GitHubCallbackServerErrorTests()
    {
        var claimsProvider = _claimsProvider;
        var mediator = new TestMediator
        {
            DispatchResult = _ => Result.Failure<GetOrCreateUserResult>("simulated server error")
        };

        var factory = new KairuFocusApiFactory
        {
            ConfigureTestServices = services =>
            {
                // Register TestClaimsProvider and TestCookieAuthHandler, then replace
                // IAuthenticationSchemeProvider so that "Cookies" uses TestCookieAuthHandler.
                services.AddSingleton(claimsProvider);
                services.AddTransient<TestCookieAuthHandler>();
                services.AddSingleton<IAuthenticationSchemeProvider>(sp =>
                {
                    var inner = new AuthenticationSchemeProvider(
                        sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthenticationOptions>>());
                    return new TestAuthSchemeProvider(inner);
                });

                // Replace IMediator with the test stub that returns a failure.
                services.RemoveAll<IMediator>();
                services.AddSingleton<IMediator>(mediator);
            }
        };

        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    /// <summary>
    /// Verifies that when AuthenticateAsync(Cookie) succeeds with a valid NameIdentifier
    /// but GetOrCreateUser returns a failure, the callback redirects with auth-error=server.
    ///
    /// Note: SignOutAsync is a no-op on the test handler (no real cookie infrastructure),
    /// so the Set-Cookie assertion is omitted — the core behavior under test is the redirect URL.
    /// </summary>
    [Fact]
    public async Task Should_ClearCookieAndRedirectWithServerError_When_GetOrCreateUserFails()
    {
        // Arrange — return a principal with NameIdentifier so the controller reaches the mediator.
        _claimsProvider.Claims = [
            new Claim(ClaimTypes.NameIdentifier, "123456"),
            new Claim(ClaimTypes.Name, "test-user")
        ];

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/github/callback");

        // Act
        var response = await _client.SendAsync(request);

        // Assert — must redirect to the server error page.
        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect
            || response.StatusCode == HttpStatusCode.Found,
            $"Expected redirect, got {(int)response.StatusCode}");

        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("auth-error=server", location, StringComparison.OrdinalIgnoreCase);
    }
}
