using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Models.Market;

namespace StockAnalyzer.Analyzer.Candles;

public static class CandleAnalyzer
{
    public static CandleMetrics Analyze(PriceBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        var range = Math.Max(bar.High - bar.Low, 0d);
        var bodySize = Math.Abs(bar.Close - bar.Open);
        var upperShadow = Math.Max(bar.High - Math.Max(bar.Open, bar.Close), 0d);

        var bodyRate = range > 0 ? bodySize / range : 0d;
        var upperShadowRate = range > 0 ? upperShadow / range : 0d;
        var closePositionRate = range > 0 ? (bar.Close - bar.Low) / range : 0d;

        return new CandleMetrics
        {
            Range = range,
            BodySize = bodySize,
            BodyRate = bodyRate,
            UpperShadowRate = upperShadowRate,
            ClosePositionRate = closePositionRate,
            IsBullish = bar.Close >= bar.Open
        };
    }
}
