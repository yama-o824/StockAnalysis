using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;

namespace StockAnalyzer.Analyzer.Signals;

public sealed class StrongBullishCandleEvaluator
{
    public bool IsStrongBullishCandle(SignalCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        if (candidate.Type != SignalType.Buy)
        {
            return false;
        }

        var candle = candidate.Current.Candle;
        return candle.IsBullish
            && candle.BodyRate >= 0.50d
            && candle.UpperShadowRate <= 0.30d
            && candle.ClosePositionRate >= 0.70d;
    }
}
