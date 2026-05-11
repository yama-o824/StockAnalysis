using StockAnalyzer.Services;
using Xunit;

namespace StockAnalyzer.Tests;

public sealed class SymbolHistoryTests
{
    [Fact(DisplayName = "履歴は最大10件に制限される")]
    public void AddOrMoveToFirst_LimitsHistoryToTenSymbols()
    {
        var current = Enumerable.Range(1, 10)
            .Select(x => $"TEST{x}")
            .ToArray();

        var symbols = SymbolHistory.AddOrMoveToFirst(current, "new");

        Assert.Equal(SymbolHistory.MaxCount, symbols.Count);
        Assert.Equal("NEW", symbols[0]);
        Assert.DoesNotContain("TEST10", symbols);
    }

    [Fact(DisplayName = "重複する銘柄は先頭へ移動する")]
    public void AddOrMoveToFirst_MovesDuplicateSymbolToFirst()
    {
        string[] current = ["AAPL", "MSFT", "7203.T"];
        string[] expected = ["MSFT", "AAPL", "7203.T"];

        var symbols = SymbolHistory.AddOrMoveToFirst(current, "msft");

        Assert.Equal(expected, symbols);
    }

    [Fact(DisplayName = "銘柄コードは大文字に正規化される")]
    public void Normalize_ConvertsSymbolToUpperCase()
    {
        var symbol = SymbolHistory.Normalize(" 7203.t ");

        Assert.Equal("7203.T", symbol);
    }
}
