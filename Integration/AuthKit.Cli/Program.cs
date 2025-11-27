using AuthKit.Cli;
using AuthKit.Cli.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using var host = Host.CreateDefaultBuilder(args)
    .UseAuthKitConsole()
    .Build();

var app = host.Services.GetRequiredService<ConsoleApp>();
app.Run(args);