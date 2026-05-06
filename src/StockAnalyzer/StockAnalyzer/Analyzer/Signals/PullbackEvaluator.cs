using StockAnalyzer.Models.Analysis;

namespace StockAnalyzer.Analyzer.Signals;

public sealed class PullbackEvaluator
{
    private const double MaTouchThreshold = 0.015d;
    private const double VolumeMaintainThreshold = 0.90d;

    public bool IsPullbackBounce(SignalCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        if (candidate.Type != SignalType.Buy || candidate.Current.Ma75 is null)
        {
            return false;
        }

        var ma75 = candidate.Current.Ma75.Value;
        if (ma75 == 0)
        {
            return false;
        }

        var distanceFromLow = Math.Abs(candidate.Current.Raw.Low - ma75) / ma75;
        var touchedMa = distanceFromLow <= MaTouchThreshold;
        var bullishRebound =
            candidate.Current.Candle.IsBullish &&
            candidate.Current.Raw.Close >= ma75;
        var keptVolume =
            candidate.Current.VolumeRatio is null ||
            candidate.Current.VolumeRatio >= VolumeMaintainThreshold;

        return touchedMa && bullishRebound && keptVolume;
    }
}
