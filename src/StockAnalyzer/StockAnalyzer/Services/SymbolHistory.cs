namespace StockAnalyzer.Services;

public static class SymbolHistory
{
    public const int MaxCount = 10;

    public static string Normalize(string symbol)
    {
        return symbol.Trim().ToUpperInvariant();
    }

    public static IReadOnlyList<string> AddOrMoveToFirst(
        IReadOnlyList<string> current,
        string symbol,
        int maxCount = MaxCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCount);

        var normalizedSymbol = Normalize(symbol);
        if (string.IsNullOrWhiteSpace(normalizedSymbol))
        {
            return current
                .Select(Normalize)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(maxCount)
                .ToList();
        }

        return current
            .Select(Normalize)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Where(x => !string.Equals(x, normalizedSymbol, StringComparison.OrdinalIgnoreCase))
            .Prepend(normalizedSymbol)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(maxCount)
            .ToList();
    }

    public static IReadOnlyList<string> NormalizeAll(
        IReadOnlyList<string> symbols,
        int maxCount = MaxCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCount);

        return symbols
            .Select(Normalize)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(maxCount)
            .ToList();
    }
}
