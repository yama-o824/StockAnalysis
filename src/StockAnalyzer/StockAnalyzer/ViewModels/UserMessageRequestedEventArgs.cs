namespace StockAnalyzer.ViewModels;

public sealed class UserMessageRequestedEventArgs : EventArgs
{
    public UserMessageRequestedEventArgs(
        string message,
        string title,
        UserMessageKind kind)
    {
        Message = message;
        Title = title;
        Kind = kind;
    }

    public string Message { get; }
    public string Title { get; }
    public UserMessageKind Kind { get; }
}
