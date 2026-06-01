using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Models;
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

    public static AnalysisHistoryRecord FromFields(IReadOnlyList<string> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);

        if (fields.Count != Headers.Length)
        {
            throw new FormatException($"分析履歴CSVの列数が不正です。Expected: {Headers.Length}, Actual: {fields.Count}");
        }

        var schemaVersion = ParseInt(fields[0], Headers[0]);
        if (schemaVersion != 1)
        {
            throw new NotSupportedException($"未対応の分析履歴CSVスキーマです。SchemaVersion: {schemaVersion}");
        }

        return new AnalysisHistoryRecord
        {
            SchemaVersion = schemaVersion,
            RunId = fields[1],
            Symbol = fields[2],
            ExecutedAt = ParseDateTimeOffset(fields[3], Headers[3]),
            RequestedPeriod = fields[4],
            AnalysisStartDate = ParseDateOnly(fields[5], Headers[5]),
            AnalysisEndDate = ParseDateOnly(fields[6], Headers[6]),
            SignalDate = ParseDateOnly(fields[7], Headers[7]),
            SignalType = ParseEnum<SignalType>(fields[8], Headers[8]),
            Price = ParseDouble(fields[9], Headers[9]),
            Ma75 = ParseDouble(fields[10], Headers[10]),
            PrevPrice = ParseDouble(fields[11], Headers[11]),
            PrevMa75 = ParseDouble(fields[12], Headers[12]),
            PrevDiff = ParseDouble(fields[13], Headers[13]),
            CurrentDiff = ParseDouble(fields[14], Headers[14]),
            Avg20Volume = ParseNullableDouble(fields[15], Headers[15]),
            VolumeRatio = ParseNullableDouble(fields[16], Headers[16]),
            Ma75DeviationRate = ParseNullableDouble(fields[17], Headers[17]),
            Score = ParseNullableInt(fields[18], Headers[18]),
            Rank = ParseNullableEnum<SignalRank>(fields[19], Headers[19]),
            ScoreBreakdown = fields[20],
            Reasons = fields[21]
        };
    }

    private static string Format(double value)
    {
        return value.ToString("G17", CultureInfo.InvariantCulture);
    }

    private static string Format(double? value)
    {
        return value?.ToString("G17", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static int ParseInt(string value, string columnName)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        throw new FormatException($"{columnName} の値が整数として読み込めません。Value: {value}");
    }

    private static int? ParseNullableInt(string value, string columnName)
    {
        return string.IsNullOrEmpty(value)
            ? null
            : ParseInt(value, columnName);
    }

    private static double ParseDouble(string value, string columnName)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        throw new FormatException($"{columnName} の値が数値として読み込めません。Value: {value}");
    }

    private static double? ParseNullableDouble(string value, string columnName)
    {
        return string.IsNullOrEmpty(value)
            ? null
            : ParseDouble(value, columnName);
    }

    private static DateTimeOffset ParseDateTimeOffset(string value, string columnName)
    {
        if (DateTimeOffset.TryParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        throw new FormatException($"{columnName} の値が日時として読み込めません。Value: {value}");
    }

    private static DateOnly ParseDateOnly(string value, string columnName)
    {
        if (DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        throw new FormatException($"{columnName} の値が日付として読み込めません。Value: {value}");
    }

    private static TEnum ParseEnum<TEnum>(string value, string columnName)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(value, ignoreCase: false, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new FormatException($"{columnName} の値が不正です。Value: {value}");
    }

    private static TEnum? ParseNullableEnum<TEnum>(string value, string columnName)
        where TEnum : struct, Enum
    {
        return string.IsNullOrEmpty(value)
            ? null
            : ParseEnum<TEnum>(value, columnName);
    }
}
