using StockAnalyzer.Models.Analysis;

namespace StockAnalyzer.Analyzer.Signals;

public sealed class SignalEvaluator
{
    private readonly Ma75DeviationRateEvaluator _ma75DeviationRateEvaluator = new();
    private readonly VolumeSupportEvaluator _volumeSupportEvaluator = new();
    private readonly StrongBullishCandleEvaluator _strongBullishCandleEvaluator = new();
    private readonly PullbackEvaluator _pullbackEvaluator = new();
    private readonly SignalReasonBuilder _signalReasonBuilder = new();

    public SignalResult Evaluate(SignalCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        var ma75DeviationRate = _ma75DeviationRateEvaluator.Evaluate(candidate);
        var hasVolumeSupport = _volumeSupportEvaluator.HasSupport(candidate);
        var hasStrongBullishCandle = _strongBullishCandleEvaluator.IsStrongBullishCandle(candidate);
        var isPullbackBounce = _pullbackEvaluator.IsPullbackBounce(candidate);
        var evaluation = new SignalEvaluation
        {
            Ma75DeviationRate = ma75DeviationRate,
            HasVolumeSupport = hasVolumeSupport,
            HasStrongBullishCandle = hasStrongBullishCandle,
            IsPullbackBounce = isPullbackBounce
        };

        return new SignalResult
        {
            Candidate = candidate,
            Evaluation = new SignalEvaluation
            {
                Ma75DeviationRate = evaluation.Ma75DeviationRate,
                HasVolumeSupport = evaluation.HasVolumeSupport,
                HasStrongBullishCandle = evaluation.HasStrongBullishCandle,
                IsPullbackBounce = evaluation.IsPullbackBounce,
                Reasons = _signalReasonBuilder.Build(candidate, evaluation)
            }
        };
    }
}
