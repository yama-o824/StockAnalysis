namespace StockAnalyzer.Models.Backtest;

public sealed class BacktestResult
{
    public BacktestSettings Settings { get; init; } = default!;
    public IReadOnlyList<BacktestTrade> Trades { get; init; } = [];

    public int TradeCount => Trades.Count;
    public int WinCount => Trades.Count(x => x.ProfitLoss > 0d);
    public int LossCount => Trades.Count(x => x.ProfitLoss < 0d);
    public int DrawCount => Trades.Count(x => x.ProfitLoss == 0d);

    public double WinRate => TradeCount == 0
        ? 0d
        : (double)WinCount / TradeCount;

    public double TotalProfitLoss => Trades.Sum(x => x.ProfitLoss);

    public double AverageProfitLossRate => TradeCount == 0
        ? 0d
        : Trades.Average(x => x.ProfitLossRate);
}
