using Spectre.Console.Cli;

namespace AuthKit.Cli.Interfaces;

public interface ICliCommand
{
    static abstract string Name { get; }
    static abstract void Configure(IConfigurator configurator);
}