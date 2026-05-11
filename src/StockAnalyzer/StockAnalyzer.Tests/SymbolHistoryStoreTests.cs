using StockAnalyzer.Services;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class SymbolHistoryStoreTests
{
    [Fact(DisplayName = "履歴はJSONファイルに保存して読み込める")]
    public void SaveAndLoad_PersistsSymbolsAsJson()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "symbol-history.json");
        var store = new SymbolHistoryStore(filePath);
        string[] expected = ["AAPL", "MSFT", "7203.T"];

        store.Save(["aapl", "msft", "7203.t"]);

        var symbols = store.Load();

        Assert.Equal(expected, symbols);
    }

    [Fact(DisplayName = "履歴追加は保存済みJSONを更新する")]
    public void Add_UpdatesPersistedJson()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "symbol-history.json");
        var store = new SymbolHistoryStore(filePath);
        string[] expected = ["AAPL", "MSFT"];

        store.Save(["AAPL", "MSFT"]);
        var symbols = store.Add("aapl");
        var loadedSymbols = store.Load();

        Assert.Equal(expected, symbols);
        Assert.Equal(symbols, loadedSymbols);
    }
}
