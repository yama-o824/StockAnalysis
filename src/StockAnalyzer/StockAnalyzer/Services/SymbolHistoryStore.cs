using System.IO;
using System.Text.Json;

namespace StockAnalyzer.Services;

public sealed class SymbolHistoryStore
{
    private readonly string _filePath;

    public SymbolHistoryStore()
        : this(CreateDefaultFilePath())
    {
    }

    public SymbolHistoryStore(string filePath)
    {
        _filePath = filePath;
    }

    public IReadOnlyList<string> Load()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = File.ReadAllText(_filePath);
        var symbols = JsonSerializer.Deserialize<List<string>>(json) ?? [];

        return SymbolHistory.NormalizeAll(symbols);
    }

    public IReadOnlyList<string> Add(string symbol)
    {
        var symbols = SymbolHistory.AddOrMoveToFirst(Load(), symbol);
        Save(symbols);

        return symbols;
    }

    public void Save(IReadOnlyList<string> symbols)
    {
        var directoryPath = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var normalizedSymbols = SymbolHistory.NormalizeAll(symbols);

        var json = JsonSerializer.Serialize(normalizedSymbols, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_filePath, json);
    }

    private static string CreateDefaultFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appDataPath, "StockAnalyzer", "symbol-history.json");
    }
}
