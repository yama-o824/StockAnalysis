namespace StockAnalyzer.Models.Backtest;

public sealed class BacktestScoreBand
{
    public string Label { get; init; } = string.Empty;
    public int? MinimumScore { get; init; }

    public static IReadOnlyList<BacktestScoreBand> Defaults { get; } =
    [
        new BacktestScoreBand { Label = "90以上", MinimumScore = 90 },
        new BacktestScoreBand { Label = "75以上", MinimumScore = 75 },
        new BacktestScoreBand { Label = "50以上", MinimumScore = 50 },
        new BacktestScoreBand { Label = "なし" }
    ];

    public bool Matches(BacktestTrade trade)
    {
        ArgumentNullException.ThrowIfNull(trade);

        return MinimumScore is null || trade.SignalScore?.Total >= MinimumScore;
    }
}
