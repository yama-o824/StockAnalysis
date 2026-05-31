using StockAnalyzer.Models.Analysis;
using System.IO;
using System.Text;

namespace StockAnalyzer.Services;

public sealed class AnalysisHistoryCsvStore
{
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
            AppendRow(builder, AnalysisHistoryCsvFormat.Headers);
        }

        foreach (var record in records)
        {
            AppendRow(builder, AnalysisHistoryCsvFormat.ToFields(record));
        }

        File.AppendAllText(_filePath, builder.ToString(), Encoding.UTF8);
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

    private static string CreateDefaultFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appDataPath, "StockAnalyzer", "analysis-history.csv");
    }
}
