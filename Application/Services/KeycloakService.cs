using Application.DTO;
using Application.Interfaces;
using Domain.Interfaces;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using NETCore.Keycloak.Client.Models.Common;
using NETCore.Keycloak.Client.Models.Users;

namespace Application.Services;

/// <inheritdoc />
public class KeycloakService(
    ILogger<KeycloakService> logger,
    IKeycloakTokenProvider tokenProvider,
    IKeycloakUserRepository userRepository)
    : IKeycloakService
{
    /// <inheritdoc />
    public async Task CreateUserAsync(KeycloakUserDto dtoUser, CancellationToken cancellationToken)
    {
        var accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken);

        if (await userRepository.UserExistsByEmailAsync(dtoUser.Email, accessToken, cancellationToken))
        {
            logger.LogWarning("User creation failed. Email {Email} already exists", dtoUser.Email);
            throw new InvalidOperationException($"Cannot create user. Email '{dtoUser.Email}' is already in use.");
        }

        var kcUser = MapToKeycloakUser(dtoUser);
        await userRepository.CreateUserAsync(kcUser, accessToken, cancellationToken);

        logger.LogInformation("User {Username} created successfully", dtoUser.Username);
        await LogCreatedUserIdAsync(dtoUser.Username, accessToken, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteUserAsync(string email, CancellationToken cancellationToken)
    {
        var accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken);
        var user = await userRepository.GetUserByEmailAsync(email, accessToken, cancellationToken);

        if (user == null)
        {
            logger.LogWarning("Cannot delete user. Email {Email} not found", email);
            return;
        }

        await userRepository.DeleteUserAsync(user.Id, accessToken, cancellationToken);

        logger.LogInformation("User {Username} (ID: {UserId}) deleted successfully", user.UserName, user.Id);
    }
    
    /// <inheritdoc />
    public async Task UpdateUserAsync(KeycloakUserDto dtoUser, CancellationToken cancellationToken)
    {
        var accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken);
        var user = await userRepository.GetUserByEmailAsync(dtoUser.Email, accessToken, cancellationToken);

        if (user == null)
        {
            logger.LogWarning("Cannot update user. Email {Email} not found", dtoUser.Email);
            throw new InvalidOperationException($"Cannot update user. Email '{dtoUser.Email}' not found.");
        }

        var kcUser = MapToKeycloakUser(dtoUser);

        await userRepository.UpdateUserAsync(user.Id, kcUser, accessToken, cancellationToken);

        logger.LogInformation("User {Username} (ID: {UserId}) updated successfully", kcUser.UserName, user.Id);
    }

    private static KcUser MapToKeycloakUser(KeycloakUserDto dtoUser)
    {
        return new KcUser
        {
            FirstName = dtoUser.FirstName,
            LastName = dtoUser.LastName,
            UserName = dtoUser.Username,
            Email = dtoUser.Email,
            Enabled = true,
            Credentials = new List<KcCredentials>
            {
                new()
                {
                    Type = "password",
                    Value = dtoUser.Password,
                    Temporary = false
                }
            }
        };
    }

    private async Task LogCreatedUserIdAsync(string username, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetUserByUsernameAsync(username, accessToken, cancellationToken);
            if (user != null)
            {
                logger.LogDebug("User found: {Username}, ID: {UserId}", user.UserName, user.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to retrieve created user ID for {Username}", username);
        }
    }
}