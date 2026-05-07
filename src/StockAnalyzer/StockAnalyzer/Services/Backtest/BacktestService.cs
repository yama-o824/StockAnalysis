using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Models.Backtest;

namespace StockAnalyzer.Services.Backtest;

public sealed class BacktestService
{
    public BacktestResult Run(AnalysisResult analysis, BacktestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(settings);

        // Phase2: クラス雛形のみ（未配線）。
        // - analysis.Signals から Buy シグナルを抽出
        // - エントリーを「翌営業日始値」で解決
        // - イグジットを「N営業日後終値」で解決
        // - BacktestTrade の一覧と集計値を作成
        return new BacktestResult
        {
            Trades = [],
            TotalSignals = 0,
            FilledTrades = 0,
            SkippedTrades = 0,
            WinRate = null,
            AverageProfitLossRate = null,
            CumulativeProfitLossRate = null,
            FirstSignalDate = null,
            LastSignalDate = null
        };
    }
}
