using System.Diagnostics;
using Application.Features.KeyManagement.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Security;

/// <summary>
/// Hosted service responsible for initializing the <see cref="IJwtKeyStore"/> at application startup.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Triggers keystore initialization asynchronously during host startup.</item>
/// <item>Logs performance metrics including initialization duration, active key ID, and total keys.</item>
/// <item>Implements <see cref="IHostedService"/> for integration with ASP.NET Core host lifecycle.</item>
/// </list>
/// </remarks>
public class JwtKeyStoreInitializer(IJwtKeyStore store, ILogger<JwtKeyStoreInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        await store.InitializeAsync();
        sw.Stop();

        logger.LogInformation(
            "JWT Keystore initialized in {ElapsedMilliseconds} ms | Active KID: {ActiveKid} | Total Keys: {TotalKeys}",
            sw.ElapsedMilliseconds,
            store.GetMetadata(store.GetActiveSigningCredentials().Key.KeyId)?.Kid ?? "N/A",
            store.GetPublicJwks().Count()
        );
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}