using StockAnalyzer.Models.Analysis;

namespace StockAnalyzer.Presentation;

public static class SignalScoreTextFormatter
{
    public static string FormatBreakdown(SignalScore? score)
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
