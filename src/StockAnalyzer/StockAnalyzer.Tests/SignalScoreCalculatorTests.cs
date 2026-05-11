using StockAnalyzer.Analyzer.Signals;
using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Models.Market;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class SignalScoreCalculatorTests
{
    private readonly SignalScoreCalculator _sut = new();

    [Fact(DisplayName = "Buyシグナルは評価事実から100点満点で採点する")]
    public void Calculate_BuySignal_ReturnsScoreAndBreakdowns()
    {
        var candidate = CreateCandidate(SignalType.Buy);
        var evaluation = new SignalEvaluation
        {
            Ma75DeviationRate = 0.06d,
            HasVolumeSupport = true,
            IsPullbackBounce = true,
            HasStrongBullishCandle = true
        };

        var score = _sut.Calculate(candidate, evaluation);

        Assert.NotNull(score);
        Assert.Equal(100, score.Total);
        Assert.Equal(SignalRank.VeryStrong, score.Rank);
        Assert.Collection(
            score.Breakdowns,
            x =>
            {
                Assert.Equal("Ma75Deviation", x.Key);
                Assert.Equal(30, x.Points);
                Assert.Equal(30, x.MaxPoints);
            },
            x =>
            {
                Assert.Equal("VolumeSupport", x.Key);
                Assert.Equal(25, x.Points);
                Assert.Equal(25, x.MaxPoints);
            },
            x =>
            {
                Assert.Equal("PullbackBounce", x.Key);
                Assert.Equal(25, x.Points);
                Assert.Equal(25, x.MaxPoints);
            },
            x =>
            {
                Assert.Equal("StrongBullishCandle", x.Key);
                Assert.Equal(20, x.Points);
                Assert.Equal(20, x.MaxPoints);
            });
    }

    [Fact(DisplayName = "Sellシグナルは採点対象外にする")]
    public void Calculate_SellSignal_ReturnsNull()
    {
        var candidate = CreateCandidate(SignalType.Sell);
        var evaluation = new SignalEvaluation
        {
            Ma75DeviationRate = 0.06d
        };

        var score = _sut.Calculate(candidate, evaluation);

        Assert.Null(score);
    }

    [Theory(DisplayName = "合計点からランクを算出する")]
    [InlineData(null, false, false, false, 0, SignalRank.None)]
    [InlineData(0.01d, false, false, false, 15, SignalRank.Weak)]
    [InlineData(0.06d, true, false, false, 55, SignalRank.Normal)]
    [InlineData(0.06d, true, false, true, 75, SignalRank.Strong)]
    [InlineData(0.06d, true, true, false, 80, SignalRank.Strong)]
    [InlineData(0.10d, true, false, true, 65, SignalRank.Normal)]
    public void Calculate_BuySignal_ReturnsRankByTotal(
        double? ma75DeviationRate,
        bool hasVolumeSupport,
        bool isPullbackBounce,
        bool hasStrongBullishCandle,
        int expectedTotal,
        SignalRank expectedRank)
    {
        var candidate = CreateCandidate(SignalType.Buy);
        var evaluation = new SignalEvaluation
        {
            Ma75DeviationRate = ma75DeviationRate,
            HasVolumeSupport = hasVolumeSupport,
            IsPullbackBounce = isPullbackBounce,
            HasStrongBullishCandle = hasStrongBullishCandle
        };

        var score = _sut.Calculate(candidate, evaluation);

        Assert.NotNull(score);
        Assert.Equal(expectedTotal, score.Total);
        Assert.Equal(expectedRank, score.Rank);
    }

    private static SignalCandidate CreateCandidate(SignalType type)
    {
        return new SignalCandidate
        {
            Type = type,
            Previous = CreateBar(new DateOnly(2024, 1, 1)),
            Current = CreateBar(new DateOnly(2024, 1, 2))
        };
    }

    private static AnalysisBar CreateBar(DateOnly date)
    {
        return new AnalysisBar
        {
            Raw = new PriceBar
            {
                Date = date,
                Open = 100d,
                High = 105d,
                Low = 99d,
                Close = 104d,
                Volume = 1000
            },
            Candle = new CandleMetrics()
        };
    }
}
