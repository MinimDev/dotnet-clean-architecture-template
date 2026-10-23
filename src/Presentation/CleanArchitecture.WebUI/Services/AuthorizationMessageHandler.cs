using System.Net;
using System.Net.Http.Headers;

namespace CleanArchitecture.WebUI.Services;

/// <summary>
/// DelegatingHandler opsional yang menyisipkan Bearer token dan mendelegasikan refresh token ke AuthorizedHttpClient.
/// </summary>
public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly AuthorizedHttpClient _authorizedHttpClient;

    public AuthorizationMessageHandler(AuthorizedHttpClient authorizedHttpClient)
    {
        _authorizedHttpClient = authorizedHttpClient;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return await _authorizedHttpClient.SendAsync(request, cancellationToken);
    }
}
