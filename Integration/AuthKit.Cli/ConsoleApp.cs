using AuthKit.Cli.Commands;
using AuthKit.Cli.Interfaces;
using AuthKit.Cli.Spectre;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace AuthKit.Cli;

public sealed class ConsoleApp(IServiceCollection services)
{

    public void Run(string[] args)
    {
        Console.WriteLine("?? Starting AuthKit CLI...");
        
        var registrar = new TypeRegistrar(services);
        var app = new CommandApp(registrar);
       
        app.Configure(cfg =>
        {
            cfg.PropagateExceptions();
            TestCommand.Configure(cfg);
        });
        app.Run(args);
    }
}