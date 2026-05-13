namespace StockAnalyzer.Models.Backtest;

public sealed class BacktestScoreBandSummary
{
    public BacktestScoreBand ScoreBand { get; init; } = default!;
    public int TradeCount { get; init; }
    public double WinRate { get; init; }
    public double AverageProfitLossRate { get; init; }
    public double AverageWinRate { get; init; }
    public double AverageLossRate { get; init; }
}
