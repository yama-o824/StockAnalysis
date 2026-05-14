using StockAnalyzer.Models.Backtest;

namespace StockAnalyzer.Presentation;

public sealed class BacktestScoreBandSummaryViewRow
{
    public string ScoreBand { get; init; } = string.Empty;
    public int TradeCount { get; init; }
    public string WinRateText { get; init; } = string.Empty;
    public string AverageProfitLossRateText { get; init; } = string.Empty;
    public string AverageWinRateText { get; init; } = string.Empty;
    public string AverageLossRateText { get; init; } = string.Empty;

    public static BacktestScoreBandSummaryViewRow From(BacktestScoreBandSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return new BacktestScoreBandSummaryViewRow
        {
            ScoreBand = summary.ScoreBand.Label,
            TradeCount = summary.TradeCount,
            WinRateText = FormatPercentOrEmpty(summary.TradeCount, summary.WinRate, includePositiveSign: false),
            AverageProfitLossRateText = FormatPercentOrEmpty(summary.TradeCount, summary.AverageProfitLossRate, includePositiveSign: true),
            AverageWinRateText = FormatPercentOrEmpty(summary.TradeCount, summary.AverageWinRate, includePositiveSign: true),
            AverageLossRateText = FormatPercentOrEmpty(summary.TradeCount, summary.AverageLossRate, includePositiveSign: true)
        };
    }

    private static string FormatPercentOrEmpty(
        int tradeCount,
        double rate,
        bool includePositiveSign)
    {
        if (tradeCount == 0)
        {
            return "-";
        }

        return includePositiveSign
            ? rate.ToString("+#,0.0%;-#,0.0%;0.0%")
            : rate.ToString("0.0%");
    }
}
