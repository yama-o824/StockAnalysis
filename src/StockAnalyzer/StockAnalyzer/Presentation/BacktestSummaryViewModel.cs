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

        return new BacktestSummaryViewModel
        {
            SignalCount = result.SignalCount,
            TradeCount = result.TradeCount,
            SkippedSignalCount = result.SkippedSignalCount,
            WinRate = result.WinRate,
            AverageProfitLossRate = result.AverageProfitLossRate,
            AverageWinRate = result.AverageWinRate,
            AverageLossRate = result.AverageLossRate
        };
    }
}
