using StockAnalyzer.Models;
using StockAnalyzer.Models.Analysis;
using StockAnalyzer.Mappers;
using StockAnalyzer.Presentation;
using StockAnalyzer.Services;
using StockAnalyzer.Services.Backtest;
using StockAnalyzer.Models.Backtest;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StockAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly StockAnalysisService _stockAnalysisService = new();
        private readonly BacktestRunner _backtestRunner = new();
        private readonly BacktestScoreBandSummaryAggregator _backtestScoreBandSummaryAggregator = new();
        private readonly SymbolHistoryStore _symbolHistoryStore = new();
        private readonly AnalysisHistoryRecordFactory _analysisHistoryRecordFactory = new();
        private readonly AnalysisHistoryCsvStore _analysisHistoryCsvStore = new();
        private AnalysisResult? _currentAnalysisResult;
        private string _currentSymbol = string.Empty;
        private string _currentRequestedPeriod = string.Empty;
        private readonly IReadOnlyList<SignalTypeOption> _signalTypeOptions =
        [
            new(SignalType.Buy, "買い"),
            new(SignalType.Sell, "売り")
        ];
        public MainWindow()
        {
            InitializeComponent();
            InitializeBacktestSettingsInputs();
            InitializeSymbolHistory();
            ResetResultViews();
        }

        private async void FetchButton_Click(object sender, RoutedEventArgs e)
        {
            var symbol = SymbolHistory.Normalize(SymbolComboBox.Text);
            if (string.IsNullOrWhiteSpace(symbol))
            {
                MessageBox.Show("銘柄を入力してください");
                return;
            }

            SymbolComboBox.Text = symbol;

            if (!TryCreateBacktestSettings(out var backtestSettings))
            {
                return;
            }

            if (PeriodComboBox.SelectedItem is not PeriodOption periodOption)
            {
                MessageBox.Show("取得期間を選択してください。");
                return;
            }

            ResetResultViews();
            SetFetchingState(true, "取得中...");

            var period = periodOption.Value;

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

            string? stderr = null;
            int? exitCode = null;

            try
            {
                using var p = Process.Start(psi) ?? throw new Exception("Process start failed.");

                var stdout = await p.StandardOutput.ReadToEndAsync();
                stderr = await p.StandardError.ReadToEndAsync();
                await p.WaitForExitAsync();
                exitCode = p.ExitCode;

                if (p.ExitCode != 0)
                {
                    SetFetchingState(false, "取得失敗");
                    ShowFriendlyError(new Exception("Python process failed."), stderr, exitCode);
                    return;
                }

                var rows = JsonSerializer.Deserialize<List<PriceRow>>(stdout, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? [];
                rows = [.. rows.OrderBy(r => r.Date)];

                var priceBars = rows.Select(PriceBarMapper.From).ToList();
                var analysisResult = _stockAnalysisService.Analyze(priceBars);
                _currentAnalysisResult = analysisResult;
                _currentSymbol = symbol;
                _currentRequestedPeriod = period;

                PricesDataGrid.ItemsSource = analysisResult.Bars
                    .Select(PriceAnalysisRow.From)
                    .ToList();

                UpdateSignalResults();
                UpdateBacktestResult(backtestSettings);
                UpdateSymbolHistory(symbol);

                SetFetchingState(false, $"取得完了: {rows.Count}件");
            }
            catch (Exception ex)
            {
                _currentAnalysisResult = null;
                _currentSymbol = string.Empty;
                _currentRequestedPeriod = string.Empty;
                ResetResultViews();
                SetFetchingState(false, "取得失敗");
                ShowFriendlyError(ex, stderr, exitCode);
            }
        }

        private void SaveAnalysisHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAnalysisResult is null)
            {
                MessageBox.Show("先に価格データを取得してください。");
                return;
            }

            var records = _analysisHistoryRecordFactory.Create(
                _currentSymbol,
                _currentRequestedPeriod,
                DateTimeOffset.Now,
                Guid.NewGuid().ToString("N"),
                _currentAnalysisResult);

            if (records.Count == 0)
            {
                MessageBox.Show("保存対象のシグナルがありません。");
                return;
            }

            try
            {
                _analysisHistoryCsvStore.Append(records);
                StatusText.Text = $"分析結果を保存しました: {records.Count}件";
                MessageBox.Show(
                    $"分析結果を保存しました。\n\n保存先: {_analysisHistoryCsvStore.FilePath}",
                    "保存完了",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"保存に失敗しました。\n\n--- 詳細 ---\n{ex.Message}",
                    "保存エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RefreshBacktestButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAnalysisResult is null)
            {
                MessageBox.Show("先に価格データを取得してください。");
                return;
            }

            if (!TryCreateBacktestSettings(out var backtestSettings))
            {
                return;
            }

            try
            {
                UpdateBacktestResult(backtestSettings);
                StatusText.Text = "バックテスト結果を更新しました。";
            }
            catch (Exception ex)
            {
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
            RefreshBacktestButton.IsEnabled = !isFetching && _currentAnalysisResult is not null;
            SaveAnalysisHistoryButton.IsEnabled = !isFetching && _currentAnalysisResult is not null;
            SymbolComboBox.IsEnabled = !isFetching;
            PeriodComboBox.IsEnabled = !isFetching;
            SignalTypeComboBox.IsEnabled = !isFetching;
            EntryDelayTextBox.IsEnabled = !isFetching;
            HoldingBarsTextBox.IsEnabled = !isFetching;
            LoadingBar.Visibility = isFetching ? Visibility.Visible : Visibility.Collapsed;
            StatusText.Text = status ?? (isFetching ? "取得中..." : "完了");
            Mouse.OverrideCursor = isFetching ? Cursors.Wait : null;
        }

        private void InitializeBacktestSettingsInputs()
        {
            PeriodComboBox.ItemsSource = PeriodOptions.All;
            PeriodComboBox.SelectedItem = PeriodOptions.All.First(x => x.Value == PeriodOptions.DefaultValue);

            ScoreFilterComboBox.ItemsSource = ScoreFilterOptions.All;
            ScoreFilterComboBox.SelectedItem = ScoreFilterOptions.All.First(x => x.MinimumScore is null);

            SignalTypeComboBox.ItemsSource = _signalTypeOptions;
            SignalTypeComboBox.SelectedItem = _signalTypeOptions.First(x => x.Value == SignalType.Buy);
        }

        private void InitializeSymbolHistory()
        {
            try
            {
                SymbolComboBox.ItemsSource = _symbolHistoryStore.Load();
            }
            catch
            {
                SymbolComboBox.ItemsSource = Array.Empty<string>();
            }
        }

        private void UpdateSymbolHistory(string symbol)
        {
            try
            {
                var symbols = _symbolHistoryStore.Add(symbol);
                SymbolComboBox.ItemsSource = symbols;
                SymbolComboBox.Text = symbol;
            }
            catch
            {
                // 履歴保存に失敗しても、取得済みの分析結果表示は成功扱いにする。
            }
        }

        private void ScoreFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currentAnalysisResult is null)
            {
                return;
            }

            if (!TryCreateBacktestSettings(out var backtestSettings))
            {
                return;
            }

            UpdateSignalResults();
            UpdateBacktestResult(backtestSettings);
        }

        private bool TryCreateBacktestSettings(out BacktestSettings settings)
        {
            settings = default!;

            if (SignalTypeComboBox.SelectedItem is not SignalTypeOption signalTypeOption)
            {
                MessageBox.Show("対象シグナルを選択してください。");
                return false;
            }

            if (!TryParsePositiveInt(EntryDelayTextBox.Text, "エントリーまでの営業日数", out var entryDelayBars))
            {
                return false;
            }

            if (!TryParsePositiveInt(HoldingBarsTextBox.Text, "保有営業日数", out var exitAfterBars))
            {
                return false;
            }

            settings = new BacktestSettings
            {
                TargetSignalType = signalTypeOption.Value,
                EntryDelayBars = entryDelayBars,
                ExitAfterBars = exitAfterBars
            };

            return true;
        }

        private static bool TryParsePositiveInt(string? text, string label, out int value)
        {
            if (!int.TryParse(text, out value) || value < 1)
            {
                MessageBox.Show($"{label}は1以上の整数で入力してください。");
                return false;
            }

            return true;
        }

        private sealed record SignalTypeOption(SignalType Value, string Label);

        private void UpdateSignalResults()
        {
            if (_currentAnalysisResult is null)
            {
                SignalsDataGrid.ItemsSource = null;
                return;
            }

            SignalsDataGrid.ItemsSource = _currentAnalysisResult.Signals
                .Where(x => MatchesScoreFilter(x.Score?.Total))
                .Select(SignalViewRow.From)
                .ToList();
        }

        private void UpdateBacktestResult(BacktestSettings settings)
        {
            if (_currentAnalysisResult is null)
            {
                throw new InvalidOperationException("AnalysisResult is not loaded.");
            }

            var backtestResult = _backtestRunner.Run(_currentAnalysisResult, settings);

            BacktestSummaryPanel.DataContext = BacktestSummaryViewModel.From(backtestResult);
            BacktestScoreBandSummaryDataGrid.ItemsSource = _backtestScoreBandSummaryAggregator
                .Create(backtestResult)
                .Select(BacktestScoreBandSummaryViewRow.From)
                .ToList();
            BacktestDataGrid.ItemsSource = backtestResult.Trades
                .Where(x => MatchesScoreFilter(x.SignalScore?.Total))
                .Select(BacktestViewRow.From)
                .ToList();

            ShowResultViews();
        }

        private bool MatchesScoreFilter(int? score)
        {
            if (ScoreFilterComboBox.SelectedItem is not ScoreFilterOption option)
            {
                return true;
            }

            return ScoreFilter.Matches(score, option.MinimumScore);
        }

        private void ResetResultViews()
        {
            PricesDataGrid.ItemsSource = null;
            SignalsDataGrid.ItemsSource = null;
            BacktestDataGrid.ItemsSource = null;
            BacktestScoreBandSummaryDataGrid.ItemsSource = null;
            BacktestSummaryPanel.DataContext = null;

            BacktestPlaceholderText.Visibility = Visibility.Visible;
            BacktestSummaryPanel.Visibility = Visibility.Collapsed;
            BacktestScoreBandSummaryTitle.Visibility = Visibility.Collapsed;
            BacktestScoreBandSummaryDataGrid.Visibility = Visibility.Collapsed;
            BacktestDataGrid.Visibility = Visibility.Collapsed;
            RefreshBacktestButton.IsEnabled = _currentAnalysisResult is not null;
            SaveAnalysisHistoryButton.IsEnabled = _currentAnalysisResult is not null;
        }

        private void ShowResultViews()
        {
            BacktestPlaceholderText.Visibility = Visibility.Collapsed;
            BacktestSummaryPanel.Visibility = Visibility.Visible;
            BacktestScoreBandSummaryTitle.Visibility = Visibility.Visible;
            BacktestScoreBandSummaryDataGrid.Visibility = Visibility.Visible;
            BacktestDataGrid.Visibility = Visibility.Visible;
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
