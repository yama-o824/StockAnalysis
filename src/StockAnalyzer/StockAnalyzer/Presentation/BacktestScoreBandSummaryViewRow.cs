using StockAnalyzer.Models.Backtest;

namespace StockAnalyzer.Presentation;

public sealed class BacktestScoreBandSummaryViewRow
{
    public string ScoreBand { get; init; } = string.Empty;
    public int TradeCount { get; init; }
    public double WinRate { get; init; }
    public double AverageProfitLossRate { get; init; }
    public double AverageWinRate { get; init; }
    public double AverageLossRate { get; init; }

    public static BacktestScoreBandSummaryViewRow From(BacktestScoreBandSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return new BacktestScoreBandSummaryViewRow
        {
            ScoreBand = summary.ScoreBand.Label,
            TradeCount = summary.TradeCount,
            WinRate = summary.WinRate,
            AverageProfitLossRate = summary.AverageProfitLossRate,
            AverageWinRate = summary.AverageWinRate,
            AverageLossRate = summary.AverageLossRate
        };
    }
}
