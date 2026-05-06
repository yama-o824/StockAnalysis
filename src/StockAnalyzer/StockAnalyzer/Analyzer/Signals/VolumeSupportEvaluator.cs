using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;

namespace StockAnalyzer.Analyzer.Signals;

public sealed class VolumeSupportEvaluator
{
    private const double VolumeSupportThreshold = 1.20d;

    public bool HasSupport(SignalCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        if (candidate.Type != SignalType.Buy)
        {
            return false;
        }

        return candidate.Current.VolumeRatio is >= VolumeSupportThreshold;
    }
}
