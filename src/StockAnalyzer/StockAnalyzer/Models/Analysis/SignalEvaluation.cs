namespace StockAnalyzer.Models.Analysis;

public sealed class SignalEvaluation
{
    public double? Ma75DeviationRate { get; init; }
    public bool HasVolumeSupport { get; init; }
    public bool IsPullbackBounce { get; init; }
    public bool HasStrongBullishCandle { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];
}
