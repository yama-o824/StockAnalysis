namespace StockAnalyzer.Models.Analysis;

public sealed class SignalCandidate
{
    public DateOnly Date => Current.Raw.Date;
    public SignalType Type { get; init; }
    public AnalysisBar Previous { get; init; } = default!;
    public AnalysisBar Current { get; init; } = default!;
}
