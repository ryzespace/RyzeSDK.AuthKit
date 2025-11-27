using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace AuthKit.Cli.Spectre;

public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver
{
    public object Resolve(Type? type)
        => provider.GetRequiredService(type!);
}