namespace StockAnalyzer.Presentation;

public static class ScoreFilterOptions
{
    public static IReadOnlyList<ScoreFilterOption> All { get; } =
    [
        new(null, "なし"),
        new(50, "50以上"),
        new(75, "75以上"),
        new(90, "90以上")
    ];
}
