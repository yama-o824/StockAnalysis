namespace StockAnalyzer.Models.Backtest;

public sealed class BacktestResult
{
    public BacktestSettings Settings { get; init; } = default!;
    public IReadOnlyList<BacktestTrade> Trades { get; init; } = [];
    public BacktestSummary Summary { get; init; } = default!;
}
