using AuthKit.CLI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthKit.Cli.Configuration;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseAuthKitConsole(this IHostBuilder builder)
    {
        return builder.ConfigureServices((context, services) =>
        {
            services.ConfigureApp(context.Configuration);
        });
    }

    private static IServiceCollection ConfigureApp(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConsoleApp, ConsoleApp>();
        return services;
    }
}