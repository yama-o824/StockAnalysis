using StockAnalyzer.Services;
using StockAnalyzer.Services.Backtest;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Presentation;

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
}
