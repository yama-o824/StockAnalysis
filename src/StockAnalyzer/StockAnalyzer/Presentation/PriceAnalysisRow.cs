using StockAnalyzer.Models.Analysis;

namespace StockAnalyzer.Presentation;

public sealed class PriceAnalysisRow
{
    public string Date { get; init; } = string.Empty;
    public double Open { get; init; }
    public double High { get; init; }
    public double Low { get; init; }
    public double Close { get; init; }
    public long Volume { get; init; }
    public double? MA75 { get; init; }
    public double? Avg20Volume { get; init; }
    public double? VolumeRatio { get; init; }
    public double BodyRate { get; init; }
    public double UpperShadowRate { get; init; }
    public double ClosePositionRate { get; init; }

    public static PriceAnalysisRow From(AnalysisBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        return new PriceAnalysisRow
        {
            Date = bar.Raw.Date.ToString("yyyy-MM-dd"),
            Open = bar.Raw.Open,
            High = bar.Raw.High,
            Low = bar.Raw.Low,
            Close = bar.Raw.Close,
            Volume = bar.Raw.Volume,
            MA75 = bar.Ma75,
            Avg20Volume = bar.Avg20Volume,
            VolumeRatio = bar.VolumeRatio,
            BodyRate = bar.Candle.BodyRate,
            UpperShadowRate = bar.Candle.UpperShadowRate,
            ClosePositionRate = bar.Candle.ClosePositionRate
        };
    }
}
