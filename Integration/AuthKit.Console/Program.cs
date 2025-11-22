using AuthKit.Console.Configuration;
using Microsoft.Extensions.Hosting;

using var host = Host.CreateDefaultBuilder(args)
    .UseAuthKitConsole()
    .Build();
