namespace StockAnalyzer.Presentation;

public static class PeriodOptions
{
    public const string DefaultValue = "1y";

    public static IReadOnlyList<PeriodOption> All { get; } =
    [
        new("3mo", "3ヶ月"),
        new("6mo", "6ヶ月"),
        new("1y", "1年"),
        new("3y", "3年"),
        new("5y", "5年")
    ];
}
