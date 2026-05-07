namespace StockAnalyzer.Models.Analysis;

public sealed class CandleMetrics
{
    public double Range { get; init; }
    public double BodySize { get; init; }
    public double BodyRate { get; init; }
    public double UpperShadowRate { get; init; }
    public double ClosePositionRate { get; init; }
    public bool IsBullish { get; init; }
}
