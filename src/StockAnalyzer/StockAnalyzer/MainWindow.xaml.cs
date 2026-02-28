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

            var psi = new ProcessStartInfo
            {
                FileName = "python",
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
                    MessageBox.Show($"取得に失敗しました:\n{stderr}");
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
                MessageBox.Show($"例外: {ex.Message}");
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
    }
}