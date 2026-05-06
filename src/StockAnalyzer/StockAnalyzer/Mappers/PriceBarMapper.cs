using StockAnalyzer.Models;
using StockAnalyzer.Models.Market;

namespace StockAnalyzer.Mappers;

public static class PriceBarMapper
{
    public static PriceBar From(PriceRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        return new PriceBar
        {
            Date = DateOnly.Parse(row.Date),
            Open = row.Open,
            High = row.High,
            Low = row.Low,
            Close = row.Close,
            Volume = row.Volume
        };
    }
}
