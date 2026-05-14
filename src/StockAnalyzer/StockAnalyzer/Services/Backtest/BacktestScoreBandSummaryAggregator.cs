using StockAnalyzer.Models.Backtest;

namespace StockAnalyzer.Services.Backtest;

public sealed class BacktestScoreBandSummaryAggregator
{
    public IReadOnlyList<BacktestScoreBandSummary> Create(BacktestResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return BacktestScoreBand.Defaults
            .Select(scoreBand => CreateSummary(scoreBand, result.Trades))
            .ToList();
    }

    private static BacktestScoreBandSummary CreateSummary(
        BacktestScoreBand scoreBand,
        IReadOnlyList<BacktestTrade> trades)
    {
        var targetTrades = trades
            .Where(scoreBand.Matches)
            .ToList();

        var tradeCount = targetTrades.Count;
        var winTrades = targetTrades.Where(x => x.IsWin).ToList();
        var lossTrades = targetTrades.Where(x => x.IsLoss).ToList();

        return new BacktestScoreBandSummary
        {
            ScoreBand = scoreBand,
            TradeCount = tradeCount,
            WinRate = tradeCount == 0 ? 0d : (double)winTrades.Count / tradeCount,
            AverageProfitLossRate = tradeCount == 0 ? 0d : targetTrades.Average(x => x.ProfitLossRate),
            AverageWinRate = winTrades.Count == 0 ? 0d : winTrades.Average(x => x.ProfitLossRate),
            AverageLossRate = lossTrades.Count == 0 ? 0d : lossTrades.Average(x => x.ProfitLossRate)
        };
    }
}
