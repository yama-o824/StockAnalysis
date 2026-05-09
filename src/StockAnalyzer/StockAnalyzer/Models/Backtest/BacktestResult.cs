namespace StockAnalyzer.Models.Backtest;

public sealed class BacktestResult
{
    public BacktestSettings Settings { get; init; } = default!;
    public IReadOnlyList<BacktestTrade> Trades { get; init; } = [];
    public int SignalCount { get; init; }

    public int TradeCount => Trades.Count;
    public int SkippedSignalCount => SignalCount - TradeCount;
    public int WinCount => Trades.Count(x => x.ProfitLossRate > 0d);
    public int LossCount => Trades.Count(x => x.ProfitLossRate < 0d);
    public int DrawCount => Trades.Count(x => x.ProfitLossRate == 0d);

    public double WinRate => TradeCount == 0
        ? 0d
        : (double)WinCount / TradeCount;

    public double TotalProfitLoss => Trades.Sum(x => x.ProfitLoss);
    public double TotalProfitLossRate => Trades.Sum(x => x.ProfitLossRate);

    public double AverageProfitLossRate => TradeCount == 0
        ? 0d
        : Trades.Average(x => x.ProfitLossRate);

    public double AverageWinRate => WinCount == 0
        ? 0d
        : Trades
            .Where(x => x.ProfitLossRate > 0d)
            .Average(x => x.ProfitLossRate);

    public double AverageLossRate => LossCount == 0
        ? 0d
        : Trades
            .Where(x => x.ProfitLossRate < 0d)
            .Average(x => x.ProfitLossRate);
}
