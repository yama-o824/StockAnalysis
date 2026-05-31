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
}
