using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Presentation;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class SignalScoreTextFormatterTests
{
    [Fact(DisplayName = "スコアがnullの場合は空文字を返す")]
    public void FormatBreakdown_NullScore_ReturnsEmpty()
    {
        var text = SignalScoreTextFormatter.FormatBreakdown(null);

        Assert.Equal(string.Empty, text);
    }

    [Fact(DisplayName = "スコア内訳はラベルと配点を区切って表示する")]
    public void FormatBreakdown_ReturnsJoinedBreakdownText()
    {
        var score = new SignalScore
        {
            Breakdowns =
            [
                new SignalScoreBreakdown
                {
                    Label = "MA75乖離",
                    Points = 20,
                    MaxPoints = 30
                },
                new SignalScoreBreakdown
                {
                    Label = "出来高",
                    Points = 25,
                    MaxPoints = 25
                }
            ]
        };

        var text = SignalScoreTextFormatter.FormatBreakdown(score);

        Assert.Equal("MA75乖離 20/30 / 出来高 25/25", text);
    }
}
