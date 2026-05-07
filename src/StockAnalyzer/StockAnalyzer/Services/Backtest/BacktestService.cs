using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Models.Backtest;

namespace StockAnalyzer.Services.Backtest;

public sealed class BacktestService
{
    public BacktestResult Run(AnalysisResult analysis, BacktestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.ExitAfterBusinessDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(settings.ExitAfterBusinessDays), "ExitAfterBusinessDays は1以上で指定してください。");

        var targetSignalType = ToSignalType(settings.SignalType);
        var targetSignals = analysis.Signals
            .Where(s => s.Candidate.Type == targetSignalType)
            .OrderBy(s => s.Candidate.Date)
            .ToList();

        var indexByDate = analysis.Bars
            .Select((bar, index) => new { bar.Raw.Date, index })
            .ToDictionary(x => x.Date, x => x.index);

        var trades = targetSignals
            .Select(signal => BuildTrade(signal.Candidate.Date, analysis, indexByDate, settings))
            .ToList();

        var filledTrades = trades.Where(t => t.Status == BacktestTradeStatus.Filled).ToList();
        var wins = filledTrades.Count(t => t.ProfitLossRate > 0);

        return new BacktestResult
        {
            Trades = trades,
            TotalSignals = targetSignals.Count,
            FilledTrades = filledTrades.Count,
            SkippedTrades = trades.Count - filledTrades.Count,
            WinRate = filledTrades.Count == 0 ? null : (double)wins / filledTrades.Count,
            AverageProfitLossRate = filledTrades.Count == 0 ? null : filledTrades.Average(t => t.ProfitLossRate ?? 0),
            CumulativeProfitLossRate = filledTrades.Count == 0 ? null : filledTrades.Sum(t => t.ProfitLossRate ?? 0),
            FirstSignalDate = targetSignals.FirstOrDefault()?.Candidate.Date,
            LastSignalDate = targetSignals.LastOrDefault()?.Candidate.Date
        };
    }

    private static SignalType ToSignalType(BacktestSignalType signalType)
    {
        return signalType switch
        {
            BacktestSignalType.Buy => SignalType.Buy,
            _ => throw new ArgumentOutOfRangeException(nameof(signalType), signalType, null)
        };
    }

    private static BacktestTrade BuildTrade(
        DateOnly signalDate,
        AnalysisResult analysis,
        IReadOnlyDictionary<DateOnly, int> indexByDate,
        BacktestSettings settings)
    {
        if (!indexByDate.TryGetValue(signalDate, out var signalIndex))
        {
            return new BacktestTrade
            {
                SignalDate = signalDate,
                HoldingBusinessDays = settings.ExitAfterBusinessDays,
                Status = BacktestTradeStatus.SkippedInsufficientBars,
                ExitReason = ExitReason.NotExitedInsufficientBars
            };
        }

        var entryIndex = signalIndex + 1;
        var exitIndex = signalIndex + settings.ExitAfterBusinessDays;

        if (entryIndex >= analysis.Bars.Count || exitIndex >= analysis.Bars.Count)
        {
            return new BacktestTrade
            {
                SignalDate = signalDate,
                HoldingBusinessDays = settings.ExitAfterBusinessDays,
                Status = BacktestTradeStatus.SkippedInsufficientBars,
                ExitReason = ExitReason.NotExitedInsufficientBars
            };
        }

        var entryBar = analysis.Bars[entryIndex].Raw;
        var exitBar = analysis.Bars[exitIndex].Raw;
        var profitLoss = exitBar.Close - entryBar.Open;
        var profitLossRate = entryBar.Open == 0 ? (double?)null : profitLoss / entryBar.Open;

        return new BacktestTrade
        {
            SignalDate = signalDate,
            EntryDate = entryBar.Date,
            EntryPrice = entryBar.Open,
            ExitDate = exitBar.Date,
            ExitPrice = exitBar.Close,
            HoldingBusinessDays = settings.ExitAfterBusinessDays,
            ProfitLoss = profitLoss,
            ProfitLossRate = profitLossRate,
            Status = BacktestTradeStatus.Filled,
            ExitReason = ExitReason.FixedHoldingPeriod
        };
    }
}
