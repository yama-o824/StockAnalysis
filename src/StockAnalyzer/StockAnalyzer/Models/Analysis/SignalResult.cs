namespace StockAnalyzer.Models.Analysis;

public sealed class SignalResult
{
    public SignalCandidate Candidate { get; init; } = default!;
    public SignalEvaluation Evaluation { get; init; } = default!;
    public SignalScore? Score { get; init; }
}
