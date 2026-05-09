using StockAnalyzer.Models.Analysis;

namespace StockAnalyzer.Analyzer.Signals;

public sealed class SignalReasonBuilder
{
    public IReadOnlyList<string> Build(
        SignalCandidate candidate,
        SignalEvaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(evaluation);

        var reasons = new List<string>();

        if (evaluation.Ma75DeviationRate is not null)
        {
            reasons.Add($"Ma75DeviationRate={evaluation.Ma75DeviationRate.Value:P2}");
        }

        if (candidate.Current.VolumeRatio is not null)
        {
            reasons.Add($"VolumeRatio={candidate.Current.VolumeRatio.Value:N2}");
        }

        if (evaluation.HasVolumeSupport)
        {
            reasons.Add("出来高を伴う上抜け");
        }

        if (evaluation.HasStrongBullishCandle)
        {
            reasons.Add("強い陽線");
        }

        if (evaluation.IsPullbackBounce)
        {
            reasons.Add("MA75押し目反発");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("クロスのみ");
        }

        return reasons;
    }
}
