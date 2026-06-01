using StockAnalyzer.ViewModels;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class AsyncRelayCommandTests
{
    [Fact(DisplayName = "AsyncRelayCommandは実行中の多重実行を防ぐ")]
    public void CanExecute_WhileExecuting_ReturnsFalse()
    {
        var gate = new TaskCompletionSource();
        var command = new AsyncRelayCommand(() => gate.Task);

        command.Execute(null);

        Assert.False(command.CanExecute(null));

        gate.SetResult();
    }

    [Fact(DisplayName = "AsyncRelayCommandは例外を通知して実行状態を解除する")]
    public async Task Execute_WhenActionThrows_NotifiesFailureAndClearsExecuting()
    {
        var expected = new InvalidOperationException("failed");
        Exception? actual = null;
        var command = new AsyncRelayCommand(() => throw expected);
        command.ExecutionFailed += (_, ex) => actual = ex;

        command.Execute(null);
        await Task.Yield();

        Assert.Same(expected, actual);
        Assert.True(command.CanExecute(null));
    }
}
