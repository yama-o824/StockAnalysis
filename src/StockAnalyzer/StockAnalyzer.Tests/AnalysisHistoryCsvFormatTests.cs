using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Services;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class AnalysisHistoryCsvFormatTests
{
    [Fact(DisplayName = "分析履歴CSVのヘッダー順を定義する")]
    public void Headers_ReturnsExpectedColumns()
    {
        string[] expected =
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

        Assert.Equal(expected, AnalysisHistoryCsvFormat.Headers);
    }

    [Fact(DisplayName = "分析履歴レコードをCSVフィールド順に変換する")]
    public void ToFields_ReturnsFieldsInHeaderOrder()
    {
        var record = new AnalysisHistoryRecord
        {
            RunId = "run-1",
            Symbol = "7203.T",
            ExecutedAt = new DateTimeOffset(2026, 6, 1, 9, 30, 0, TimeSpan.FromHours(9)),
            RequestedPeriod = "1y",
            AnalysisStartDate = new DateOnly(2026, 1, 1),
            AnalysisEndDate = new DateOnly(2026, 6, 1),
            SignalDate = new DateOnly(2026, 5, 31),
            SignalType = SignalType.Buy,
            Price = 105.5d,
            Ma75 = 101.25d,
            PrevPrice = 99d,
            PrevMa75 = 100d,
            PrevDiff = -1d,
            CurrentDiff = 4.25d,
            Avg20Volume = 1200d,
            VolumeRatio = 1.25d,
            Ma75DeviationRate = 0.04197530864197531d,
            Score = 80,
            Rank = SignalRank.Strong,
            ScoreBreakdown = "出来高 20/20",
            Reasons = "出来高を伴う上抜け / 強い陽線"
        };

        var fields = AnalysisHistoryCsvFormat.ToFields(record);

        string[] expected =
        [
                "1",
                "run-1",
                "7203.T",
                "2026-06-01T09:30:00.0000000+09:00",
                "1y",
                "2026-01-01",
                "2026-06-01",
                "2026-05-31",
                "Buy",
                "105.5",
                "101.25",
                "99",
                "100",
                "-1",
                "4.25",
                "1200",
                "1.25",
                "0.041975308641975309",
                "80",
                "Strong",
                "出来高 20/20",
                "出来高を伴う上抜け / 強い陽線"
        ];

        Assert.Equal(expected, fields);
    }

    [Fact(DisplayName = "null許容の数値とスコア情報は空文字に変換する")]
    public void ToFields_NullOptionalValues_ReturnsEmptyFields()
    {
        var record = new AnalysisHistoryRecord
        {
            RunId = "run-1",
            Symbol = "7203.T",
            ExecutedAt = new DateTimeOffset(2026, 6, 1, 9, 30, 0, TimeSpan.FromHours(9)),
            RequestedPeriod = "1y",
            AnalysisStartDate = new DateOnly(2026, 1, 1),
            AnalysisEndDate = new DateOnly(2026, 6, 1),
            SignalDate = new DateOnly(2026, 5, 31),
            SignalType = SignalType.Sell
        };

        var fields = AnalysisHistoryCsvFormat.ToFields(record);

        Assert.Equal(string.Empty, fields[15]);
        Assert.Equal(string.Empty, fields[16]);
        Assert.Equal(string.Empty, fields[17]);
        Assert.Equal(string.Empty, fields[18]);
        Assert.Equal(string.Empty, fields[19]);
    }

    [Fact(DisplayName = "CSVフィールド順から分析履歴レコードを復元する")]
    public void FromFields_ReturnsRecord()
    {
        string[] fields =
        [
                "1",
                "run-1",
                "7203.T",
                "2026-06-01T09:30:00.0000000+09:00",
                "1y",
                "2026-01-01",
                "2026-06-01",
                "2026-05-31",
                "Buy",
                "105.5",
                "101.25",
                "99",
                "100",
                "-1",
                "4.25",
                "1200",
                "1.25",
                "0.041975308641975309",
                "80",
                "Strong",
                "出来高 20/20",
                "出来高を伴う上抜け / 強い陽線"
        ];

        var record = AnalysisHistoryCsvFormat.FromFields(fields);

        Assert.Equal(1, record.SchemaVersion);
        Assert.Equal("run-1", record.RunId);
        Assert.Equal("7203.T", record.Symbol);
        Assert.Equal(new DateTimeOffset(2026, 6, 1, 9, 30, 0, TimeSpan.FromHours(9)), record.ExecutedAt);
        Assert.Equal("1y", record.RequestedPeriod);
        Assert.Equal(new DateOnly(2026, 1, 1), record.AnalysisStartDate);
        Assert.Equal(new DateOnly(2026, 6, 1), record.AnalysisEndDate);
        Assert.Equal(new DateOnly(2026, 5, 31), record.SignalDate);
        Assert.Equal(SignalType.Buy, record.SignalType);
        Assert.Equal(105.5d, record.Price);
        Assert.Equal(101.25d, record.Ma75);
        Assert.Equal(99d, record.PrevPrice);
        Assert.Equal(100d, record.PrevMa75);
        Assert.Equal(-1d, record.PrevDiff);
        Assert.Equal(4.25d, record.CurrentDiff);
        Assert.Equal(1200d, record.Avg20Volume);
        Assert.Equal(1.25d, record.VolumeRatio);
        Assert.Equal(0.041975308641975309d, record.Ma75DeviationRate);
        Assert.Equal(80, record.Score);
        Assert.Equal(SignalRank.Strong, record.Rank);
        Assert.Equal("出来高 20/20", record.ScoreBreakdown);
        Assert.Equal("出来高を伴う上抜け / 強い陽線", record.Reasons);
    }

    [Fact(DisplayName = "未対応のSchemaVersionは読み込み失敗にする")]
    public void FromFields_UnsupportedSchemaVersion_Throws()
    {
        var fields = AnalysisHistoryCsvFormat.ToFields(new AnalysisHistoryRecord
        {
            SchemaVersion = 2,
            RunId = "run-1",
            Symbol = "7203.T",
            ExecutedAt = new DateTimeOffset(2026, 6, 1, 9, 30, 0, TimeSpan.FromHours(9)),
            RequestedPeriod = "1y",
            AnalysisStartDate = new DateOnly(2026, 1, 1),
            AnalysisEndDate = new DateOnly(2026, 6, 1),
            SignalDate = new DateOnly(2026, 5, 31),
            SignalType = SignalType.Buy
        });

        Assert.Throws<NotSupportedException>(() => AnalysisHistoryCsvFormat.FromFields(fields));
    }

    [Fact(DisplayName = "未定義のSignalType数値は読み込み失敗にする")]
    public void FromFields_UndefinedSignalTypeNumber_Throws()
    {
        var fields = CreateValidFields();
        fields[8] = "999";

        Assert.Throws<FormatException>(() => AnalysisHistoryCsvFormat.FromFields(fields));
    }

    [Fact(DisplayName = "未定義のRank数値は読み込み失敗にする")]
    public void FromFields_UndefinedRankNumber_Throws()
    {
        var fields = CreateValidFields();
        fields[19] = "999";

        Assert.Throws<FormatException>(() => AnalysisHistoryCsvFormat.FromFields(fields));
    }

    private static string[] CreateValidFields()
    {
        return
        [
                "1",
                "run-1",
                "7203.T",
                "2026-06-01T09:30:00.0000000+09:00",
                "1y",
                "2026-01-01",
                "2026-06-01",
                "2026-05-31",
                "Buy",
                "105.5",
                "101.25",
                "99",
                "100",
                "-1",
                "4.25",
                "1200",
                "1.25",
                "0.041975308641975309",
                "80",
                "Strong",
                "出来高 20/20",
                "出来高を伴う上抜け / 強い陽線"
        ];
    }
}
