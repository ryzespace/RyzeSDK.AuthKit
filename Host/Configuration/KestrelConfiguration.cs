using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Host.Configuration;

/// <summary>
/// Configures Kestrel server with HTTPS endpoints and environment-based certificate settings.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Reads certificate path, password, and REST/gRPC ports from environment variables.</description></item>
/// <item><description>Falls back to default values for local development if environment variables are missing.</description></item>
/// <item><description>Validates certificate existence and warns if the password is missing.</description></item>
/// <item><description>Sets REST endpoint with HTTP/1 + HTTP/2 and gRPC endpoint with HTTP/2 only.</description></item>
/// </list>
/// </remarks>
public static class KestrelConfiguration
{
    /// <summary>
    /// Configures Kestrel server with HTTPS listeners for REST and gRPC using the provided certificate settings.
    /// </summary>
    /// <param name="webHost">The <see cref="IWebHostBuilder"/> to configure.</param>
    public static void ConfigureKestrelServer(this IWebHostBuilder webHost)
    {
        var (certPath, certPassword, portRest, portGrpc) = LoadSettings();
        ValidateCertificate(certPath, certPassword);
        ConfigureListeners(webHost, certPath, certPassword, portRest, portGrpc);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ Kestrel configured: REST={portRest}, gRPC={portGrpc}, Cert={certPath}");
        Console.ResetColor();
    }

    #region Helper Methods

    private static (string certPath, string certPassword, int portRest, int portGrpc) LoadSettings()
    {
        var certPath = Environment.GetEnvironmentVariable("DEV_CERT_PATH") ?? "/root/certs/devcert.pfx";
        var certPassword = Environment.GetEnvironmentVariable("DEV_CERT_PASSWORD") ?? string.Empty;
        var portRest = int.TryParse(Environment.GetEnvironmentVariable("DEV_CERT_PORT_REST"), out var p1) ? p1 : 5000;
        var portGrpc = int.TryParse(Environment.GetEnvironmentVariable("DEV_CERT_PORT_GRPC"), out var p2) ? p2 : 5001;

        return (certPath, certPassword, portRest, portGrpc);
    }

    private static void ValidateCertificate(string certPath, string certPassword)
    {
        if (!File.Exists(certPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Certificate not found at: {certPath}");
            Console.ResetColor();
        }

        if (string.IsNullOrWhiteSpace(certPassword))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️  DEV_CERT_PASSWORD not set — using empty password for certificate.");
            Console.ResetColor();
        }
    }

    private static void ConfigureListeners(
        IWebHostBuilder webHost, 
        string certPath, 
        string certPassword, 
        int portRest, 
        int portGrpc)
    {
        webHost.ConfigureKestrel(options =>
        {
            // REST (HTTP/1 + HTTP/2)
            options.ListenAnyIP(portRest, listenOptions =>
            {
                listenOptions.UseHttps(certPath, certPassword);
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
            });

            // gRPC (HTTP/2 only)
            options.ListenAnyIP(portGrpc, listenOptions =>
            {
                listenOptions.UseHttps(certPath, certPassword);
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        });
    }

    #endregion
}