using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Presentation;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class AnalysisHistoryViewRowTests
{
    [Fact(DisplayName = "分析履歴レコードを履歴表示行に変換する")]
    public void From_ReturnsViewRow()
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

        var row = AnalysisHistoryViewRow.From(record);

        Assert.Equal("run-1", row.RunId);
        Assert.Equal("7203.T", row.Symbol);
        Assert.Equal(record.ExecutedAt, row.ExecutedAt);
        Assert.Equal("2026-06-01 09:30:00 +09:00", row.ExecutedAtText);
        Assert.Equal("1y", row.RequestedPeriod);
        Assert.Equal("2026-01-01 - 2026-06-01", row.AnalysisPeriodText);
        Assert.Equal(new DateOnly(2026, 5, 31), row.SignalDate);
        Assert.Equal("2026-05-31", row.SignalDateText);
        Assert.Equal(SignalType.Buy, row.SignalType);
        Assert.Equal("買い", row.SignalTypeLabel);
        Assert.Equal(105.5d, row.Price);
        Assert.Equal(101.25d, row.Ma75);
        Assert.Equal(99d, row.PrevPrice);
        Assert.Equal(100d, row.PrevMa75);
        Assert.Equal(-1d, row.PrevDiff);
        Assert.Equal(4.25d, row.CurrentDiff);
        Assert.Equal(1200d, row.Avg20Volume);
        Assert.Equal(1.25d, row.VolumeRatio);
        Assert.Equal(0.04197530864197531d, row.Ma75DeviationRate);
        Assert.Equal(80, row.Score);
        Assert.Equal(SignalRank.Strong, row.Rank);
        Assert.Equal("80", row.ScoreText);
        Assert.Equal("Strong", row.RankText);
        Assert.Equal("出来高 20/20", row.ScoreBreakdown);
        Assert.Equal("出来高を伴う上抜け / 強い陽線", row.Reasons);
    }
}
