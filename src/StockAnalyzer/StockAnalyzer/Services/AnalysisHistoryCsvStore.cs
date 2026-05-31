using StockAnalyzer.Models.Analysis;
using System.Globalization;
using System.IO;
using System.Text;

namespace StockAnalyzer.Services;

public sealed class AnalysisHistoryCsvStore
{
    private static readonly string[] Headers =
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

    private readonly string _filePath;

    public AnalysisHistoryCsvStore()
        : this(CreateDefaultFilePath())
    {
    }

    public AnalysisHistoryCsvStore(string filePath)
    {
        _filePath = filePath;
    }

    public string FilePath => _filePath;

    public void Append(IReadOnlyList<AnalysisHistoryRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        if (records.Count == 0)
        {
            return;
        }

        var directoryPath = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var shouldWriteHeader = !File.Exists(_filePath) || new FileInfo(_filePath).Length == 0;
        var builder = new StringBuilder();

        if (shouldWriteHeader)
        {
            AppendRow(builder, Headers);
        }

        foreach (var record in records)
        {
            AppendRow(builder, ToFields(record));
        }

        File.AppendAllText(_filePath, builder.ToString(), Encoding.UTF8);
    }

    private static IReadOnlyList<string> ToFields(AnalysisHistoryRecord record)
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

    private static void AppendRow(StringBuilder builder, IReadOnlyList<string> fields)
    {
        builder.AppendLine(string.Join(",", fields.Select(Escape)));
    }

    private static string Escape(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\r') && !value.Contains('\n'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static string Format(double value)
    {
        return value.ToString("G17", CultureInfo.InvariantCulture);
    }

    private static string Format(double? value)
    {
        return value?.ToString("G17", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static string CreateDefaultFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appDataPath, "StockAnalyzer", "analysis-history.csv");
    }
}
