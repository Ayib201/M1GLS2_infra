namespace M1GLS2_infra.Extensions;

/// <summary>
/// Réécrit silencieusement les appels HTTP sortants qui visent l'adresse
/// PUBLIQUE de Keycloak (celle que contiennent les jetons, ex:
/// "localhost:8000" via Kong) vers son adresse INTERNE au réseau Docker
/// (ex: "keycloak:8080").
///
/// Pourquoi c'est nécessaire : le document de découverte OIDC renvoyé par
/// Keycloak contient des URLs (notamment "jwks_uri", l'adresse où récupérer
/// les clés utilisées pour vérifier la signature des jetons) construites à
/// partir de son nom PUBLIC -- celui que voit le navigateur. Depuis
/// l'intérieur d'un conteneur Docker, ce nom public n'est pas forcément
/// joignable ("localhost", par exemple, y désigne le conteneur lui-même,
/// jamais la machine hôte ni un autre conteneur). Ce handler intercepte donc
/// ces appels sortants pour les rediriger vers l'adresse qui, elle,
/// fonctionne réellement depuis l'intérieur du réseau Docker -- sans que le
/// reste du code (ni Keycloak) n'ait besoin de le savoir.
/// </summary>
public sealed class KeycloakInternalRoutingHandler : DelegatingHandler
{
    private readonly Uri _publicAuthority;
    private readonly Uri _internalAuthority;

    public KeycloakInternalRoutingHandler(string publicIssuer, string internalAuthority)
    {
        _publicAuthority = new Uri(publicIssuer);
        _internalAuthority = new Uri(internalAuthority);
        InnerHandler = new HttpClientHandler();
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri is not null &&
            string.Equals(request.RequestUri.Authority, _publicAuthority.Authority, StringComparison.OrdinalIgnoreCase))
        {
            var rewritten = new UriBuilder(request.RequestUri)
            {
                Scheme = _internalAuthority.Scheme,
                Host = _internalAuthority.Host,
                Port = _internalAuthority.Port,
            };
            request.RequestUri = rewritten.Uri;
        }

        return base.SendAsync(request, cancellationToken);
    }
}
