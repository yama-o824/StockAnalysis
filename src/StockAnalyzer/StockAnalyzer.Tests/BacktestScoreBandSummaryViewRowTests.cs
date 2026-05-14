using StockAnalyzer.Models.Backtest;
using StockAnalyzer.Presentation;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class BacktestScoreBandSummaryViewRowTests
{
    [Fact(DisplayName = "スコア帯別集計を表示行に変換する")]
    public void From_MapsSummaryToViewRow()
    {
        var summary = new BacktestScoreBandSummary
        {
            ScoreBand = new BacktestScoreBand
            {
                Label = "75以上",
                MinimumScore = 75
            },
            TradeCount = 3,
            WinRate = 2d / 3d,
            AverageProfitLossRate = 0.12d,
            AverageWinRate = 0.20d,
            AverageLossRate = -0.04d
        };

        var row = BacktestScoreBandSummaryViewRow.From(summary);

        Assert.Equal("75以上", row.ScoreBand);
        Assert.Equal(3, row.TradeCount);
        Assert.Equal(2d / 3d, row.WinRate);
        Assert.Equal(0.12d, row.AverageProfitLossRate);
        Assert.Equal(0.20d, row.AverageWinRate);
        Assert.Equal(-0.04d, row.AverageLossRate);
    }
}
