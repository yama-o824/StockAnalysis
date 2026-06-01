using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Presentation;
using StockAnalyzer.Services;
using StockAnalyzer.ViewModels;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class AnalysisHistoryViewModelTests
{
    [Fact(DisplayName = "履歴読み込みCommandで日付の新しい順に履歴行を表示する")]
    public void LoadHistoryCommand_LoadsRowsInDefaultSortOrder()
    {
        var store = CreateStore(
        [
            CreateRecord("run-1", "7203.T", new DateOnly(2026, 5, 30), SignalType.Buy),
            CreateRecord("run-2", "6758.T", new DateOnly(2026, 5, 31), SignalType.Sell)
        ]);
        var viewModel = new AnalysisHistoryViewModel(store);

        viewModel.LoadHistoryCommand.Execute(null);

        Assert.True(viewModel.HasLoadedHistory);
        Assert.True(viewModel.HasHistoryRows);
        Assert.Equal(2, viewModel.HistoryRows.Count);
        Assert.Equal("6758.T", viewModel.HistoryRows[0].Symbol);
        Assert.Equal("7203.T", viewModel.HistoryRows[1].Symbol);
        Assert.Equal(viewModel.HistoryRows[0], viewModel.SelectedHistoryRow);
        Assert.Equal("分析履歴を読み込みました: 2件", viewModel.StatusText);
    }

    [Fact(DisplayName = "銘柄コードで履歴行を絞り込む")]
    public void SymbolFilterText_FiltersRows()
    {
        var viewModel = new AnalysisHistoryViewModel(CreateStore(
        [
            CreateRecord("run-1", "7203.T", new DateOnly(2026, 5, 30), SignalType.Buy),
            CreateRecord("run-2", "6758.T", new DateOnly(2026, 5, 31), SignalType.Sell)
        ]));
        viewModel.LoadHistory();

        viewModel.SymbolFilterText = "7203";

        Assert.Single(viewModel.HistoryRows);
        Assert.Equal("7203.T", viewModel.HistoryRows[0].Symbol);
        Assert.Equal("表示中: 1件 / 全2件", viewModel.StatusText);
    }

    [Fact(DisplayName = "SignalTypeで履歴行を絞り込む")]
    public void SelectedSignalTypeFilter_FiltersRows()
    {
        var viewModel = new AnalysisHistoryViewModel(CreateStore(
        [
            CreateRecord("run-1", "7203.T", new DateOnly(2026, 5, 30), SignalType.Buy),
            CreateRecord("run-2", "6758.T", new DateOnly(2026, 5, 31), SignalType.Sell)
        ]));
        viewModel.LoadHistory();

        viewModel.SelectedSignalTypeFilter = viewModel.SignalTypeFilterOptions.First(x => x.Value == SignalType.Sell);

        Assert.Single(viewModel.HistoryRows);
        Assert.Equal(SignalType.Sell, viewModel.HistoryRows[0].SignalType);
    }

    [Fact(DisplayName = "日付の古い順で履歴行を並び替える")]
    public void SelectedSortOrder_SortsRowsAscending()
    {
        var viewModel = new AnalysisHistoryViewModel(CreateStore(
        [
            CreateRecord("run-1", "7203.T", new DateOnly(2026, 5, 30), SignalType.Buy),
            CreateRecord("run-2", "6758.T", new DateOnly(2026, 5, 31), SignalType.Sell)
        ]));
        viewModel.LoadHistory();

        viewModel.SelectedSortOrder = viewModel.SortOrderOptions.First(x => x.Value == AnalysisHistorySortOrder.SignalDateAscending);

        Assert.Equal("7203.T", viewModel.HistoryRows[0].Symbol);
        Assert.Equal("6758.T", viewModel.HistoryRows[1].Symbol);
    }

    [Fact(DisplayName = "読み込み失敗時はメッセージを通知する")]
    public void LoadHistory_WhenLoadFails_RequestsMessage()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "analysis-history.csv");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, "InvalidHeader\n");
        var viewModel = new AnalysisHistoryViewModel(new AnalysisHistoryCsvStore(filePath));
        UserMessageRequestedEventArgs? message = null;
        viewModel.MessageRequested += (_, e) => message = e;

        viewModel.LoadHistory();

        Assert.NotNull(message);
        Assert.Equal("履歴読み込みエラー", message.Title);
        Assert.Equal(UserMessageKind.Warning, message.Kind);
        Assert.Equal("分析履歴の読み込みに失敗しました。", viewModel.StatusText);
    }

    [Fact(DisplayName = "読み込み済みの履歴は保存後更新用に再読み込みできる")]
    public void ReloadIfLoaded_WhenAlreadyLoaded_ReloadsRows()
    {
        var store = CreateStore(
        [
            CreateRecord("run-1", "7203.T", new DateOnly(2026, 5, 30), SignalType.Buy)
        ]);
        var viewModel = new AnalysisHistoryViewModel(store);
        viewModel.LoadHistory();
        store.Append(
        [
            CreateRecord("run-2", "6758.T", new DateOnly(2026, 5, 31), SignalType.Sell)
        ]);

        viewModel.ReloadIfLoaded();

        Assert.Equal(2, viewModel.HistoryRows.Count);
        Assert.Equal("6758.T", viewModel.HistoryRows[0].Symbol);
        Assert.Equal("分析履歴を更新しました: 2件", viewModel.StatusText);
    }

    [Fact(DisplayName = "未読み込みの履歴は保存後更新で読み込まない")]
    public void ReloadIfLoaded_WhenNotLoaded_DoesNotLoadRows()
    {
        var viewModel = new AnalysisHistoryViewModel(CreateStore(
        [
            CreateRecord("run-1", "7203.T", new DateOnly(2026, 5, 30), SignalType.Buy)
        ]));

        viewModel.ReloadIfLoaded();

        Assert.Empty(viewModel.HistoryRows);
        Assert.False(viewModel.HasLoadedHistory);
    }

    private static AnalysisHistoryCsvStore CreateStore(IReadOnlyList<AnalysisHistoryRecord> records)
    {
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "analysis-history.csv");
        var store = new AnalysisHistoryCsvStore(filePath);
        store.Append(records);
        return store;
    }

    private static AnalysisHistoryRecord CreateRecord(
        string runId,
        string symbol,
        DateOnly signalDate,
        SignalType signalType)
    {
        return new AnalysisHistoryRecord
        {
            RunId = runId,
            Symbol = symbol,
            ExecutedAt = new DateTimeOffset(2026, 6, 1, 9, 30, 0, TimeSpan.FromHours(9)),
            RequestedPeriod = "1y",
            AnalysisStartDate = new DateOnly(2026, 1, 1),
            AnalysisEndDate = new DateOnly(2026, 6, 1),
            SignalDate = signalDate,
            SignalType = signalType,
            Price = 105.5d,
            Ma75 = 101.25d,
            PrevPrice = 99d,
            PrevMa75 = 100d,
            PrevDiff = -1d,
            CurrentDiff = 4.25d,
            Avg20Volume = 1200d,
            VolumeRatio = 1.25d,
            Ma75DeviationRate = 0.04197530864197531d,
            Score = 80,
            Rank = SignalRank.Strong,
            ScoreBreakdown = "出来高 20/20",
            Reasons = "出来高を伴う上抜け / 強い陽線"
        };
    }
}
