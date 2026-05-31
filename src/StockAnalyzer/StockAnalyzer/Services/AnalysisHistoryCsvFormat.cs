using StockAnalyzer.Models.Analysis;
using System.Globalization;

namespace StockAnalyzer.Services;

public static class AnalysisHistoryCsvFormat
{
    public static readonly string[] Headers =
    [
        "SchemaVersion",
        "RunId",
        "Symbol",
        "ExecutedAt",
        "RequestedPeriod",
        "AnalysisStartDate",
        "AnalysisEndDate",
        "SignalDate",
        "SignalType",
        "Price",
        "MA75",
        "PrevPrice",
        "PrevMA75",
        "PrevDiff",
        "CurrentDiff",
        "Avg20Volume",
        "VolumeRatio",
        "MA75DeviationRate",
        "Score",
        "Rank",
        "ScoreBreakdown",
        "Reasons"
    ];

    public static IReadOnlyList<string> ToFields(AnalysisHistoryRecord record)
    {
        return
        [
            record.SchemaVersion.ToString(CultureInfo.InvariantCulture),
            record.RunId,
            record.Symbol,
            record.ExecutedAt.ToString("O", CultureInfo.InvariantCulture),
            record.RequestedPeriod,
            record.AnalysisStartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            record.AnalysisEndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            record.SignalDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            record.SignalType.ToString(),
            Format(record.Price),
            Format(record.Ma75),
            Format(record.PrevPrice),
            Format(record.PrevMa75),
            Format(record.PrevDiff),
            Format(record.CurrentDiff),
            Format(record.Avg20Volume),
            Format(record.VolumeRatio),
            Format(record.Ma75DeviationRate),
            record.Score?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            record.Rank?.ToString() ?? string.Empty,
            record.ScoreBreakdown,
            record.Reasons
        ];
    }

    private static string Format(double value)
    {
        return value.ToString("G17", CultureInfo.InvariantCulture);
    }

    private static string Format(double? value)
    {
        return value?.ToString("G17", CultureInfo.InvariantCulture) ?? string.Empty;
    }
}
