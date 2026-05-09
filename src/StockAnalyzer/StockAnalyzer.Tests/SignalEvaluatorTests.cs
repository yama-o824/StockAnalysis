using StockAnalyzer.Analyzer.Signals;
using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Models.Market;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class SignalEvaluatorTests
{
    private readonly SignalEvaluator _sut = new();

    [Fact(DisplayName = "BuyシグナルはMA75乖離率と買い評価フラグを算出する")]
    public void Evaluate_BuySignal_ReturnsCurrentEvaluation()
    {
        var candidate = CreateCandidate(
            SignalType.Buy,
            previousClose: 99d,
            previousMa75: 100d,
            currentOpen: 100d,
            currentHigh: 108d,
            currentLow: 99d,
            currentClose: 106d,
            currentMa75: 100d,
            volumeRatio: 1.5d);

        var result = _sut.Evaluate(candidate);

        Assert.Same(candidate, result.Candidate);
        Assert.Equal(0.06d, result.Evaluation.Ma75DeviationRate, precision: 10);
        Assert.True(result.Evaluation.HasVolumeSupport);
        Assert.True(result.Evaluation.HasStrongBullishCandle);
        Assert.True(result.Evaluation.IsPullbackBounce);
        Assert.Equal(
            [
                "Ma75DeviationRate=6.00%",
                "VolumeRatio=1.50",
                "出来高を伴う上抜け",
                "強い陽線",
                "MA75押し目反発"
            ],
            result.Evaluation.Reasons);
    }

    [Fact(DisplayName = "Sellシグナルは乖離率を売り方向の強度として正値にする")]
    public void Evaluate_SellSignal_ReturnsPositiveSellStrength()
    {
        var candidate = CreateCandidate(
            SignalType.Sell,
            previousClose: 101d,
            previousMa75: 100d,
            currentOpen: 100d,
            currentHigh: 101d,
            currentLow: 93d,
            currentClose: 94d,
            currentMa75: 100d,
            volumeRatio: null);

        var result = _sut.Evaluate(candidate);

        Assert.Equal(0.06d, result.Evaluation.Ma75DeviationRate, precision: 10);
        Assert.False(result.Evaluation.HasVolumeSupport);
        Assert.False(result.Evaluation.HasStrongBullishCandle);
        Assert.False(result.Evaluation.IsPullbackBounce);
        Assert.Equal(["Ma75DeviationRate=6.00%"], result.Evaluation.Reasons);
    }

    [Fact(DisplayName = "追加評価がない場合はクロスのみを根拠にする")]
    public void Evaluate_NoAdditionalEvaluation_ReturnsCrossOnlyReason()
    {
        var candidate = CreateCandidate(
            SignalType.Buy,
            previousClose: 99d,
            previousMa75: 100d,
            currentOpen: 100d,
            currentHigh: 101d,
            currentLow: 99d,
            currentClose: 100d,
            currentMa75: null,
            volumeRatio: null);

        var result = _sut.Evaluate(candidate);

        Assert.Null(result.Evaluation.Ma75DeviationRate);
        Assert.False(result.Evaluation.HasVolumeSupport);
        Assert.False(result.Evaluation.HasStrongBullishCandle);
        Assert.False(result.Evaluation.IsPullbackBounce);
        Assert.Equal(["クロスのみ"], result.Evaluation.Reasons);
    }

    private static SignalCandidate CreateCandidate(
        SignalType type,
        double previousClose,
        double? previousMa75,
        double currentOpen,
        double currentHigh,
        double currentLow,
        double currentClose,
        double? currentMa75,
        double? volumeRatio)
    {
        return new SignalCandidate
        {
            Type = type,
            Previous = new AnalysisBar
            {
                Raw = new PriceBar
                {
                    Date = new DateOnly(2024, 1, 1),
                    Open = previousClose,
                    High = previousClose,
                    Low = previousClose,
                    Close = previousClose,
                    Volume = 1000
                },
                Ma75 = previousMa75,
                Candle = new CandleMetrics()
            },
            Current = new AnalysisBar
            {
                Raw = new PriceBar
                {
                    Date = new DateOnly(2024, 1, 2),
                    Open = currentOpen,
                    High = currentHigh,
                    Low = currentLow,
                    Close = currentClose,
                    Volume = 1500
                },
                Ma75 = currentMa75,
                VolumeRatio = volumeRatio,
                Candle = CreateCandle(currentOpen, currentHigh, currentLow, currentClose)
            }
        };
    }

    private static CandleMetrics CreateCandle(
        double open,
        double high,
        double low,
        double close)
    {
        var range = Math.Max(high - low, 0d);
        var bodySize = Math.Abs(close - open);
        var upperShadow = Math.Max(high - Math.Max(open, close), 0d);

        return new CandleMetrics
        {
            Range = range,
            BodySize = bodySize,
            BodyRate = range > 0 ? bodySize / range : 0d,
            UpperShadowRate = range > 0 ? upperShadow / range : 0d,
            ClosePositionRate = range > 0 ? (close - low) / range : 0d,
            IsBullish = close >= open
        };
    }
}
