namespace StockAnalyzer.Models.Backtest;

public sealed class BacktestSummary
{
    public int SignalCount { get; init; }
    public int TradeCount { get; init; }
    public int SkippedSignalCount { get; init; }
    public int WinCount { get; init; }
    public int LossCount { get; init; }
    public int DrawCount { get; init; }
    public double WinRate { get; init; }
    public double TotalProfitLoss { get; init; }
    public double TotalProfitLossRate { get; init; }
    public double AverageProfitLossRate { get; init; }
    public double AverageWinRate { get; init; }
    public double AverageLossRate { get; init; }

    public static BacktestSummary Create(
        int signalCount,
        IReadOnlyList<BacktestTrade> trades)
    {
        ArgumentNullException.ThrowIfNull(trades);

        var tradeCount = trades.Count;
        var winTrades = trades.Where(x => x.IsWin).ToList();
        var lossTrades = trades.Where(x => x.IsLoss).ToList();
        var drawTrades = trades.Where(x => x.IsDraw).ToList();

        return new BacktestSummary
        {
            SignalCount = signalCount,
            TradeCount = tradeCount,
            SkippedSignalCount = signalCount - tradeCount,
            WinCount = winTrades.Count,
            LossCount = lossTrades.Count,
            DrawCount = drawTrades.Count,
            WinRate = tradeCount == 0 ? 0d : (double)winTrades.Count / tradeCount,
            TotalProfitLoss = trades.Sum(x => x.ProfitLoss),
            TotalProfitLossRate = trades.Sum(x => x.ProfitLossRate),
            AverageProfitLossRate = tradeCount == 0 ? 0d : trades.Average(x => x.ProfitLossRate),
            AverageWinRate = winTrades.Count == 0 ? 0d : winTrades.Average(x => x.ProfitLossRate),
            AverageLossRate = lossTrades.Count == 0 ? 0d : lossTrades.Average(x => x.ProfitLossRate)
        };
    }
}
