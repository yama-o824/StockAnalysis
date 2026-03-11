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

            DetectCrossSignalCore(signals, prev, current);
        }

        return signals;
    }

    private static void DetectCrossSignalCore(List<SignalEntry> signals, PriceRow prev, PriceRow current)
    {
        var prevClose = prev.Close;
        var prevMa = prev.MA75.Value;
        var currentClose = current.Close;
        var currentMa = current.MA75.Value;

        var prevDiff = prevClose - prevMa;
        var currentDiff = currentClose - currentMa;

        // BUY: ゴールデンクロス
        if (prevDiff < 0 && currentDiff > 0)
        {
            signals.Add(new SignalEntry
            {
                Date = current.Date,
                Type = SignalType.Buy,
                PrevPrice = prevClose,
                PrevMa = prevMa,
                PrevDiff = prevDiff,
                Price = currentClose,
                Ma = currentMa,
                CurrentDiff = currentDiff
            });
        }
        // SELL: デッドクロス
        else if (prevDiff > 0 && currentDiff < 0)
        {
            signals.Add(new SignalEntry
            {
                Date = current.Date,
                Type = SignalType.Sell,
                PrevPrice = prevClose,
                PrevMa = prevMa,
                PrevDiff = prevDiff,
                Price = currentClose,
                Ma = currentMa,
                CurrentDiff = currentDiff
            });
        }
    }
}