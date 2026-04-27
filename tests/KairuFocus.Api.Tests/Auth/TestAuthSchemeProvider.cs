using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace KairuFocus.Api.Tests.Auth;

/// <summary>
/// Replaces IAuthenticationSchemeProvider to redirect "Cookies" scheme
/// authentication to TestCookieAuthHandler, enabling integration tests
/// that simulate a successful cookie authentication result.
/// </summary>
public sealed class TestAuthSchemeProvider : IAuthenticationSchemeProvider
{
    private readonly IAuthenticationSchemeProvider _inner;
    private static readonly AuthenticationScheme TestCookieScheme = new(
        CookieAuthenticationDefaults.AuthenticationScheme,
        CookieAuthenticationDefaults.AuthenticationScheme,
        typeof(TestCookieAuthHandler));

    public TestAuthSchemeProvider(IAuthenticationSchemeProvider inner)
    {
        _inner = inner;
    }

    public void AddScheme(AuthenticationScheme scheme) => _inner.AddScheme(scheme);
    public void RemoveScheme(string name) => _inner.RemoveScheme(name);
    public Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync() => _inner.GetAllSchemesAsync();
    public Task<AuthenticationScheme?> GetDefaultAuthenticateSchemeAsync() => _inner.GetDefaultAuthenticateSchemeAsync();
    public Task<AuthenticationScheme?> GetDefaultChallengeSchemeAsync() => _inner.GetDefaultChallengeSchemeAsync();
    public Task<AuthenticationScheme?> GetDefaultForbidSchemeAsync() => _inner.GetDefaultForbidSchemeAsync();
    public Task<AuthenticationScheme?> GetDefaultSignInSchemeAsync() => _inner.GetDefaultSignInSchemeAsync();
    public Task<AuthenticationScheme?> GetDefaultSignOutSchemeAsync() => _inner.GetDefaultSignOutSchemeAsync();
    public Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync() => _inner.GetRequestHandlerSchemesAsync();

    public Task<AuthenticationScheme?> GetSchemeAsync(string name)
    {
        if (name == CookieAuthenticationDefaults.AuthenticationScheme)
            return Task.FromResult<AuthenticationScheme?>(TestCookieScheme);

        return _inner.GetSchemeAsync(name);
    }
}
