using StockAnalyzer.Models.Market;

namespace StockAnalyzer.Models.Analysis;

public sealed class AnalysisBar
{
    public PriceBar Raw { get; init; } = default!;
    public double? Ma75 { get; init; }
    public double? Avg20Volume { get; init; }
    public double? VolumeRatio { get; init; }
    public CandleMetrics Candle { get; init; } = default!;
}
