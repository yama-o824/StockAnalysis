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
        Assert.Equal("66.7%", row.WinRateText);
        Assert.Equal("+12.0%", row.AverageProfitLossRateText);
        Assert.Equal("+20.0%", row.AverageWinRateText);
        Assert.Equal("-4.0%", row.AverageLossRateText);
    }

    [Fact(DisplayName = "対象取引数が0件の率はハイフンで表示する")]
    public void From_TradeCountIsZero_FormatsRatesAsEmpty()
    {
        var summary = new BacktestScoreBandSummary
        {
            ScoreBand = new BacktestScoreBand
            {
                Label = "90以上",
                MinimumScore = 90
            },
            TradeCount = 0,
            WinRate = 0d,
            AverageProfitLossRate = 0d,
            AverageWinRate = 0d,
            AverageLossRate = 0d
        };

        var row = BacktestScoreBandSummaryViewRow.From(summary);

        Assert.Equal("90以上", row.ScoreBand);
        Assert.Equal(0, row.TradeCount);
        Assert.Equal("-", row.WinRateText);
        Assert.Equal("-", row.AverageProfitLossRateText);
        Assert.Equal("-", row.AverageWinRateText);
        Assert.Equal("-", row.AverageLossRateText);
    }
}
