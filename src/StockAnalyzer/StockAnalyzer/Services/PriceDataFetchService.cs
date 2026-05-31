using StockAnalyzer.Mappers;
using StockAnalyzer.Models;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace StockAnalyzer.Services;

public sealed class PriceDataFetchService
{
    public async Task<PriceDataFetchResult> FetchAsync(string symbol, string period)
    {
        var fetcherDir = FindFetcherDir();
        var scriptPath = Path.Combine(fetcherDir, "fetch_price_data.py");
        var venvPython = Path.Combine(fetcherDir, ".venv", "Scripts", "python.exe");
        var pythonExe = File.Exists(venvPython) ? venvPython : "python";

        var psi = new ProcessStartInfo
        {
            FileName = pythonExe,
            Arguments = $"\"{scriptPath}\" {symbol} {period}",
            WorkingDirectory = fetcherDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        using var process = Process.Start(psi) ?? throw new Exception("Process start failed.");

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new PriceDataFetchException("Python process failed.", stderr, process.ExitCode);
        }

        var rows = JsonSerializer.Deserialize<List<PriceRow>>(stdout, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];
        rows = [.. rows.OrderBy(r => r.Date)];

        return new PriceDataFetchResult
        {
            Rows = rows,
            PriceBars = rows.Select(PriceBarMapper.From).ToList()
        };
    }

    private static string FindFetcherDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "tools", "fetcher", "fetch_price_data.py");
            if (File.Exists(candidate))
            {
                return Path.GetDirectoryName(candidate)!;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("tools/fetcher/fetch_price_data.py が見つかりません。");
    }
}
