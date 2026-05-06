namespace StockAnalyzer.Models.Analysis;

public sealed class AnalysisResult
{
    public IReadOnlyList<AnalysisBar> Bars { get; init; } = [];
    public IReadOnlyList<SignalResult> Signals { get; init; } = [];
}
