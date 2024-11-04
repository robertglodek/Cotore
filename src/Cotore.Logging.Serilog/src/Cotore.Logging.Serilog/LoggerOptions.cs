namespace Cotore.Logging.Serilog;

public sealed class LoggerOptions
{
    public string? Level { get; set; }
    public ConsoleOptions? Console { get; set; }
    public FileOptions? File { get; set; }
    public SeqOptions? Seq { get; set; }
    public MongoDBOptions? Mongo { get; set; }
    public ElkOptions? Elk { get; set; }
    public IDictionary<string, string>? MinimumLevelOverrides { get; set; }
    public IEnumerable<string>? ExcludePaths { get; set; }
    public IEnumerable<string>? ExcludeProperties { get; set; }
    public IDictionary<string, object>? Tags { get; set; }

    public sealed class ConsoleOptions
    {
        public bool Enabled { get; set; }
        public string? Template { get; set; }
    }

    public sealed class FileOptions
    {
        public bool Enabled { get; set; }
        public string Path { get; set; } = null!;
        public string? Interval { get; set; }
        public string? Template { get; set; }
    }

    public sealed class MongoDBOptions
    {
        public bool Enabled { get; set; }
        public string Url { get; set; } = null!;
        public string Collection { get; set; } = null!;
    }

    public sealed class SeqOptions
    {
        public bool Enabled { get; set; }
        public string Url { get; set; } = null!;
        public string? ApiKey { get; set; }
    }

    public sealed class ElkOptions
    {
        public bool Enabled { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool BasicAuthEnabled { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}