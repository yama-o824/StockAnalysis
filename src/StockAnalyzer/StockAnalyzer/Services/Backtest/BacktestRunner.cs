using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Models.Backtest;

namespace StockAnalyzer.Services.Backtest;

public sealed class BacktestRunner
{
    public BacktestResult Run(
        AnalysisResult analysisResult,
        BacktestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(analysisResult);
        ArgumentNullException.ThrowIfNull(settings);

        ValidateSettings(settings);

        var barIndexByDate = analysisResult.Bars
            .Select((bar, index) => new { bar.Raw.Date, Index = index })
            .ToDictionary(x => x.Date, x => x.Index);

        var trades = analysisResult.Signals
            .Where(x => x.Candidate.Type == settings.TargetSignalType)
            .Select(x => CreateTradeOrDefault(x, analysisResult.Bars, barIndexByDate, settings))
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        return new BacktestResult
        {
            Settings = settings,
            Trades = trades
        };
    }

    private static BacktestTrade? CreateTradeOrDefault(
        SignalResult signal,
        IReadOnlyList<AnalysisBar> bars,
        IReadOnlyDictionary<DateOnly, int> barIndexByDate,
        BacktestSettings settings)
    {
        if (!barIndexByDate.TryGetValue(signal.Candidate.Date, out var signalIndex))
        {
            return null;
        }

        var entryIndex = signalIndex + settings.EntryDelayBars;
        if (entryIndex >= bars.Count)
        {
            return null;
        }

        var exitIndex = entryIndex + settings.ExitAfterBars;
        if (exitIndex >= bars.Count)
        {
            return null;
        }

        var entryBar = bars[entryIndex].Raw;
        var exitBar = bars[exitIndex].Raw;

        return new BacktestTrade
        {
            Signal = signal,
            SignalDate = signal.Candidate.Date,
            EntryDate = entryBar.Date,
            EntryPrice = entryBar.Open,
            ExitDate = exitBar.Date,
            ExitPrice = exitBar.Close
        };
    }

    private static void ValidateSettings(BacktestSettings settings)
    {
        if (settings.EntryDelayBars < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(settings),
                settings.EntryDelayBars,
                "EntryDelayBars must be 1 or greater.");
        }

        if (settings.ExitAfterBars < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(settings),
                settings.ExitAfterBars,
                "ExitAfterBars must be 1 or greater.");
        }
    }
}
