namespace StockAnalyzer.Analyzer;

public static class MovingAverageAnalyzer
{
    public static List<double?> CalculateSma(IReadOnlyList<double> values, int period)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(period);

        var result = new List<double?>(values.Count);
        double sum = 0;

        for (int i = 0; i < values.Count; i++)
        {
            sum += values[i];

            if (i >= period)
            {
                sum -= values[i - period];
            }

            result.Add(i >= period - 1 ? sum / period : null);
        }

        return result;
    }
}
