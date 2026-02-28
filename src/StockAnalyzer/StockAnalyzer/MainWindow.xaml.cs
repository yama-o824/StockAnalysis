using StockAnalyzer.Models;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace StockAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void FetchButton_Click(object sender, RoutedEventArgs e)
        {
            var symbol = SymbolTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(symbol))
            {
                MessageBox.Show("銘柄を入力してください");
                return;
            }

            SetFetchingState(true, "取得中...");

            var period = "1y";

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

            try
            {
                using var p = Process.Start(psi) ?? throw new Exception("Process start failed.");

                var stdout = await p.StandardOutput.ReadToEndAsync();
                var stderr = await p.StandardError.ReadToEndAsync();
                await p.WaitForExitAsync();

                if (p.ExitCode != 0)
                {
                    SetFetchingState(false, "取得失敗");
                    ShowFriendlyError(new Exception("Python process failed."), stderr, p.ExitCode);
                    return;
                }

                var rows = JsonSerializer.Deserialize<List<PriceRow>>(stdout, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? [];

                PricesDataGrid.ItemsSource = rows;
                SetFetchingState(false, $"取得完了: {rows.Count}件");
            }
            catch (Exception ex)
            {
                SetFetchingState(false, "取得失敗");
                ShowFriendlyError(ex);
            }
        }

        static string FindFetcherDir()
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

        private void SetFetchingState(bool isFetching, string? status = null)
        {
            FetchButton.IsEnabled = !isFetching;
            LoadingBar.Visibility = isFetching ? Visibility.Visible : Visibility.Collapsed;
            StatusText.Text = status ?? (isFetching ? "取得中..." : "完了");
            Mouse.OverrideCursor = isFetching ? Cursors.Wait : null;
        }

        private void ShowFriendlyError(Exception ex, string? stderr = null, int? exitCode = null)
        {
            var summary = "取得に失敗しました。";

            if (!string.IsNullOrWhiteSpace(stderr))
            {
                if (stderr.Contains("No data returned", StringComparison.OrdinalIgnoreCase))
                    summary = "データが取得できませんでした。銘柄コードが正しいか確認してください。";
                else if (stderr.Contains("Too Many Requests", StringComparison.OrdinalIgnoreCase) ||
                         stderr.Contains("Rate limit", StringComparison.OrdinalIgnoreCase) ||
                         stderr.Contains("429"))
                    summary = "アクセスが集中しています。時間を置いて再実行してください。";
                else if (stderr.Contains("ModuleNotFoundError", StringComparison.OrdinalIgnoreCase))
                    summary = "Python側のライブラリが不足しています。requirements.txt をインストールしてください。";
            }

            var details = new StringBuilder();
            if (exitCode != null) details.AppendLine($"ExitCode: {exitCode}");
            if (!string.IsNullOrWhiteSpace(stderr)) details.AppendLine(stderr.Trim());
            details.AppendLine(ex.Message);

            MessageBox.Show($"{summary}\n\n--- 詳細 ---\n{details}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
