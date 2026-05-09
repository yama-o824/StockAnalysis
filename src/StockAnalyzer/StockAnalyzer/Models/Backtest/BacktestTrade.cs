using StockAnalyzer.Models;

namespace StockAnalyzer.Models.Backtest;

public sealed class BacktestTrade
{
    public DateOnly SignalDate { get; init; }
    public SignalType SignalType { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];
    public DateOnly EntryDate { get; init; }
    public double EntryPrice { get; init; }
    public DateOnly ExitDate { get; init; }
    public double ExitPrice { get; init; }
    public int HoldingBars { get; init; }

    public double ProfitLoss => ExitPrice - EntryPrice;
    public double ProfitLossRate => EntryPrice == 0d
        ? 0d
        : (ExitPrice - EntryPrice) / EntryPrice;
    public bool IsWin => ProfitLossRate > 0d;
    public bool IsLoss => ProfitLossRate < 0d;
    public bool IsDraw => !IsWin && !IsLoss;
}
