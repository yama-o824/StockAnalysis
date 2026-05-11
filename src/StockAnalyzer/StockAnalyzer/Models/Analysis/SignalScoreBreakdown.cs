namespace StockAnalyzer.Models.Analysis;

public sealed class SignalScoreBreakdown
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int Points { get; init; }
    public int MaxPoints { get; init; }
    public string Reason { get; init; } = string.Empty;
}
