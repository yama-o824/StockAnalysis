using StockAnalyzer.Services;
using StockAnalyzer.Services.Backtest;

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
}
