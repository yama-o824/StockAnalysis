namespace StockAnalyzer.Presentation;

public static class ScoreFilter
{
    public static bool Matches(int? score, int? minimumScore)
    {
        return minimumScore is null || score >= minimumScore;
    }
}
