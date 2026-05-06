using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Models.Market;

namespace StockAnalyzer.Analyzer.Indicators;

public sealed class AnalysisBarBuilder
{
    public IReadOnlyList<AnalysisBar> Build(IReadOnlyList<PriceBar> priceBars)
    {
        ArgumentNullException.ThrowIfNull(priceBars);

        var closes = priceBars.Select(x => x.Close).ToList();
        var volumes = priceBars.Select(x => (double)x.Volume).ToList();

        var ma75 = MovingAverageAnalyzer.CalculateSma(closes, 75);
        var avg20Volume = MovingAverageAnalyzer.CalculateSma(volumes, 20);

        var result = new List<AnalysisBar>(priceBars.Count);

        for (int i = 0; i < priceBars.Count; i++)
        {
            var avgVolume = avg20Volume[i];
            double? volumeRatio = avgVolume is > 0
                ? priceBars[i].Volume / avgVolume.Value
                : null;

            result.Add(new AnalysisBar
            {
                Raw = priceBars[i],
                Ma75 = ma75[i],
                Avg20Volume = avgVolume,
                VolumeRatio = volumeRatio,
                Candle = new CandleMetrics()
            });
        }

        return result;
    }
}
