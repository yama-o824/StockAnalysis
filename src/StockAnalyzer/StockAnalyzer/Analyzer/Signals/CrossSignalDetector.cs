using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;

namespace StockAnalyzer.Analyzer.Signals;

public sealed class CrossSignalDetector
{
    public IReadOnlyList<SignalCandidate> Detect(IReadOnlyList<AnalysisBar> bars)
    {
        ArgumentNullException.ThrowIfNull(bars);

        var candidates = new List<SignalCandidate>();

        if (bars.Count < 2)
        {
            return candidates;
        }

        for (int i = 1; i < bars.Count; i++)
        {
            var previous = bars[i - 1];
            var current = bars[i];

            if (previous.Ma75 is null || current.Ma75 is null)
            {
                continue;
            }

            TryAddCandidate(candidates, previous, current);
        }

        return candidates;
    }

    private static void TryAddCandidate(
        List<SignalCandidate> candidates,
        AnalysisBar previous,
        AnalysisBar current)
    {
        var prevDiff = previous.Raw.Close - previous.Ma75!.Value;
        var currentDiff = current.Raw.Close - current.Ma75!.Value;

        if (prevDiff < 0 && currentDiff > 0)
        {
            candidates.Add(new SignalCandidate
            {
                Type = SignalType.Buy,
                Previous = previous,
                Current = current
            });
        }
        else if (prevDiff > 0 && currentDiff < 0)
        {
            candidates.Add(new SignalCandidate
            {
                Type = SignalType.Sell,
                Previous = previous,
                Current = current
            });
        }
    }
}
