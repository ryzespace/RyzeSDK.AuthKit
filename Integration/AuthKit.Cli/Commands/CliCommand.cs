using Spectre.Console.Cli;

namespace AuthKit.Cli.Commands;

public abstract class CliCommand<TSettings> 
    : AsyncCommand<TSettings>
    where TSettings : CommandSettings
{
 
}