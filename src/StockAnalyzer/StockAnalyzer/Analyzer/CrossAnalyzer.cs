using StockAnalyzer.Models;

namespace StockAnalyzer.Analyzer;
public static class CrossAnalyzer
{
    public static List<SignalEntry> DetectCrossSignals(IReadOnlyList<PriceRow> rows)
    {
        var signals = new List<SignalEntry>();

        if (rows.Count < 2)
            return signals;

        for (int i = 1; i < rows.Count; i++)
        {
            var prev = rows[i - 1];
            var current = rows[i];

            if (prev.MA75 is null || current.MA75 is null) continue;

            var prevClose = prev.Close;
            var prevMa = prev.MA75.Value;

            var close = current.Close;
            var ma = current.MA75.Value;

            // BUY: ゴールデンクロス
            if (prevClose < prevMa && close >= ma)
            {
                signals.Add(CreateSignalEntry(current, SignalType.Buy));
            }
            // SELL: デッドクロス
            else if (prevClose > prevMa && close <= ma)
            {
                signals.Add(CreateSignalEntry(current, SignalType.Sell));
            }
        }

        return signals;
    }

    private static SignalEntry CreateSignalEntry(PriceRow current, SignalType type)
    {
        return new SignalEntry
        {
            Date = current.Date,
            Type = type,
            Price = current.Close,
            Ma = current.MA75 ?? 0
        };
    }
}