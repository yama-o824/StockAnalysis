using StockAnalyzer.Models;
using StockAnalyzer.Models.Market;

namespace StockAnalyzer.Services;

public sealed class PriceDataFetchResult
{
    public IReadOnlyList<PriceRow> Rows { get; init; } = [];
    public IReadOnlyList<PriceBar> PriceBars { get; init; } = [];
}
