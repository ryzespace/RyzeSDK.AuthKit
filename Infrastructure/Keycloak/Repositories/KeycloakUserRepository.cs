using Application.Services;
using Domain.Exceptions;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using NETCore.Keycloak.Client.HttpClients.Implementation;
using NETCore.Keycloak.Client.Models.Users;

namespace Infrastructure.Keycloak.Repositories;

/// <inheritdoc />
public class KeycloakUserRepository(KeycloakClient client,
    KeycloakSettings settings,
    ILogger<KeycloakUserRepository> logger) : IKeycloakUserRepository
{
    /// <inheritdoc />
    public async Task CreateUserAsync(KcUser kcUser, string accessToken, CancellationToken cancellationToken)
    {
        var response = await client.Users.CreateAsync(settings.Realm, accessToken, kcUser, cancellationToken);

        if (response.IsError)
        {
            logger.LogError("Failed to create user in Keycloak: {Error}", response.ErrorMessage);
            throw new KeycloakOperationException($"Failed to create user: {response.ErrorMessage}");
        }
    }
    
    /// <inheritdoc />
    public async Task UpdateUserAsync(string userId, KcUser user, string accessToken, CancellationToken cancellationToken = default)
    {
        var response = await client.Users.UpdateAsync(
            settings.Realm,
            accessToken,
            userId,
            user,
            cancellationToken);

        if (response.IsError)
        {
            logger.LogError("Failed to update user in Keycloak: {Error}", response.ErrorMessage);
            throw new KeycloakOperationException($"Failed to update user: {response.ErrorMessage}");
        }
    }

    /// <inheritdoc />
    public async Task DeleteUserAsync(string userId, string accessToken, CancellationToken cancellationToken = default)
    {
        var response = await client.Users.DeleteAsync(
            settings.Realm,
            accessToken,
            userId,
            cancellationToken);

        if (response.IsError)
        {
            logger.LogError("Failed to delete user from Keycloak: {Error}", response.ErrorMessage);
            throw new KeycloakOperationException($"Failed to delete user: {response.ErrorMessage}");
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> UserExistsByEmailAsync(string email, string accessToken, CancellationToken cancellationToken)
    {
        var existsResponse = await client.Users.IsUserExistsByEmailAsync(
            settings.Realm, accessToken, email, cancellationToken);

        return existsResponse.Response;
    }

    /// <inheritdoc />
    public async Task<KcUser?> GetUserByEmailAsync(string email, string accessToken, CancellationToken cancellationToken)
    {
        if (!await UserExistsByEmailAsync(email, accessToken, cancellationToken))
            return null;

        var filter = new KcUserFilter
        {
            Email = email
        };
        var usersResponse = await client.Users.ListUserAsync(
            settings.Realm, accessToken, filter, cancellationToken);

        return usersResponse.Response?.FirstOrDefault();
    }

    /// <inheritdoc />
    public async Task<KcUser?> GetUserByUsernameAsync(string username, string accessToken, CancellationToken cancellationToken)
    {
        var filter = new KcUserFilter
        {
            Username = username
        };
        var usersResponse = await client.Users.ListUserAsync(
            settings.Realm, accessToken, filter, cancellationToken);

        return usersResponse.Response?.FirstOrDefault();
    }
}