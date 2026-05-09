using StockAnalyzer.Models.Analysis;

namespace StockAnalyzer.Models.Backtest;

public sealed class BacktestTrade
{
    public SignalResult Signal { get; init; } = default!;

    public DateOnly SignalDate { get; init; }
    public DateOnly EntryDate { get; init; }
    public double EntryPrice { get; init; }
    public DateOnly ExitDate { get; init; }
    public double ExitPrice { get; init; }

    public double ProfitLoss => ExitPrice - EntryPrice;
    public double ProfitLossRate => EntryPrice == 0d
        ? 0d
        : (ExitPrice - EntryPrice) / EntryPrice;
}
