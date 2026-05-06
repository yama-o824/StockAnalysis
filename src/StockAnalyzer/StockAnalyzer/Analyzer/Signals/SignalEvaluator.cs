using StockAnalyzer.Models.Analysis;

namespace StockAnalyzer.Analyzer.Signals;

public sealed class SignalEvaluator
{
    private readonly SignalStrengthEvaluator _signalStrengthEvaluator = new();
    private readonly VolumeSupportEvaluator _volumeSupportEvaluator = new();
    private readonly PullbackEvaluator _pullbackEvaluator = new();

    public SignalResult Evaluate(SignalCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        var signalStrength = _signalStrengthEvaluator.Evaluate(candidate);
        var hasVolumeSupport = _volumeSupportEvaluator.HasSupport(candidate);
        var hasStrongBullishCandle = HasStrongBullishCandle(candidate);
        var isPullbackBounce = _pullbackEvaluator.IsPullbackBounce(candidate);

        return new SignalResult
        {
            Candidate = candidate,
            Evaluation = new SignalEvaluation
            {
                SignalStrength = signalStrength,
                HasVolumeSupport = hasVolumeSupport,
                HasStrongBullishCandle = hasStrongBullishCandle,
                IsPullbackBounce = isPullbackBounce,
                Reasons = BuildReasons(
                    signalStrength,
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
        double? signalStrength,
        bool hasVolumeSupport,
        bool hasStrongBullishCandle,
        bool isPullbackBounce,
        SignalCandidate candidate)
    {
        var reasons = new List<string>();

        if (signalStrength is not null)
        {
            reasons.Add($"SignalStrength={signalStrength.Value:P2}");
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
