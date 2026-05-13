using StockAnalyzer.Presentation;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class ScoreFilterTests
{
    [Fact(DisplayName = "最小スコアなしならスコア未設定も含める")]
    public void Matches_WithoutMinimumScore_IncludesNullScore()
    {
        var result = ScoreFilter.Matches(null, null);

        Assert.True(result);
    }

    [Theory(DisplayName = "最小スコア以上なら表示対象にする")]
    [InlineData(50, 50)]
    [InlineData(75, 50)]
    [InlineData(90, 75)]
    public void Matches_WhenScoreIsGreaterThanOrEqualToMinimum_ReturnsTrue(int score, int minimumScore)
    {
        var result = ScoreFilter.Matches(score, minimumScore);

        Assert.True(result);
    }

    [Theory(DisplayName = "最小スコア未満またはスコア未設定なら除外する")]
    [InlineData(49, 50)]
    [InlineData(74, 75)]
    [InlineData(89, 90)]
    public void Matches_WhenScoreIsBelowMinimum_ReturnsFalse(int score, int minimumScore)
    {
        var result = ScoreFilter.Matches(score, minimumScore);

        Assert.False(result);
    }

    [Fact(DisplayName = "最小スコアありならスコア未設定は除外する")]
    public void Matches_WithMinimumScore_ExcludesNullScore()
    {
        var result = ScoreFilter.Matches(null, 50);

        Assert.False(result);
    }
}
