using StockAnalyzer.Models.Backtest;

namespace StockAnalyzer.Presentation;

public sealed class BacktestSummaryViewModel
{
    public int SignalCount { get; init; }
    public int TradeCount { get; init; }
    public int SkippedSignalCount { get; init; }
    public double WinRate { get; init; }
    public double AverageProfitLossRate { get; init; }
    public double AverageWinRate { get; init; }
    public double AverageLossRate { get; init; }

    public static BacktestSummaryViewModel From(BacktestResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var summary = result.Summary;

        return new BacktestSummaryViewModel
        {
            SignalCount = summary.SignalCount,
            TradeCount = summary.TradeCount,
            SkippedSignalCount = summary.SkippedSignalCount,
            WinRate = summary.WinRate,
            AverageProfitLossRate = summary.AverageProfitLossRate,
            AverageWinRate = summary.AverageWinRate,
            AverageLossRate = summary.AverageLossRate
        };
    }
}
