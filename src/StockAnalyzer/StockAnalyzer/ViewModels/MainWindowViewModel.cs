using StockAnalyzer.Services;
using StockAnalyzer.Services.Backtest;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Presentation;
using StockAnalyzer.Models.Backtest;
using StockAnalyzer.Models;

namespace StockAnalyzer.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly StockAnalysisService _stockAnalysisService;
    private readonly PriceDataFetchService _priceDataFetchService;
    private readonly BacktestRunner _backtestRunner;
    private readonly BacktestScoreBandSummaryAggregator _backtestScoreBandSummaryAggregator;
    private readonly AnalysisHistoryRecordFactory _analysisHistoryRecordFactory;
    private readonly AnalysisHistoryCsvStore _analysisHistoryCsvStore;
    private readonly SymbolHistoryStore _symbolHistoryStore;

    public MainWindowViewModel()
        : this(
            new StockAnalysisService(),
            new PriceDataFetchService(),
            new BacktestRunner(),
            new BacktestScoreBandSummaryAggregator(),
            new AnalysisHistoryRecordFactory(),
            new AnalysisHistoryCsvStore(),
            new SymbolHistoryStore())
    {
    }

    public MainWindowViewModel(
        StockAnalysisService stockAnalysisService,
        PriceDataFetchService priceDataFetchService,
        BacktestRunner backtestRunner,
        BacktestScoreBandSummaryAggregator backtestScoreBandSummaryAggregator,
        AnalysisHistoryRecordFactory analysisHistoryRecordFactory,
        AnalysisHistoryCsvStore analysisHistoryCsvStore,
        SymbolHistoryStore symbolHistoryStore,
        AnalysisHistoryViewModel? historyViewModel = null)
    {
        _stockAnalysisService = stockAnalysisService;
        _priceDataFetchService = priceDataFetchService;
        _backtestRunner = backtestRunner;
        _backtestScoreBandSummaryAggregator = backtestScoreBandSummaryAggregator;
        _analysisHistoryRecordFactory = analysisHistoryRecordFactory;
        _analysisHistoryCsvStore = analysisHistoryCsvStore;
        _symbolHistoryStore = symbolHistoryStore;
        History = historyViewModel ?? new AnalysisHistoryViewModel(analysisHistoryCsvStore);
        History.MessageRequested += History_MessageRequested;
        SignalTypeOptions =
        [
            new(SignalType.Buy, "買い"),
            new(SignalType.Sell, "売り")
        ];
        AvailablePeriodOptions = Presentation.PeriodOptions.All;
        AvailableScoreFilterOptions = Presentation.ScoreFilterOptions.All;
        _selectedPeriod = AvailablePeriodOptions.First(x => x.Value == Presentation.PeriodOptions.DefaultValue);
        _selectedScoreFilter = AvailableScoreFilterOptions.First(x => x.MinimumScore is null);
        _selectedSignalType = SignalTypeOptions.First(x => x.Value == SignalType.Buy);
        FetchCommand = new AsyncRelayCommand(FetchFromInputAsync, () => !IsFetching);
        RefreshBacktestCommand = new RelayCommand(RefreshBacktestFromInput, () => CanRefreshBacktest);
        SaveAnalysisHistoryCommand = new RelayCommand(SaveAnalysisHistoryFromInput, () => CanSaveAnalysisHistory);
    }

    private string _symbolText = string.Empty;
    private PeriodOption? _selectedPeriod;
    private SignalTypeOption? _selectedSignalType;
    private ScoreFilterOption? _selectedScoreFilter;
    private string _entryDelayText = "1";
    private string _holdingBarsText = "5";
    private IReadOnlyList<string> _symbolHistory = [];

    public IReadOnlyList<PeriodOption> AvailablePeriodOptions { get; }
    public IReadOnlyList<SignalTypeOption> SignalTypeOptions { get; }
    public IReadOnlyList<ScoreFilterOption> AvailableScoreFilterOptions { get; }
    public string SymbolText
    {
        get => _symbolText;
        set
        {
            if (SetProperty(ref _symbolText, value))
            {
                FetchCommand.RaiseCanExecuteChanged();
            }
        }
    }
    public PeriodOption? SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            if (SetProperty(ref _selectedPeriod, value))
            {
                FetchCommand.RaiseCanExecuteChanged();
            }
        }
    }
    public SignalTypeOption? SelectedSignalType
    {
        get => _selectedSignalType;
        set
        {
            if (SetProperty(ref _selectedSignalType, value))
            {
                RefreshBacktestCommand.RaiseCanExecuteChanged();
            }
        }
    }
    public ScoreFilterOption? SelectedScoreFilter
    {
        get => _selectedScoreFilter;
        set
        {
            if (SetProperty(ref _selectedScoreFilter, value))
            {
                RefreshSignals(GetSelectedScoreFilterOption());
                if (CurrentAnalysisResult is not null && TryCreateBacktestSettings(out var backtestSettings, notifyOnError: false))
                {
                    RefreshBacktest(backtestSettings, GetSelectedScoreFilterOption(), StatusText);
                }

                RaiseStateChanged();
            }
        }
    }
    public string EntryDelayText
    {
        get => _entryDelayText;
        set => SetProperty(ref _entryDelayText, value);
    }
    public string HoldingBarsText
    {
        get => _holdingBarsText;
        set => SetProperty(ref _holdingBarsText, value);
    }
    public IReadOnlyList<string> SymbolHistory
    {
        get => _symbolHistory;
        private set => SetProperty(ref _symbolHistory, value);
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
    public AnalysisHistoryViewModel History { get; }
    public bool CanRefreshBacktest => !IsFetching && CurrentAnalysisResult is not null;
    public bool CanSaveAnalysisHistory => !IsFetching && CurrentAnalysisResult is not null;
    public bool HasBacktestSummary => BacktestSummary is not null;
    public AsyncRelayCommand FetchCommand { get; }
    public RelayCommand RefreshBacktestCommand { get; }
    public RelayCommand SaveAnalysisHistoryCommand { get; }
    public event EventHandler<UserMessageRequestedEventArgs>? MessageRequested;
    public event EventHandler<MainWindowOperationResult>? OperationFailed;

    public void LoadSymbolHistory()
    {
        try
        {
            SymbolHistory = _symbolHistoryStore.Load();
        }
        catch
        {
            SymbolHistory = [];
        }
    }

    public void BeginFetch()
    {
        ResetResults();
        IsFetching = true;
        StatusText = "取得中...";
        RaiseStateChanged();
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
        RaiseStateChanged();
    }

    public void SetStatus(string statusText)
    {
        StatusText = statusText;
        OnPropertyChanged(nameof(StatusText));
    }

    public void EndFetch(string statusText)
    {
        IsFetching = false;
        StatusText = statusText;
        RaiseStateChanged();
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
            RaiseStateChanged();

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
            History.ReloadIfLoaded();
            StatusText = $"分析結果を保存しました: {records.Count}件";
            OnPropertyChanged(nameof(StatusText));

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

    private async Task FetchFromInputAsync()
    {
        var symbol = Services.SymbolHistory.Normalize(SymbolText);
        if (string.IsNullOrWhiteSpace(symbol))
        {
            RequestMessage("銘柄を入力してください", "エラー", UserMessageKind.Warning);
            return;
        }

        SymbolText = symbol;

        if (SelectedPeriod is null)
        {
            RequestMessage("取得期間を選択してください。", "エラー", UserMessageKind.Warning);
            return;
        }

        if (!TryCreateBacktestSettings(out var backtestSettings))
        {
            return;
        }

        var scoreFilterOption = GetSelectedScoreFilterOption();
        var result = await FetchAsync(symbol, SelectedPeriod.Value, scoreFilterOption);

        if (!result.Succeeded)
        {
            RequestFailureMessage(result);
            return;
        }

        var backtestResult = RefreshBacktest(
            backtestSettings,
            scoreFilterOption,
            StatusText);

        if (!backtestResult.Succeeded)
        {
            RequestFailureMessage(backtestResult);
            return;
        }

        UpdateSymbolHistory(symbol);
    }

    private void RefreshBacktestFromInput()
    {
        if (!TryCreateBacktestSettings(out var backtestSettings))
        {
            return;
        }

        var result = RefreshBacktest(
            backtestSettings,
            GetSelectedScoreFilterOption());

        if (!result.Succeeded)
        {
            RequestFailureMessage(result);
        }
    }

    private void SaveAnalysisHistoryFromInput()
    {
        var result = SaveAnalysisHistory();

        if (!result.Succeeded)
        {
            RequestFailureMessage(result, "保存エラー");
            return;
        }

        if (!string.IsNullOrWhiteSpace(result.UserMessage))
        {
            RequestMessage(result.UserMessage, "保存完了", UserMessageKind.Information);
        }
    }

    private void UpdateSymbolHistory(string symbol)
    {
        try
        {
            SymbolHistory = _symbolHistoryStore.Add(symbol);
            SymbolText = symbol;
        }
        catch
        {
            // 履歴保存に失敗しても、取得済みの分析結果表示は成功扱いにする。
        }
    }

    private bool TryCreateBacktestSettings(
        out BacktestSettings settings,
        bool notifyOnError = true)
    {
        settings = default!;

        if (SelectedSignalType is null)
        {
            if (notifyOnError)
            {
                RequestMessage("対象シグナルを選択してください。", "エラー", UserMessageKind.Warning);
            }
            return false;
        }

        if (!TryParsePositiveInt(EntryDelayText, "エントリーまでの営業日数", out var entryDelayBars, notifyOnError))
        {
            return false;
        }

        if (!TryParsePositiveInt(HoldingBarsText, "保有営業日数", out var exitAfterBars, notifyOnError))
        {
            return false;
        }

        settings = new BacktestSettings
        {
            TargetSignalType = SelectedSignalType.Value,
            EntryDelayBars = entryDelayBars,
            ExitAfterBars = exitAfterBars
        };

        return true;
    }

    private bool TryParsePositiveInt(
        string? text,
        string label,
        out int value,
        bool notifyOnError = true)
    {
        if (!int.TryParse(text, out value) || value < 1)
        {
            if (notifyOnError)
            {
                RequestMessage($"{label}は1以上の整数で入力してください。", "エラー", UserMessageKind.Warning);
            }
            return false;
        }

        return true;
    }

    private ScoreFilterOption GetSelectedScoreFilterOption()
    {
        return SelectedScoreFilter
            ?? AvailableScoreFilterOptions.First(x => x.MinimumScore is null);
    }

    private void RequestFailureMessage(
        MainWindowOperationResult result,
        string title = "エラー")
    {
        if (result.Exception is not null)
        {
            OperationFailed?.Invoke(this, result);
            return;
        }

        if (!string.IsNullOrWhiteSpace(result.UserMessage))
        {
            RequestMessage(result.UserMessage, title, UserMessageKind.Warning);
        }
    }

    private void RequestMessage(
        string message,
        string title,
        UserMessageKind kind)
    {
        MessageRequested?.Invoke(this, new UserMessageRequestedEventArgs(message, title, kind));
    }

    private void History_MessageRequested(object? sender, UserMessageRequestedEventArgs e)
    {
        MessageRequested?.Invoke(this, e);
    }

    private void RaiseStateChanged()
    {
        OnPropertyChanged(nameof(CurrentAnalysisResult));
        OnPropertyChanged(nameof(CurrentSymbol));
        OnPropertyChanged(nameof(CurrentRequestedPeriod));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(IsFetching));
        OnPropertyChanged(nameof(PriceRows));
        OnPropertyChanged(nameof(SignalRows));
        OnPropertyChanged(nameof(BacktestSummary));
        OnPropertyChanged(nameof(HasBacktestSummary));
        OnPropertyChanged(nameof(BacktestScoreBandSummaryRows));
        OnPropertyChanged(nameof(BacktestRows));
        OnPropertyChanged(nameof(History));
        OnPropertyChanged(nameof(CanRefreshBacktest));
        OnPropertyChanged(nameof(CanSaveAnalysisHistory));
        RefreshBacktestCommand.RaiseCanExecuteChanged();
        SaveAnalysisHistoryCommand.RaiseCanExecuteChanged();
        FetchCommand.RaiseCanExecuteChanged();
    }
}
