using Application.Services;
using Domain.Interfaces;
using NETCore.Keycloak.Client.HttpClients.Implementation;
using NETCore.Keycloak.Client.Models.Auth;

namespace Infrastructure.Keycloak.Providers;

/// <inheritdoc />
public class KeycloakTokenProvider(KeycloakClient client, KeycloakSettings settings) : IKeycloakTokenProvider
{
    /// <inheritdoc />
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = await client.Auth.GetClientCredentialsTokenAsync(
            settings.Realm,
            new KcClientCredentials
            {
                ClientId = settings.ClientId,
                Secret = settings.ClientSecret
            }, cancellationToken);

        return token.Response.AccessToken;
    }
}