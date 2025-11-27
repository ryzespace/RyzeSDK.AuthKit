using Spectre.Console.Cli;

namespace AuthKit.Cli.Commands;

public sealed class TestCommand : AsyncCommand<TestCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-u|--user <USER>")]
        public string User { get; init; } = default!;

        [CommandOption("-p|--pass <PASS>")]
        public string Password { get; init; } = default!;
    }

    public static void Configure(IConfigurator cfg)
    {
        cfg.AddCommand<TestCommand>("test")
            .WithDescription("test AuthKit");
    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        Console.WriteLine($"testing {settings.User}...");
        return Task.FromResult(0);
    }
}