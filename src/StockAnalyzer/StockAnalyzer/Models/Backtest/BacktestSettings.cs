using StockAnalyzer.Models;

namespace StockAnalyzer.Models.Backtest;

public sealed class BacktestSettings
{
    public SignalType TargetSignalType { get; init; } = SignalType.Buy;
    public int EntryDelayBars { get; init; } = 1;
    public int ExitAfterBars { get; init; } = 5;
}
