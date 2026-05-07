namespace StockAnalyzer.Models.Backtest;

public enum BacktestSignalType
{
    Buy
}

public enum EntryTiming
{
    NextBusinessDayOpen
}

public enum ExitTiming
{
    NBusinessDaysLaterClose
}

public sealed class BacktestSettings
{
    public BacktestSignalType SignalType { get; init; } = BacktestSignalType.Buy;
    public EntryTiming EntryRule { get; init; } = EntryTiming.NextBusinessDayOpen;
    public ExitTiming ExitRule { get; init; } = ExitTiming.NBusinessDaysLaterClose;
    public int ExitAfterBusinessDays { get; init; } = 5;
}
