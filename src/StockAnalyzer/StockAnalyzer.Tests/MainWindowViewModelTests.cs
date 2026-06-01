using StockAnalyzer.ViewModels;
using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Models.Market;
using StockAnalyzer.Services;
using StockAnalyzer.Services.Backtest;
using System.Reflection;
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
        Assert.False(viewModel.IsCurrentAnalysisSaved);
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

    [Fact(DisplayName = "保存成功後は同じ分析結果を再保存できない")]
    public void SaveAnalysisHistory_AfterSuccessfulSave_DisablesFurtherSave()
    {
        var historyFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "analysis-history.csv");
        var viewModel = CreateViewModel(historyFilePath);
        SetCurrentAnalysisResult(viewModel);

        var firstResult = viewModel.SaveAnalysisHistory();
        var secondResult = viewModel.SaveAnalysisHistory();

        Assert.True(firstResult.Succeeded);
        Assert.True(viewModel.IsCurrentAnalysisSaved);
        Assert.False(viewModel.CanSaveAnalysisHistory);
        Assert.False(viewModel.SaveAnalysisHistoryCommand.CanExecute(null));
        Assert.False(secondResult.Succeeded);
        Assert.Equal("この分析結果は保存済みです。", secondResult.UserMessage);
        Assert.Equal(2, File.ReadAllLines(historyFilePath).Length);
    }

    [Fact(DisplayName = "結果リセット時は保存済み状態を解除する")]
    public void ResetResults_ClearsCurrentAnalysisSavedState()
    {
        var viewModel = CreateViewModel(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "analysis-history.csv"));
        SetCurrentAnalysisResult(viewModel);
        viewModel.SaveAnalysisHistory();

        viewModel.ResetResults();

        Assert.False(viewModel.IsCurrentAnalysisSaved);
        Assert.False(viewModel.CanSaveAnalysisHistory);
    }

    private static MainWindowViewModel CreateViewModel(string historyFilePath)
    {
        return new MainWindowViewModel(
            new StockAnalysisService(),
            new PriceDataFetchService(),
            new BacktestRunner(),
            new BacktestScoreBandSummaryAggregator(),
            new AnalysisHistoryRecordFactory(),
            new AnalysisHistoryCsvStore(historyFilePath),
            new SymbolHistoryStore(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "symbol-history.json")));
    }

    private static void SetCurrentAnalysisResult(MainWindowViewModel viewModel)
    {
        typeof(MainWindowViewModel)
            .GetProperty(nameof(MainWindowViewModel.CurrentAnalysisResult))!
            .SetValue(viewModel, CreateAnalysisResult());
        typeof(MainWindowViewModel)
            .GetProperty(nameof(MainWindowViewModel.CurrentSymbol))!
            .SetValue(viewModel, "7203.T");
        typeof(MainWindowViewModel)
            .GetProperty(nameof(MainWindowViewModel.CurrentRequestedPeriod))!
            .SetValue(viewModel, "1y");
        typeof(MainWindowViewModel)
            .GetMethod("RaiseStateChanged", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(viewModel, null);
    }

    private static AnalysisResult CreateAnalysisResult()
    {
        var previous = CreateAnalysisBar(new DateOnly(2026, 5, 30), close: 99d, ma75: 100d);
        var current = CreateAnalysisBar(new DateOnly(2026, 5, 31), close: 105.5d, ma75: 101.25d);

        return new AnalysisResult
        {
            Bars = [previous, current],
            Signals =
            [
                new SignalResult
                {
                    Candidate = new SignalCandidate
                    {
                        Type = SignalType.Buy,
                        Previous = previous,
                        Current = current
                    },
                    Evaluation = new SignalEvaluation
                    {
                        Ma75DeviationRate = 0.04197530864197531d,
                        HasVolumeSupport = true,
                        Reasons = ["出来高を伴う上抜け"]
                    },
                    Score = new SignalScore
                    {
                        Total = 80,
                        Rank = SignalRank.Strong
                    }
                }
            ]
        };
    }

    private static AnalysisBar CreateAnalysisBar(DateOnly date, double close, double ma75)
    {
        return new AnalysisBar
        {
            Raw = new PriceBar
            {
                Date = date,
                Open = close,
                High = close,
                Low = close,
                Close = close,
                Volume = 1200
            },
            Ma75 = ma75,
            Avg20Volume = 1000d,
            VolumeRatio = 1.2d
        };
    }
}
