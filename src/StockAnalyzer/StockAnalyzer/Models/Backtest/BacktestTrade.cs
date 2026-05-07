namespace StockAnalyzer.Models.Backtest;

public enum BacktestTradeStatus
{
    Filled,
    SkippedInsufficientBars
}

public enum ExitReason
{
    FixedHoldingPeriod,
    NotExitedInsufficientBars
}

public sealed class BacktestTrade
{
    public DateOnly SignalDate { get; init; }

    public DateOnly? EntryDate { get; init; }
    public double? EntryPrice { get; init; }

    public DateOnly? ExitDate { get; init; }
    public double? ExitPrice { get; init; }

    public int HoldingBusinessDays { get; init; }

    public double? ProfitLoss { get; init; }
    public double? ProfitLossRate { get; init; }

    public BacktestTradeStatus Status { get; init; }
    public ExitReason ExitReason { get; init; }
}
