using System.Reflection;
using AuthKit.Cli;

namespace AuthKit.CLI;

public class ConsoleApp : IConsoleApp
{
    public void Run(string[] args)
    {
        if (args.Length != 1 || (args[0] != "-v" && args[0] != "--version")) return;
        ShowVersion();
    }
    
    private static void ShowVersion()
    {
        var version = Assembly.GetExecutingAssembly()
            .GetName()
            .Version;
        
        var informationalVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        Console.WriteLine($"AuthKit CLI v{informationalVersion ?? version?.ToString() ?? "1.0.0"}");
        Console.WriteLine("Copyright (C) 2025 RyzeSpace");
    }
}