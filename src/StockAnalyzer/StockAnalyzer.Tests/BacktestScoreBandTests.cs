using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Models.Backtest;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class BacktestScoreBandTests
{
    [Fact(DisplayName = "条件なしはスコアありなしを問わず一致する")]
    public void Matches_NoMinimumScore_MatchesAnyTrade()
    {
        var sut = new BacktestScoreBand { Label = "なし" };

        Assert.True(sut.Matches(CreateTrade(80)));
        Assert.True(sut.Matches(CreateTrade(null)));
    }

    [Theory(DisplayName = "閾値付きはスコアが閾値以上の取引だけ一致する")]
    [InlineData(50, 49, false)]
    [InlineData(50, 50, true)]
    [InlineData(75, 74, false)]
    [InlineData(75, 75, true)]
    [InlineData(90, 89, false)]
    [InlineData(90, 90, true)]
    public void Matches_MinimumScore_MatchesScoreAtOrAboveThreshold(
        int minimumScore,
        int score,
        bool expected)
    {
        var sut = new BacktestScoreBand
        {
            Label = $"{minimumScore}以上",
            MinimumScore = minimumScore
        };

        Assert.Equal(expected, sut.Matches(CreateTrade(score)));
    }

    [Fact(DisplayName = "閾値付きはスコアなしの取引に一致しない")]
    public void Matches_MinimumScore_DoesNotMatchTradeWithoutScore()
    {
        var sut = new BacktestScoreBand
        {
            Label = "50以上",
            MinimumScore = 50
        };

        Assert.False(sut.Matches(CreateTrade(null)));
    }

    [Fact(DisplayName = "既定のスコア帯は条件なし、50以上、75以上、90以上の順で返す")]
    public void Defaults_ReturnsScoreBandsInDisplayOrder()
    {
        var defaults = BacktestScoreBand.Defaults;

        Assert.Collection(
            defaults,
            x =>
            {
                Assert.Equal("なし", x.Label);
                Assert.Null(x.MinimumScore);
            },
            x =>
            {
                Assert.Equal("50以上", x.Label);
                Assert.Equal(50, x.MinimumScore);
            },
            x =>
            {
                Assert.Equal("75以上", x.Label);
                Assert.Equal(75, x.MinimumScore);
            },
            x =>
            {
                Assert.Equal("90以上", x.Label);
                Assert.Equal(90, x.MinimumScore);
            });
    }

    private static BacktestTrade CreateTrade(int? score)
    {
        return new BacktestTrade
        {
            SignalScore = score is null
                ? null
                : new SignalScore
                {
                    Total = score.Value,
                    Rank = SignalRank.Normal
                }
        };
    }
}
