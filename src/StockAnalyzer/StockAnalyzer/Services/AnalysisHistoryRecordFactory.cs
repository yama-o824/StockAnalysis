using StockAnalyzer.Models.Analysis;

namespace StockAnalyzer.Services;

public sealed class AnalysisHistoryRecordFactory
{
    public IReadOnlyList<AnalysisHistoryRecord> Create(
        string symbol,
        string requestedPeriod,
        DateTimeOffset executedAt,
        string runId,
        AnalysisResult analysisResult)
    {
        ArgumentNullException.ThrowIfNull(analysisResult);

        if (analysisResult.Bars.Count == 0)
        {
            return [];
        }

        var analysisStartDate = analysisResult.Bars[0].Raw.Date;
        var analysisEndDate = analysisResult.Bars[^1].Raw.Date;

        return analysisResult.Signals
            .Select(signal => CreateRecord(
                symbol,
                requestedPeriod,
                executedAt,
                runId,
                analysisStartDate,
                analysisEndDate,
                signal))
            .ToList();
    }

    private static AnalysisHistoryRecord CreateRecord(
        string symbol,
        string requestedPeriod,
        DateTimeOffset executedAt,
        string runId,
        DateOnly analysisStartDate,
        DateOnly analysisEndDate,
        SignalResult signal)
    {
        var previous = signal.Candidate.Previous;
        var current = signal.Candidate.Current;
        var previousMa75 = previous.Ma75
            ?? throw new InvalidOperationException("Previous MA75 is required.");
        var currentMa75 = current.Ma75
            ?? throw new InvalidOperationException("Current MA75 is required.");

        return new AnalysisHistoryRecord
        {
            RunId = runId,
            Symbol = symbol,
            ExecutedAt = executedAt,
            RequestedPeriod = requestedPeriod,
            AnalysisStartDate = analysisStartDate,
            AnalysisEndDate = analysisEndDate,
            SignalDate = signal.Candidate.Date,
            SignalType = signal.Candidate.Type,
            Price = current.Raw.Close,
            Ma75 = currentMa75,
            PrevPrice = previous.Raw.Close,
            PrevMa75 = previousMa75,
            PrevDiff = previous.Raw.Close - previousMa75,
            CurrentDiff = current.Raw.Close - currentMa75,
            Avg20Volume = current.Avg20Volume,
            VolumeRatio = current.VolumeRatio,
            Ma75DeviationRate = signal.Evaluation.Ma75DeviationRate,
            Score = signal.Score?.Total,
            Rank = signal.Score?.Rank,
            ScoreBreakdown = FormatScoreBreakdown(signal.Score),
            Reasons = string.Join(" / ", signal.Evaluation.Reasons)
        };
    }

    private static string FormatScoreBreakdown(SignalScore? score)
    {
        if (score is null)
        {
            return string.Empty;
        }

        return string.Join(
            " / ",
            score.Breakdowns.Select(x => $"{x.Label} {x.Points}/{x.MaxPoints}"));
    }
}
