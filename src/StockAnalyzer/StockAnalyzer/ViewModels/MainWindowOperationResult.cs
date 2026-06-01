namespace StockAnalyzer.ViewModels;

public sealed class MainWindowOperationResult
{
    private MainWindowOperationResult(
        bool succeeded,
        string statusMessage,
        string? userMessage,
        Exception? exception,
        string? stderr,
        int? exitCode)
    {
        Succeeded = succeeded;
        StatusMessage = statusMessage;
        UserMessage = userMessage;
        Exception = exception;
        Stderr = stderr;
        ExitCode = exitCode;
    }

    public bool Succeeded { get; }
    public string StatusMessage { get; }
    public string? UserMessage { get; }
    public Exception? Exception { get; }
    public string? Stderr { get; }
    public int? ExitCode { get; }

    public static MainWindowOperationResult Success(
        string statusMessage,
        string? userMessage = null)
    {
        return new MainWindowOperationResult(
            succeeded: true,
            statusMessage,
            userMessage,
            exception: null,
            stderr: null,
            exitCode: null);
    }

    public static MainWindowOperationResult Failure(
        string statusMessage,
        string? userMessage = null,
        Exception? exception = null,
        string? stderr = null,
        int? exitCode = null)
    {
        return new MainWindowOperationResult(
            succeeded: false,
            statusMessage,
            userMessage,
            exception,
            stderr,
            exitCode);
    }
}
