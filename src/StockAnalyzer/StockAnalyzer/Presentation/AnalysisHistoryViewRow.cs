using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;
using System.Globalization;

namespace StockAnalyzer.Presentation;

public sealed class AnalysisHistoryViewRow
{
    public string RunId { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public DateTimeOffset ExecutedAt { get; init; }
    public string ExecutedAtText { get; init; } = string.Empty;
    public string RequestedPeriod { get; init; } = string.Empty;
    public string AnalysisPeriodText { get; init; } = string.Empty;
    public DateOnly SignalDate { get; init; }
    public string SignalDateText { get; init; } = string.Empty;
    public SignalType SignalType { get; init; }
    public string SignalTypeLabel { get; init; } = string.Empty;
    public double Price { get; init; }
    public double Ma75 { get; init; }
    public double PrevPrice { get; init; }
    public double PrevMa75 { get; init; }
    public double PrevDiff { get; init; }
    public double CurrentDiff { get; init; }
    public double? Avg20Volume { get; init; }
    public double? VolumeRatio { get; init; }
    public double? Ma75DeviationRate { get; init; }
    public int? Score { get; init; }
    public SignalRank? Rank { get; init; }
    public string ScoreText { get; init; } = string.Empty;
    public string RankText { get; init; } = string.Empty;
    public string ScoreBreakdown { get; init; } = string.Empty;
    public string Reasons { get; init; } = string.Empty;

    public static AnalysisHistoryViewRow From(AnalysisHistoryRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        return new AnalysisHistoryViewRow
        {
            RunId = record.RunId,
            Symbol = record.Symbol,
            ExecutedAt = record.ExecutedAt,
            ExecutedAtText = record.ExecutedAt.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture),
            RequestedPeriod = record.RequestedPeriod,
            AnalysisPeriodText = $"{FormatDate(record.AnalysisStartDate)} - {FormatDate(record.AnalysisEndDate)}",
            SignalDate = record.SignalDate,
            SignalDateText = FormatDate(record.SignalDate),
            SignalType = record.SignalType,
            SignalTypeLabel = FormatSignalType(record.SignalType),
            Price = record.Price,
            Ma75 = record.Ma75,
            PrevPrice = record.PrevPrice,
            PrevMa75 = record.PrevMa75,
            PrevDiff = record.PrevDiff,
            CurrentDiff = record.CurrentDiff,
            Avg20Volume = record.Avg20Volume,
            VolumeRatio = record.VolumeRatio,
            Ma75DeviationRate = record.Ma75DeviationRate,
            Score = record.Score,
            Rank = record.Rank,
            ScoreText = record.Score?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            RankText = record.Rank?.ToString() ?? string.Empty,
            ScoreBreakdown = record.ScoreBreakdown,
            Reasons = record.Reasons
        };
    }

    private static string FormatDate(DateOnly date)
    {
        return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static string FormatSignalType(SignalType signalType)
    {
        return signalType switch
        {
            SignalType.Buy => "買い",
            SignalType.Sell => "売り",
            _ => signalType.ToString()
        };
    }
}
