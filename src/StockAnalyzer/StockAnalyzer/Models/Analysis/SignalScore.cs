namespace StockAnalyzer.Models.Analysis;

public sealed class SignalScore
{
    public int Total { get; init; }
    public SignalRank Rank { get; init; }
    public IReadOnlyList<SignalScoreBreakdown> Breakdowns { get; init; } = [];
}
