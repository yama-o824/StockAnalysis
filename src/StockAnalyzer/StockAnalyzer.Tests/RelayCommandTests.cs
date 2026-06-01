using StockAnalyzer.ViewModels;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class RelayCommandTests
{
    [Fact(DisplayName = "RelayCommandは実行可否を判定する")]
    public void CanExecute_ReturnsPredicateResult()
    {
        var canExecute = false;
        var command = new RelayCommand(() => { }, () => canExecute);

        Assert.False(command.CanExecute(null));

        canExecute = true;

        Assert.True(command.CanExecute(null));
    }

    [Fact(DisplayName = "RelayCommandは処理を実行する")]
    public void Execute_RunsAction()
    {
        var executed = false;
        var command = new RelayCommand(() => executed = true);

        command.Execute(null);

        Assert.True(executed);
    }
}
