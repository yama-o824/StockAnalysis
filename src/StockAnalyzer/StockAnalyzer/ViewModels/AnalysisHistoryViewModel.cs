using StockAnalyzer.Models;
using StockAnalyzer.Presentation;
using StockAnalyzer.Services;

namespace StockAnalyzer.ViewModels;

public sealed class AnalysisHistoryViewModel : ObservableObject
{
    private readonly AnalysisHistoryCsvStore _analysisHistoryCsvStore;
    private IReadOnlyList<AnalysisHistoryViewRow> _allRows = [];
    private IReadOnlyList<AnalysisHistoryViewRow> _historyRows = [];
    private AnalysisHistoryViewRow? _selectedHistoryRow;
    private string _symbolFilterText = string.Empty;
    private AnalysisHistorySignalTypeFilterOption? _selectedSignalTypeFilter;
    private AnalysisHistorySortOrderOption? _selectedSortOrder;
    private string _statusText = string.Empty;
    private bool _hasLoadedHistory;

    public AnalysisHistoryViewModel()
        : this(new AnalysisHistoryCsvStore())
    {
    }

    public AnalysisHistoryViewModel(AnalysisHistoryCsvStore analysisHistoryCsvStore)
    {
        _analysisHistoryCsvStore = analysisHistoryCsvStore;
        SignalTypeFilterOptions =
        [
            new(null, "すべて"),
            new(SignalType.Buy, "買い"),
            new(SignalType.Sell, "売り")
        ];
        SortOrderOptions =
        [
            new(AnalysisHistorySortOrder.SignalDateDescending, "日付の新しい順"),
            new(AnalysisHistorySortOrder.SignalDateAscending, "日付の古い順")
        ];
        _selectedSignalTypeFilter = SignalTypeFilterOptions[0];
        _selectedSortOrder = SortOrderOptions[0];
        LoadHistoryCommand = new RelayCommand(LoadHistory);
        ClearFilterCommand = new RelayCommand(ClearFilter);
    }

    public IReadOnlyList<AnalysisHistorySignalTypeFilterOption> SignalTypeFilterOptions { get; }
    public IReadOnlyList<AnalysisHistorySortOrderOption> SortOrderOptions { get; }
    public IReadOnlyList<AnalysisHistoryViewRow> HistoryRows
    {
        get => _historyRows;
        private set
        {
            if (SetProperty(ref _historyRows, value))
            {
                OnPropertyChanged(nameof(HasHistoryRows));
            }
        }
    }
    public AnalysisHistoryViewRow? SelectedHistoryRow
    {
        get => _selectedHistoryRow;
        set => SetProperty(ref _selectedHistoryRow, value);
    }
    public string SymbolFilterText
    {
        get => _symbolFilterText;
        set
        {
            if (SetProperty(ref _symbolFilterText, value))
            {
                ApplyFilters();
            }
        }
    }
    public AnalysisHistorySignalTypeFilterOption? SelectedSignalTypeFilter
    {
        get => _selectedSignalTypeFilter;
        set
        {
            if (SetProperty(ref _selectedSignalTypeFilter, value))
            {
                ApplyFilters();
            }
        }
    }
    public AnalysisHistorySortOrderOption? SelectedSortOrder
    {
        get => _selectedSortOrder;
        set
        {
            if (SetProperty(ref _selectedSortOrder, value))
            {
                ApplyFilters();
            }
        }
    }
    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }
    public bool HasHistoryRows => HistoryRows.Count > 0;
    public bool HasLoadedHistory => _hasLoadedHistory;
    public RelayCommand LoadHistoryCommand { get; }
    public RelayCommand ClearFilterCommand { get; }
    public event EventHandler<UserMessageRequestedEventArgs>? MessageRequested;

    public void LoadHistory()
    {
        try
        {
            LoadRows();
            _hasLoadedHistory = true;
            OnPropertyChanged(nameof(HasLoadedHistory));
            ApplyFilters();
            StatusText = _allRows.Count == 0
                ? "保存済みの分析履歴はありません。"
                : $"分析履歴を読み込みました: {_allRows.Count}件";
        }
        catch (Exception ex)
        {
            ClearRows();
            StatusText = "分析履歴の読み込みに失敗しました。";
            RequestMessage(
                $"分析履歴の読み込みに失敗しました。\n\n--- 詳細 ---\n{ex.Message}",
                "履歴読み込みエラー",
                UserMessageKind.Warning);
        }
    }

    public void ReloadIfLoaded()
    {
        if (!_hasLoadedHistory)
        {
            return;
        }

        try
        {
            LoadRows();
            ApplyFilters();
            StatusText = $"分析履歴を更新しました: {_allRows.Count}件";
        }
        catch (Exception ex)
        {
            StatusText = $"保存は完了しましたが、履歴一覧の更新に失敗しました: {ex.Message}";
        }
    }

    public void ClearFilter()
    {
        SymbolFilterText = string.Empty;
        SelectedSignalTypeFilter = SignalTypeFilterOptions[0];
    }

    private void LoadRows()
    {
        var records = _analysisHistoryCsvStore.Load();
        _allRows = records
            .Select(AnalysisHistoryViewRow.From)
            .ToList();
    }

    private void ClearRows()
    {
        _allRows = [];
        HistoryRows = [];
        SelectedHistoryRow = null;
    }

    private void ApplyFilters()
    {
        IEnumerable<AnalysisHistoryViewRow> rows = _allRows;

        if (!string.IsNullOrWhiteSpace(SymbolFilterText))
        {
            rows = rows.Where(x => x.Symbol.Contains(SymbolFilterText.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (SelectedSignalTypeFilter?.Value is SignalType signalType)
        {
            rows = rows.Where(x => x.SignalType == signalType);
        }

        rows = SelectedSortOrder?.Value == AnalysisHistorySortOrder.SignalDateAscending
            ? rows.OrderBy(x => x.SignalDate).ThenBy(x => x.ExecutedAt)
            : rows.OrderByDescending(x => x.SignalDate).ThenByDescending(x => x.ExecutedAt);

        HistoryRows = rows.ToList();
        SelectedHistoryRow = HistoryRows.FirstOrDefault();
        StatusText = _hasLoadedHistory
            ? $"表示中: {HistoryRows.Count}件 / 全{_allRows.Count}件"
            : StatusText;
    }

    private void RequestMessage(
        string message,
        string title,
        UserMessageKind kind)
    {
        MessageRequested?.Invoke(this, new UserMessageRequestedEventArgs(message, title, kind));
    }
}
