namespace StockAnalyzer.Models;

public sealed class SignalEntry
{
    public string Date { get; set; } = string.Empty;
    public SignalType Type { get; set; }

    public double PrevPrice { get; set; }
    public double PrevMa { get; set; }
    public double PrevDiff { get; set; }

    public double Price { get; set; }
    public double Ma { get; set; }
    public double CurrentDiff { get; set; }
}