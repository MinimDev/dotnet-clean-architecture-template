namespace CleanArchitecture.WebUI.Services;

/// <summary>
/// DelegatingHandler yang otomatis menyisipkan Bearer token ke setiap HTTP request.
/// Ini memastikan token selalu terpasang, tanpa bergantung pada timing lifecycle komponen.
/// </summary>
public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly TokenProvider _tokenProvider;

    public AuthorizationMessageHandler(TokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_tokenProvider.Token))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenProvider.Token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
