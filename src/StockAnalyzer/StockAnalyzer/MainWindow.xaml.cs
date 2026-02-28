using StockAnalyzer.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Windows;

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

            // v0.1は期間固定
            var period = "1y";

            // tools/fetcher を基準に実行
            var fetcherDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..\tools\fetcher"));
            var scriptPath = System.IO.Path.Combine(fetcherDir, "fetch_price_data.py");

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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"例外: {ex.Message}");
            }
        }
    }
}