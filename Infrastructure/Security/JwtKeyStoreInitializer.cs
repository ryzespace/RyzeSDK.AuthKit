using System.Diagnostics;
using Application.Features.KeyManagement.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Security;

public class JwtKeyStoreInitializer(IServiceProvider provider, ILogger<JwtKeyStoreInitializer> logger)
    : IHostedService, IAsyncDisposable
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = provider.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IJwtKeyStore>();

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

    public async ValueTask DisposeAsync()
    {
        await using var scope = provider.CreateAsyncScope();
        if (scope.ServiceProvider.GetService<IJwtKeyStore>() is IAsyncDisposable asyncStore)
        {
            await asyncStore.DisposeAsync();
        }
    }
}