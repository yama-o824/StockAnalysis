using StockAnalyzer.Models;
using StockAnalyzer.Models.Backtest;

namespace StockAnalyzer.Presentation;

public sealed class BacktestViewRow
{
    public string SignalDate { get; init; } = string.Empty;
    public SignalType SignalType { get; init; }
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
            SignalType = trade.Signal.Candidate.Type,
            EntryDate = trade.EntryDate.ToString("yyyy-MM-dd"),
            EntryPrice = trade.EntryPrice,
            ExitDate = trade.ExitDate.ToString("yyyy-MM-dd"),
            ExitPrice = trade.ExitPrice,
            ProfitLoss = trade.ProfitLoss,
            ProfitLossRate = trade.ProfitLossRate,
            Reasons = string.Join(" / ", trade.Signal.Evaluation.Reasons)
        };
    }
}
