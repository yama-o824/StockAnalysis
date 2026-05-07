namespace StockAnalyzer.Models;

public sealed class PriceRow
{
    public string Date { get; set; } = string.Empty;
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    public long Volume { get; set; }
}
