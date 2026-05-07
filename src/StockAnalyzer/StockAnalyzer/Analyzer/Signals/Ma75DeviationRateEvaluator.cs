using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;

namespace StockAnalyzer.Analyzer.Signals;

public sealed class Ma75DeviationRateEvaluator
{
    public double? Evaluate(SignalCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        var ma75 = candidate.Current.Ma75;
        if (ma75 is null || ma75.Value == 0)
        {
            return null;
        }

        var strength = (candidate.Current.Raw.Close - ma75.Value) / ma75.Value;
        return candidate.Type == SignalType.Sell
            ? -strength
            : strength;
    }
}
