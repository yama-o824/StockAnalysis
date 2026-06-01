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

    public IReadOnlyList<AnalysisHistoryRecord> Load()
    {
        if (!File.Exists(_filePath) || new FileInfo(_filePath).Length == 0)
        {
            return [];
        }

        var rows = ParseCsv(File.ReadAllText(_filePath, Encoding.UTF8));
        if (rows.Count == 0)
        {
            return [];
        }

        if (!rows[0].SequenceEqual(AnalysisHistoryCsvFormat.Headers))
        {
            throw new InvalidDataException("分析履歴CSVのヘッダーが不正です。");
        }

        return rows
            .Skip(1)
            .Select(AnalysisHistoryCsvFormat.FromFields)
            .ToList();
    }

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

    private static IReadOnlyList<IReadOnlyList<string>> ParseCsv(string text)
    {
        var rows = new List<IReadOnlyList<string>>();
        var fields = new List<string>();
        var fieldBuilder = new StringBuilder();
        var inQuotes = false;
        var hasAnyCharacterInRow = false;

        for (var i = 0; i < text.Length; i++)
        {
            var current = text[i];
            hasAnyCharacterInRow = true;

            if (inQuotes)
            {
                if (current == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        fieldBuilder.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    fieldBuilder.Append(current);
                }

                continue;
            }

            if (current == '"')
            {
                inQuotes = true;
                continue;
            }

            if (current == ',')
            {
                fields.Add(fieldBuilder.ToString());
                fieldBuilder.Clear();
                continue;
            }

            if (current == '\r' || current == '\n')
            {
                fields.Add(fieldBuilder.ToString());
                fieldBuilder.Clear();
                rows.Add(fields.ToList());
                fields.Clear();
                hasAnyCharacterInRow = false;

                if (current == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                continue;
            }

            fieldBuilder.Append(current);
        }

        if (inQuotes)
        {
            throw new FormatException("分析履歴CSVの引用符が閉じられていません。");
        }

        if (hasAnyCharacterInRow || fieldBuilder.Length > 0 || fields.Count > 0)
        {
            fields.Add(fieldBuilder.ToString());
            rows.Add(fields.ToList());
        }

        return rows;
    }

    private static string CreateDefaultFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appDataPath, "StockAnalyzer", "analysis-history.csv");
    }
}
