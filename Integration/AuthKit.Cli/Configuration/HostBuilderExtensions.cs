using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        services.AddSingleton<ConsoleApp>(sp =>
        {
            var serviceCollection = new ServiceCollection();
            foreach (var descriptor in sp.GetServices<ServiceDescriptor>())
            {
                serviceCollection.Add(descriptor);
            }

            return new ConsoleApp(
                serviceCollection
            );
        });

        return services;
    }
}