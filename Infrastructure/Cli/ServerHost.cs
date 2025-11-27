using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace Infrastructure.Cli;

public sealed class ServerHost(IOptions<AuthKitServerOptions> opts) : BackgroundService
{
    private readonly AuthKitServerOptions _opts = opts.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        // Panel powitalny
        var panel = new Panel($"[bold green]AuthKit Server[/]\n[bold blue]Listening on:[/] {_opts.Host}\n[bold yellow]Issuer:[/] {_opts.Issuer}")
        {
            Border = BoxBorder.Double,
            Padding = new Padding(1, 1),
            Header = new PanelHeader("🚀 Server Started")
        };
        AnsiConsole.Write(panel);

        // Stworzenie tabeli statusu
        var table = new Table();
        table.AddColumn("[green]Service[/]");
        table.AddColumn("[yellow]Status[/]");
        table.AddRow("JWT KeyStore", "Initializing...");
        table.AddRow("Developer Tokens", "Initializing...");
        AnsiConsole.Write(table);

        // Pętla serwera
        while (!stoppingToken.IsCancellationRequested)
        {
            // Można tu np. aktualizować statusy usług
            table.Rows.Clear();
            table.AddRow("JWT KeyStore", "[green]Ready[/]");
            table.AddRow("Developer Tokens", "[green]Ready[/]");
            AnsiConsole.Write(new Panel(table) { Border = BoxBorder.Rounded });

            await Task.Delay(2000, stoppingToken); // odświeżanie co 2 sekundy
        }

        AnsiConsole.MarkupLine("[red]Server shutting down...[/]");
    }
}