using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Models.Backtest;

namespace StockAnalyzer.Presentation;

public sealed class BacktestViewRow
{
    public string SignalDate { get; init; } = string.Empty;
    public SignalType SignalType { get; init; }
    public int? Score { get; init; }
    public SignalRank? Rank { get; init; }
    public string ScoreBreakdown { get; init; } = string.Empty;
    public string EntryDate { get; init; } = string.Empty;
    public double EntryPrice { get; init; }
    public string ExitDate { get; init; } = string.Empty;
    public double ExitPrice { get; init; }
    public double ProfitLoss { get; init; }
    public double ProfitLossRate { get; init; }
    public string Reasons { get; init; } = string.Empty;

    public static BacktestViewRow From(BacktestTrade trade)
    {
        ArgumentNullException.ThrowIfNull(trade);

        return new BacktestViewRow
        {
            SignalDate = trade.SignalDate.ToString("yyyy-MM-dd"),
            SignalType = trade.SignalType,
            Score = trade.SignalScore?.Total,
            Rank = trade.SignalScore?.Rank,
            ScoreBreakdown = FormatScoreBreakdown(trade.SignalScore),
            EntryDate = trade.EntryDate.ToString("yyyy-MM-dd"),
            EntryPrice = trade.EntryPrice,
            ExitDate = trade.ExitDate.ToString("yyyy-MM-dd"),
            ExitPrice = trade.ExitPrice,
            ProfitLoss = trade.ProfitLoss,
            ProfitLossRate = trade.ProfitLossRate,
            Reasons = string.Join(" / ", trade.Reasons)
        };
    }

    private static string FormatScoreBreakdown(SignalScore? score)
    {
        if (score is null)
        {
            return string.Empty;
        }

        return string.Join(
            " / ",
            score.Breakdowns.Select(x => $"{x.Label} {x.Points}/{x.MaxPoints}"));
    }
}
