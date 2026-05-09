using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;

namespace StockAnalyzer.Analyzer.Signals;

public sealed class SignalScoreCalculator
{
    private const int Ma75DeviationMaxPoints = 30;
    private const int VolumeSupportMaxPoints = 25;
    private const int PullbackBounceMaxPoints = 25;
    private const int StrongBullishCandleMaxPoints = 20;

    public SignalScore? Calculate(
        SignalCandidate candidate,
        SignalEvaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(evaluation);

        if (candidate.Type != SignalType.Buy)
        {
            return null;
        }

        var breakdowns = new List<SignalScoreBreakdown>
        {
            CreateMa75DeviationBreakdown(evaluation.Ma75DeviationRate),
            CreateBooleanBreakdown(
                "VolumeSupport",
                "出来高支持",
                evaluation.HasVolumeSupport,
                VolumeSupportMaxPoints,
                "出来高を伴う上抜け"),
            CreateBooleanBreakdown(
                "PullbackBounce",
                "押し目反発",
                evaluation.IsPullbackBounce,
                PullbackBounceMaxPoints,
                "MA75押し目反発"),
            CreateBooleanBreakdown(
                "StrongBullishCandle",
                "強い陽線",
                evaluation.HasStrongBullishCandle,
                StrongBullishCandleMaxPoints,
                "強い陽線")
        };

        var total = Math.Clamp(breakdowns.Sum(x => x.Points), 0, 100);

        return new SignalScore
        {
            Total = total,
            Rank = ToRank(total),
            Breakdowns = breakdowns
        };
    }

    private static SignalScoreBreakdown CreateMa75DeviationBreakdown(double? ma75DeviationRate)
    {
        var points = ma75DeviationRate switch
        {
            null => 0,
            < 0.03d => 15,
            <= 0.08d => Ma75DeviationMaxPoints,
            _ => 20
        };

        return new SignalScoreBreakdown
        {
            Key = "Ma75Deviation",
            Label = "MA75乖離",
            Points = points,
            MaxPoints = Ma75DeviationMaxPoints,
            Reason = ma75DeviationRate is null
                ? "MA75乖離率なし"
                : $"MA75乖離率={ma75DeviationRate.Value:P2}"
        };
    }

    private static SignalScoreBreakdown CreateBooleanBreakdown(
        string key,
        string label,
        bool matched,
        int maxPoints,
        string reason)
    {
        return new SignalScoreBreakdown
        {
            Key = key,
            Label = label,
            Points = matched ? maxPoints : 0,
            MaxPoints = maxPoints,
            Reason = matched ? reason : "該当なし"
        };
    }

    private static SignalRank ToRank(int total)
    {
        return total switch
        {
            >= 80 => SignalRank.VeryStrong,
            >= 65 => SignalRank.Strong,
            >= 50 => SignalRank.Normal,
            > 0 => SignalRank.Weak,
            _ => SignalRank.None
        };
    }
}
