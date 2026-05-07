namespace StockAnalyzer.Models.Backtest;

public sealed class BacktestResult
{
    public IReadOnlyList<BacktestTrade> Trades { get; init; } = [];

    public int TotalSignals { get; init; }
    public int FilledTrades { get; init; }
    public int SkippedTrades { get; init; }

    public double? WinRate { get; init; }
    public double? AverageProfitLossRate { get; init; }
    public double? CumulativeProfitLossRate { get; init; }

    public DateOnly? FirstSignalDate { get; init; }
    public DateOnly? LastSignalDate { get; init; }
}
