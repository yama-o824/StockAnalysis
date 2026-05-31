namespace StockAnalyzer.Models.Analysis;

public sealed class AnalysisHistoryRecord
{
    public int SchemaVersion { get; init; } = 1;
    public string RunId { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public DateTimeOffset ExecutedAt { get; init; }
    public string RequestedPeriod { get; init; } = string.Empty;
    public DateOnly AnalysisStartDate { get; init; }
    public DateOnly AnalysisEndDate { get; init; }
    public DateOnly SignalDate { get; init; }
    public SignalType SignalType { get; init; }
    public double Price { get; init; }
    public double Ma75 { get; init; }
    public double PrevPrice { get; init; }
    public double PrevMa75 { get; init; }
    public double PrevDiff { get; init; }
    public double CurrentDiff { get; init; }
    public double? Avg20Volume { get; init; }
    public double? VolumeRatio { get; init; }
    public double? Ma75DeviationRate { get; init; }
    public int? Score { get; init; }
    public SignalRank? Rank { get; init; }
    public string ScoreBreakdown { get; init; } = string.Empty;
    public string Reasons { get; init; } = string.Empty;
}
