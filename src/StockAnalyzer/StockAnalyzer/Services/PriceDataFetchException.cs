namespace StockAnalyzer.Services;

public sealed class PriceDataFetchException : Exception
{
    public PriceDataFetchException(string message, string? stderr, int? exitCode)
        : base(message)
    {
        Stderr = stderr;
        ExitCode = exitCode;
    }

    public string? Stderr { get; }
    public int? ExitCode { get; }
}
