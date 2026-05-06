namespace StockAnalyzer.Models.Market;

public sealed class PriceBar
{
    public DateOnly Date { get; init; }
    public double Open { get; init; }
    public double High { get; init; }
    public double Low { get; init; }
    public double Close { get; init; }
    public long Volume { get; init; }
}
