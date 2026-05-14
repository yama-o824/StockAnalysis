using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Models.Backtest;
using StockAnalyzer.Services.Backtest;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class BacktestScoreBandSummaryAggregatorTests
{
    private readonly BacktestScoreBandSummaryAggregator _sut = new();

    [Fact(DisplayName = "BacktestResultから既定スコア帯ごとの集計を作る")]
    public void Create_CalculatesSummariesForDefaultScoreBands()
    {
        var result = new BacktestResult
        {
            Trades =
            [
                CreateTrade(null, 0.10d),
                CreateTrade(49, -0.05d),
                CreateTrade(50, 0.20d),
                CreateTrade(75, -0.10d),
                CreateTrade(90, 0.30d)
            ]
        };

        var summaries = _sut.Create(result);

        Assert.Collection(
            summaries,
            x => AssertSummary(x, "90以上", 1, 1.00d, 0.30d, 0.30d, 0d),
            x => AssertSummary(x, "75以上", 2, 0.50d, 0.10d, 0.30d, -0.10d),
            x => AssertSummary(x, "50以上", 3, 2d / 3d, 0.13333333333333333d, 0.25d, -0.10d),
            x => AssertSummary(x, "なし", 5, 0.60d, 0.09d, 0.20d, -0.075d));
    }

    [Fact(DisplayName = "該当取引がないスコア帯は0で集計する")]
    public void Create_NoMatchingTrades_ReturnsZeroSummary()
    {
        var result = new BacktestResult
        {
            Trades =
            [
                CreateTrade(null, 0.10d),
                CreateTrade(49, -0.05d)
            ]
        };

        var summaries = _sut.Create(result);

        var summary = Assert.Single(summaries.Where(x => x.ScoreBand.Label == "90以上"));
        AssertSummary(summary, "90以上", 0, 0d, 0d, 0d, 0d);
    }

    private static void AssertSummary(
        BacktestScoreBandSummary actual,
        string scoreBandLabel,
        int tradeCount,
        double winRate,
        double averageProfitLossRate,
        double averageWinRate,
        double averageLossRate)
    {
        Assert.Equal(scoreBandLabel, actual.ScoreBand.Label);
        Assert.Equal(tradeCount, actual.TradeCount);
        Assert.Equal(winRate, actual.WinRate, 10);
        Assert.Equal(averageProfitLossRate, actual.AverageProfitLossRate, 10);
        Assert.Equal(averageWinRate, actual.AverageWinRate, 10);
        Assert.Equal(averageLossRate, actual.AverageLossRate, 10);
    }

    private static BacktestTrade CreateTrade(int? score, double profitLossRate)
    {
        const double entryPrice = 100d;

        return new BacktestTrade
        {
            SignalScore = score is null
                ? null
                : new SignalScore
                {
                    Total = score.Value,
                    Rank = SignalRank.Normal
                },
            EntryPrice = entryPrice,
            ExitPrice = entryPrice * (1d + profitLossRate)
        };
    }
}
