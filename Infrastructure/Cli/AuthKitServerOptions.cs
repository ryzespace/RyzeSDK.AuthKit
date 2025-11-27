namespace Infrastructure.Cli;

public sealed class AuthKitServerOptions
{
    public string Host { get; set; } = "http://0.0.0.0:7070";
    public string Issuer { get; set; } = "authkit.local";

    public StorageOptions Storage { get; set; } = new();

    public LoggingOptions Logging { get; set; } = new();

    public sealed class StorageOptions
    {
        public string Provider { get; set; } = "marten";
        public string ConnectionString { get; set; } = "";
    }

    public sealed class LoggingOptions
    {
        public bool Enabled { get; set; } = true;
        public string Level { get; set; } = "Information";
    }
}