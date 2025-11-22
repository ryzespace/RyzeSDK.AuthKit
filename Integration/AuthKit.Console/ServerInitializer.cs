namespace AuthKit.Console;

public class ServerInitializer
{
    private readonly string _initFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AuthKit", ".authkit_initialized"
    );

    public bool IsFirstRun() => !File.Exists(_initFile);

    public void MarkInitialized()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_initFile)!);
        File.WriteAllText(_initFile, DateTime.UtcNow.ToString("O"));
    }
}