namespace Cotore.Logging.Serilog;

public sealed class LoggerOptions
{
    public string? Level { get; init; }
    public ConsoleOptions? Console { get; init; }
    public FileOptions? File { get; init; }
    public SeqOptions? Seq { get; init; }
    public MongoDbOptions? Mongo { get; init; }
    public ElkOptions? Elk { get; init; }
    public IDictionary<string, string>? MinimumLevelOverrides { get; init; }
    public IEnumerable<string>? ExcludePaths { get; init; }
    public IEnumerable<string>? ExcludeProperties { get; init; }
    public IDictionary<string, object>? Tags { get; init; }

    public sealed class ConsoleOptions
    {
        public bool Enabled { get; init; }
        public string? Template { get; init; }
    }

    public sealed class FileOptions
    {
        public bool Enabled { get; init; }
        public string Path { get; init; } = null!;
        public string? Interval { get; init; }
        public string? Template { get; init; }
    }

    public sealed class MongoDbOptions
    {
        public bool Enabled { get; init; }
        public string Url { get; init; } = null!;
        public string Collection { get; init; } = null!;
    }

    public sealed class SeqOptions
    {
        public bool Enabled { get; init; }
        public string Url { get; init; } = null!;
        public string? ApiKey { get; init; }
    }

    public sealed class ElkOptions
    {
        public bool Enabled { get; init; }
        public string Url { get; init; } = string.Empty;
        public bool BasicAuthEnabled { get; init; }
        public string? Username { get; init; }
        public string? Password { get; init; }
    }
}