using StockAnalyzer.ViewModels;
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

    [Fact(DisplayName = "取得終了時は取得中状態を解除してステータスを更新する")]
    public void EndFetch_ClearsFetchingStateAndUpdatesStatus()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.BeginFetch();
        viewModel.EndFetch("取得失敗");

        Assert.False(viewModel.IsFetching);
        Assert.Equal("取得失敗", viewModel.StatusText);
    }
}
