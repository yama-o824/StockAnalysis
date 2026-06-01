using StockAnalyzer.ViewModels;
using StockAnalyzer.Models;
using StockAnalyzer.Services;
using StockAnalyzer.Services.Backtest;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class MainWindowViewModelTests
{
    [Fact(DisplayName = "取得開始時は結果をリセットして取得中状態にする")]
    public void BeginFetch_ResetsResultsAndSetsFetchingState()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.BeginFetch();

        Assert.True(viewModel.IsFetching);
        Assert.Equal("取得中...", viewModel.StatusText);
        Assert.Null(viewModel.CurrentAnalysisResult);
        Assert.Empty(viewModel.CurrentSymbol);
        Assert.Empty(viewModel.CurrentRequestedPeriod);
        Assert.Empty(viewModel.PriceRows);
        Assert.Empty(viewModel.SignalRows);
        Assert.Null(viewModel.BacktestSummary);
        Assert.Empty(viewModel.BacktestScoreBandSummaryRows);
        Assert.Empty(viewModel.BacktestRows);
        Assert.False(viewModel.CanRefreshBacktest);
        Assert.False(viewModel.CanSaveAnalysisHistory);
    }

    [Fact(DisplayName = "初期表示用の選択肢と入力値を保持する")]
    public void Constructor_InitializesInputState()
    {
        var viewModel = new MainWindowViewModel();

        Assert.Equal("1", viewModel.EntryDelayText);
        Assert.Equal("5", viewModel.HoldingBarsText);
        Assert.NotEmpty(viewModel.AvailablePeriodOptions);
        Assert.NotEmpty(viewModel.AvailableScoreFilterOptions);
        Assert.NotEmpty(viewModel.SignalTypeOptions);
        Assert.Equal("1y", viewModel.SelectedPeriod?.Value);
        Assert.Null(viewModel.SelectedScoreFilter?.MinimumScore);
        Assert.Equal(SignalType.Buy, viewModel.SelectedSignalType?.Value);
    }

    [Fact(DisplayName = "取得終了時は取得中状態を解除してステータスを更新する")]
    public void EndFetch_ClearsFetchingStateAndUpdatesStatus()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.BeginFetch();
        viewModel.EndFetch("取得失敗");

        Assert.False(viewModel.IsFetching);
        Assert.Equal("取得失敗", viewModel.StatusText);
    }

    [Fact(DisplayName = "分析結果がない場合は履歴保存できない")]
    public void SaveAnalysisHistory_WithoutAnalysisResult_ReturnsFailure()
    {
        var viewModel = new MainWindowViewModel();

        var result = viewModel.SaveAnalysisHistory();

        Assert.False(result.Succeeded);
        Assert.Equal("先に価格データを取得してください。", result.UserMessage);
    }

    [Fact(DisplayName = "保存コマンドは保存対象がない場合にメッセージを通知する")]
    public void SaveAnalysisHistoryCommand_WithoutAnalysisResult_RequestsMessage()
    {
        var viewModel = new MainWindowViewModel();
        UserMessageRequestedEventArgs? message = null;
        viewModel.MessageRequested += (_, e) => message = e;

        viewModel.SaveAnalysisHistoryCommand.Execute(null);

        Assert.NotNull(message);
        Assert.Equal("先に価格データを取得してください。", message.Message);
        Assert.Equal("保存エラー", message.Title);
        Assert.Equal(UserMessageKind.Warning, message.Kind);
    }

    [Fact(DisplayName = "履歴ViewModelのメッセージ通知をMainWindowViewModelから中継する")]
    public void HistoryMessageRequested_RelaysMessage()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "analysis-history.csv");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, "InvalidHeader\n");
        var history = new AnalysisHistoryViewModel(new AnalysisHistoryCsvStore(filePath));
        var viewModel = new MainWindowViewModel(
            new StockAnalysisService(),
            new PriceDataFetchService(),
            new BacktestRunner(),
            new BacktestScoreBandSummaryAggregator(),
            new AnalysisHistoryRecordFactory(),
            new AnalysisHistoryCsvStore(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "analysis-history.csv")),
            new SymbolHistoryStore(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "symbol-history.json")),
            history);
        UserMessageRequestedEventArgs? message = null;
        viewModel.MessageRequested += (_, e) => message = e;

        viewModel.History.LoadHistory();

        Assert.NotNull(message);
        Assert.Equal("履歴読み込みエラー", message.Title);
    }
}
