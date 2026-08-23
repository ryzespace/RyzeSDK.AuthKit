namespace Host.Cli;

/// <summary>
/// Represents configuration options for the AuthKit server.
/// </summary>
/// <remarks>
/// <para>
/// Contains the network endpoint, token issuer, storage configuration,
/// and logging configuration used by the server.
/// </para>
/// </remarks>
public sealed class AuthKitServerOptions
{
    /// <summary>
    /// Gets or sets the HTTP endpoint on which the AuthKit server listens to.
    /// </summary>
    public string Host { get; set; } = "http://0.0.0.0:7070";

    /// <summary>
    /// Gets or sets the issuer identifier used by AuthKit when issuing tokens.
    /// </summary>
    public string Issuer { get; set; } = "authkit.local";

    /// <summary>
    /// Gets or sets the storage configuration used by the AuthKit server.
    /// </summary>
    public StorageOptions Storage { get; set; } = new();

    /// <summary>
    /// Gets or sets the logging configuration used by the AuthKit server.
    /// </summary>
    public LoggingOptions Logging { get; set; } = new();

    /// <summary>
    /// Represents configuration options for AuthKit server persistence.
    /// </summary>
    public sealed class StorageOptions
    {
        /// <summary>
        /// Gets or sets the storage provider used to persist AuthKit data.
        /// </summary>
        public string Provider { get; set; } = "marten";

        /// <summary>
        /// Gets or sets the connection string used by the configured storage provider.
        /// </summary>
        public string ConnectionString { get; set; } = "";
    }

    /// <summary>
    /// Represents logging configuration options for the AuthKit server.
    /// </summary>
    public sealed class LoggingOptions
    {
        /// <summary>
        /// Gets or sets value indicating whether server logging is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum logging level.
        /// </summary>
        public string Level { get; set; } = "Information";
    }
}