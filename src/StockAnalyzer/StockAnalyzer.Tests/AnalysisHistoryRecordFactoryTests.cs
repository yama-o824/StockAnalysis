using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Models.Market;
using StockAnalyzer.Services;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class AnalysisHistoryRecordFactoryTests
{
    private readonly AnalysisHistoryRecordFactory _sut = new();

    [Fact(DisplayName = "分析結果から保存用履歴レコードを作成する")]
    public void Create_ReturnsHistoryRecordsFromSignals()
    {
        var executedAt = new DateTimeOffset(2026, 6, 1, 9, 30, 0, TimeSpan.FromHours(9));
        var analysisResult = new AnalysisResult
        {
            Bars =
            [
                CreateAnalysisBar(new DateOnly(2026, 1, 1), close: 95d, ma75: 100d),
                CreateAnalysisBar(new DateOnly(2026, 1, 2), close: 105d, ma75: 101d, avg20Volume: 1200d, volumeRatio: 1.25d)
            ],
            Signals =
            [
                new SignalResult
                {
                    Candidate = new SignalCandidate
                    {
                        Type = SignalType.Buy,
                        Previous = CreateAnalysisBar(new DateOnly(2026, 1, 1), close: 95d, ma75: 100d),
                        Current = CreateAnalysisBar(new DateOnly(2026, 1, 2), close: 105d, ma75: 101d, avg20Volume: 1200d, volumeRatio: 1.25d)
                    },
                    Evaluation = new SignalEvaluation
                    {
                        Ma75DeviationRate = 0.0396039604d,
                        Reasons = ["出来高を伴う上抜け", "強い陽線"]
                    },
                    Score = new SignalScore
                    {
                        Total = 80,
                        Rank = SignalRank.Strong,
                        Breakdowns =
                        [
                            new SignalScoreBreakdown
                            {
                                Label = "出来高",
                                Points = 20,
                                MaxPoints = 20
                            }
                        ]
                    }
                }
            ]
        };

        var records = _sut.Create("7203.T", "1y", executedAt, "run-1", analysisResult);

        var record = Assert.Single(records);
        Assert.Equal(1, record.SchemaVersion);
        Assert.Equal("run-1", record.RunId);
        Assert.Equal("7203.T", record.Symbol);
        Assert.Equal(executedAt, record.ExecutedAt);
        Assert.Equal("1y", record.RequestedPeriod);
        Assert.Equal(new DateOnly(2026, 1, 1), record.AnalysisStartDate);
        Assert.Equal(new DateOnly(2026, 1, 2), record.AnalysisEndDate);
        Assert.Equal(new DateOnly(2026, 1, 2), record.SignalDate);
        Assert.Equal(SignalType.Buy, record.SignalType);
        Assert.Equal(105d, record.Price);
        Assert.Equal(101d, record.Ma75);
        Assert.Equal(95d, record.PrevPrice);
        Assert.Equal(100d, record.PrevMa75);
        Assert.Equal(-5d, record.PrevDiff);
        Assert.Equal(4d, record.CurrentDiff);
        Assert.Equal(1200d, record.Avg20Volume);
        Assert.Equal(1.25d, record.VolumeRatio);
        Assert.Equal(0.0396039604d, record.Ma75DeviationRate);
        Assert.Equal(80, record.Score);
        Assert.Equal(SignalRank.Strong, record.Rank);
        Assert.Equal("出来高 20/20", record.ScoreBreakdown);
        Assert.Equal("出来高を伴う上抜け / 強い陽線", record.Reasons);
    }

    [Fact(DisplayName = "価格データがない分析結果は履歴レコードを作らない")]
    public void Create_WithoutBars_ReturnsEmptyRecords()
    {
        var records = _sut.Create(
            "7203.T",
            "1y",
            DateTimeOffset.Now,
            "run-1",
            new AnalysisResult());

        Assert.Empty(records);
    }

    private static AnalysisBar CreateAnalysisBar(
        DateOnly date,
        double close,
        double? ma75,
        double? avg20Volume = null,
        double? volumeRatio = null)
    {
        return new AnalysisBar
        {
            Raw = new PriceBar
            {
                Date = date,
                Open = close,
                High = close,
                Low = close,
                Close = close,
                Volume = 1000
            },
            Ma75 = ma75,
            Avg20Volume = avg20Volume,
            VolumeRatio = volumeRatio,
            Candle = new CandleMetrics()
        };
    }
}
