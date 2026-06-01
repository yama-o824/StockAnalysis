using StockAnalyzer.Services;
using StockAnalyzer.Services.Backtest;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Presentation;
using StockAnalyzer.Models.Backtest;

namespace StockAnalyzer.ViewModels;

public sealed class MainWindowViewModel
{
    private readonly StockAnalysisService _stockAnalysisService;
    private readonly PriceDataFetchService _priceDataFetchService;
    private readonly BacktestRunner _backtestRunner;
    private readonly BacktestScoreBandSummaryAggregator _backtestScoreBandSummaryAggregator;
    private readonly AnalysisHistoryRecordFactory _analysisHistoryRecordFactory;
    private readonly AnalysisHistoryCsvStore _analysisHistoryCsvStore;

    public MainWindowViewModel()
        : this(
            new StockAnalysisService(),
            new PriceDataFetchService(),
            new BacktestRunner(),
            new BacktestScoreBandSummaryAggregator(),
            new AnalysisHistoryRecordFactory(),
            new AnalysisHistoryCsvStore())
    {
    }

    public MainWindowViewModel(
        StockAnalysisService stockAnalysisService,
        PriceDataFetchService priceDataFetchService,
        BacktestRunner backtestRunner,
        BacktestScoreBandSummaryAggregator backtestScoreBandSummaryAggregator,
        AnalysisHistoryRecordFactory analysisHistoryRecordFactory,
        AnalysisHistoryCsvStore analysisHistoryCsvStore)
    {
        _stockAnalysisService = stockAnalysisService;
        _priceDataFetchService = priceDataFetchService;
        _backtestRunner = backtestRunner;
        _backtestScoreBandSummaryAggregator = backtestScoreBandSummaryAggregator;
        _analysisHistoryRecordFactory = analysisHistoryRecordFactory;
        _analysisHistoryCsvStore = analysisHistoryCsvStore;
    }

    public AnalysisResult? CurrentAnalysisResult { get; private set; }
    public string CurrentSymbol { get; private set; } = string.Empty;
    public string CurrentRequestedPeriod { get; private set; } = string.Empty;
    public string StatusText { get; private set; } = string.Empty;
    public bool IsFetching { get; private set; }
    public IReadOnlyList<PriceAnalysisRow> PriceRows { get; private set; } = [];
    public IReadOnlyList<SignalViewRow> SignalRows { get; private set; } = [];
    public BacktestSummaryViewModel? BacktestSummary { get; private set; }
    public IReadOnlyList<BacktestScoreBandSummaryViewRow> BacktestScoreBandSummaryRows { get; private set; } = [];
    public IReadOnlyList<BacktestViewRow> BacktestRows { get; private set; } = [];
    public bool CanRefreshBacktest => !IsFetching && CurrentAnalysisResult is not null;
    public bool CanSaveAnalysisHistory => !IsFetching && CurrentAnalysisResult is not null;

    public void BeginFetch()
    {
        ResetResults();
        IsFetching = true;
        StatusText = "取得中...";
    }

    public void ResetResults()
    {
        CurrentAnalysisResult = null;
        CurrentSymbol = string.Empty;
        CurrentRequestedPeriod = string.Empty;
        PriceRows = [];
        SignalRows = [];
        BacktestSummary = null;
        BacktestScoreBandSummaryRows = [];
        BacktestRows = [];
    }

    public void SetStatus(string statusText)
    {
        StatusText = statusText;
    }

    public void EndFetch(string statusText)
    {
        IsFetching = false;
        StatusText = statusText;
    }

    public async Task<MainWindowOperationResult> FetchAsync(
        string symbol,
        string requestedPeriod,
        ScoreFilterOption scoreFilterOption)
    {
        BeginFetch();

        try
        {
            var fetchResult = await _priceDataFetchService.FetchAsync(symbol, requestedPeriod);
            var analysisResult = _stockAnalysisService.Analyze(fetchResult.PriceBars);

            CurrentAnalysisResult = analysisResult;
            CurrentSymbol = symbol;
            CurrentRequestedPeriod = requestedPeriod;
            PriceRows = analysisResult.Bars
                .Select(PriceAnalysisRow.From)
                .ToList();

            RefreshSignals(scoreFilterOption);
            EndFetch($"取得完了: {fetchResult.Rows.Count}件");

            return MainWindowOperationResult.Success(StatusText);
        }
        catch (PriceDataFetchException ex)
        {
            ResetResults();
            EndFetch("取得失敗");
            return MainWindowOperationResult.Failure(
                StatusText,
                exception: ex,
                stderr: ex.Stderr,
                exitCode: ex.ExitCode);
        }
        catch (Exception ex)
        {
            ResetResults();
            EndFetch("取得失敗");
            return MainWindowOperationResult.Failure(
                StatusText,
                exception: ex);
        }
    }

    public void RefreshSignals(ScoreFilterOption scoreFilterOption)
    {
        if (CurrentAnalysisResult is null)
        {
            SignalRows = [];
            return;
        }

        SignalRows = CurrentAnalysisResult.Signals
            .Where(x => ScoreFilter.Matches(x.Score?.Total, scoreFilterOption.MinimumScore))
            .Select(SignalViewRow.From)
            .ToList();
    }

    public MainWindowOperationResult RefreshBacktest(
        BacktestSettings settings,
        ScoreFilterOption scoreFilterOption,
        string successStatusMessage = "バックテスト結果を更新しました。")
    {
        if (CurrentAnalysisResult is null)
        {
            return MainWindowOperationResult.Failure(
                StatusText,
                userMessage: "先に価格データを取得してください。");
        }

        try
        {
            var backtestResult = _backtestRunner.Run(CurrentAnalysisResult, settings);

            BacktestSummary = BacktestSummaryViewModel.From(backtestResult);
            BacktestScoreBandSummaryRows = _backtestScoreBandSummaryAggregator
                .Create(backtestResult)
                .Select(BacktestScoreBandSummaryViewRow.From)
                .ToList();
            BacktestRows = backtestResult.Trades
                .Where(x => ScoreFilter.Matches(x.SignalScore?.Total, scoreFilterOption.MinimumScore))
                .Select(BacktestViewRow.From)
                .ToList();
            StatusText = successStatusMessage;

            return MainWindowOperationResult.Success(StatusText);
        }
        catch (Exception ex)
        {
            return MainWindowOperationResult.Failure(
                StatusText,
                exception: ex);
        }
    }

    public MainWindowOperationResult SaveAnalysisHistory()
    {
        if (CurrentAnalysisResult is null)
        {
            return MainWindowOperationResult.Failure(
                StatusText,
                userMessage: "先に価格データを取得してください。");
        }

        var records = _analysisHistoryRecordFactory.Create(
            CurrentSymbol,
            CurrentRequestedPeriod,
            DateTimeOffset.Now,
            Guid.NewGuid().ToString("N"),
            CurrentAnalysisResult);

        if (records.Count == 0)
        {
            return MainWindowOperationResult.Failure(
                StatusText,
                userMessage: "保存対象のシグナルがありません。");
        }

        try
        {
            _analysisHistoryCsvStore.Append(records);
            StatusText = $"分析結果を保存しました: {records.Count}件";

            return MainWindowOperationResult.Success(
                StatusText,
                $"分析結果を保存しました。\n\n保存先: {_analysisHistoryCsvStore.FilePath}");
        }
        catch (Exception ex)
        {
            return MainWindowOperationResult.Failure(
                StatusText,
                userMessage: $"保存に失敗しました。\n\n--- 詳細 ---\n{ex.Message}",
                exception: ex);
        }
    }
}
