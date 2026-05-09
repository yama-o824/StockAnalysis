using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;

namespace StockAnalyzer.Analyzer.Signals;

public sealed class SignalEvaluator
{
    private readonly Ma75DeviationRateEvaluator _ma75DeviationRateEvaluator = new();
    private readonly VolumeSupportEvaluator _volumeSupportEvaluator = new();
    private readonly PullbackEvaluator _pullbackEvaluator = new();

    public SignalResult Evaluate(SignalCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        var ma75DeviationRate = _ma75DeviationRateEvaluator.Evaluate(candidate);
        var hasVolumeSupport = _volumeSupportEvaluator.HasSupport(candidate);
        var hasStrongBullishCandle = HasStrongBullishCandle(candidate);
        var isPullbackBounce = _pullbackEvaluator.IsPullbackBounce(candidate);

        return new SignalResult
        {
            Candidate = candidate,
            Evaluation = new SignalEvaluation
            {
                Ma75DeviationRate = ma75DeviationRate,
                HasVolumeSupport = hasVolumeSupport,
                HasStrongBullishCandle = hasStrongBullishCandle,
                IsPullbackBounce = isPullbackBounce,
                Reasons = BuildReasons(
                    ma75DeviationRate,
                    hasVolumeSupport,
                    hasStrongBullishCandle,
                    isPullbackBounce,
                    candidate)
            }
        };
    }

    private static bool HasStrongBullishCandle(SignalCandidate candidate)
    {
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

    private static IReadOnlyList<string> BuildReasons(
        double? ma75DeviationRate,
        bool hasVolumeSupport,
        bool hasStrongBullishCandle,
        bool isPullbackBounce,
        SignalCandidate candidate)
    {
        var reasons = new List<string>();

        if (ma75DeviationRate is not null)
        {
            reasons.Add($"Ma75DeviationRate={ma75DeviationRate.Value:P2}");
        }

        if (candidate.Current.VolumeRatio is not null)
        {
            reasons.Add($"VolumeRatio={candidate.Current.VolumeRatio.Value:N2}");
        }

        if (hasVolumeSupport)
        {
            reasons.Add("出来高を伴う上抜け");
        }

        if (hasStrongBullishCandle)
        {
            reasons.Add("強い陽線");
        }

        if (isPullbackBounce)
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
