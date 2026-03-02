namespace StockAnalyzer.Models;

public sealed class SignalEntry
{
    public string Date { get; set; } = string.Empty;
    public SignalType Type { get; set; }
    public double Price { get; set; }
    public double Ma { get; set; }
}