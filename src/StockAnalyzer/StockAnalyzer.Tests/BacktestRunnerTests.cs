using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Models.Backtest;
using StockAnalyzer.Models.Market;
using StockAnalyzer.Services.Backtest;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class BacktestRunnerTests
{
    private readonly BacktestRunner _sut = new();

    [Fact(DisplayName = "Buyシグナルは翌営業日始値でエントリーし、5営業日後終値で決済する")]
    public void Run_BuySignal_UsesNextBarOpenAndFifthBarClose()
    {
        var bars = CreateBars(8);
        var signal = CreateSignalResult(SignalType.Buy, bars[0], bars[1]);
        var analysisResult = new AnalysisResult
        {
            Bars = bars,
            Signals = [signal]
        };

        var result = _sut.Run(analysisResult, new BacktestSettings());

        var trade = Assert.Single(result.Trades);
        Assert.Equal(new DateOnly(2024, 1, 2), trade.SignalDate);
        Assert.Equal(new DateOnly(2024, 1, 3), trade.EntryDate);
        Assert.Equal(102d, trade.EntryPrice);
        Assert.Equal(new DateOnly(2024, 1, 8), trade.ExitDate);
        Assert.Equal(107.5d, trade.ExitPrice);
        Assert.Equal(1, result.SignalCount);
        Assert.Equal(0, result.SkippedSignalCount);
    }

    [Fact(DisplayName = "対象がBuy設定のとき、Sellシグナルはバックテスト対象外にする")]
    public void Run_TargetSignalTypeIsBuy_SkipsSellSignal()
    {
        var bars = CreateBars(8);
        var buySignal = CreateSignalResult(SignalType.Buy, bars[0], bars[1]);
        var sellSignal = CreateSignalResult(SignalType.Sell, bars[1], bars[2]);
        var analysisResult = new AnalysisResult
        {
            Bars = bars,
            Signals = [buySignal, sellSignal]
        };

        var result = _sut.Run(analysisResult, new BacktestSettings());

        var trade = Assert.Single(result.Trades);
        Assert.Equal(SignalType.Buy, trade.Signal.Candidate.Type);
        Assert.Equal(1, result.SignalCount);
    }

    [Fact(DisplayName = "決済に必要な営業日が足りないシグナルはスキップする")]
    public void Run_NotEnoughBarsForExit_SkipsTrade()
    {
        var bars = CreateBars(7);
        var signal = CreateSignalResult(SignalType.Buy, bars[0], bars[1]);
        var analysisResult = new AnalysisResult
        {
            Bars = bars,
            Signals = [signal]
        };

        var result = _sut.Run(analysisResult, new BacktestSettings());

        Assert.Empty(result.Trades);
        Assert.Equal(1, result.SignalCount);
        Assert.Equal(1, result.SkippedSignalCount);
    }

    [Theory(DisplayName = "不正なBacktestSettingsは例外にする")]
    [InlineData(0, 5)]
    [InlineData(1, 0)]
    public void Run_InvalidSettings_ThrowsArgumentOutOfRangeException(
        int entryDelayBars,
        int exitAfterBars)
    {
        var analysisResult = new AnalysisResult
        {
            Bars = CreateBars(8),
            Signals = []
        };

        var settings = new BacktestSettings
        {
            EntryDelayBars = entryDelayBars,
            ExitAfterBars = exitAfterBars
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Run(analysisResult, settings));
    }

    private static IReadOnlyList<AnalysisBar> CreateBars(int count)
    {
        var startDate = new DateOnly(2024, 1, 1);
        var bars = new List<AnalysisBar>(count);

        for (int i = 0; i < count; i++)
        {
            var open = 100d + i;

            bars.Add(new AnalysisBar
            {
                Raw = new PriceBar
                {
                    Date = startDate.AddDays(i),
                    Open = open,
                    High = open + 2d,
                    Low = open - 1d,
                    Close = open + 0.5d,
                    Volume = 1000 + i
                },
                Candle = new CandleMetrics
                {
                    Range = 3d,
                    BodySize = 0.5d,
                    BodyRate = 0.2d,
                    UpperShadowRate = 0.2d,
                    ClosePositionRate = 0.7d,
                    IsBullish = true
                }
            });
        }

        return bars;
    }

    private static SignalResult CreateSignalResult(
        SignalType type,
        AnalysisBar previous,
        AnalysisBar current)
    {
        return new SignalResult
        {
            Candidate = new SignalCandidate
            {
                Type = type,
                Previous = previous,
                Current = current
            },
            Evaluation = new SignalEvaluation
            {
                Reasons = ["test"]
            }
        };
    }
}
