using StockAnalyzer.Models;
using StockAnalyzer.Presentation;
using StockAnalyzer.Services;
using StockAnalyzer.Models.Backtest;
using StockAnalyzer.ViewModels;
using System.Text;
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
        private readonly MainWindowViewModel _viewModel = new();
        private readonly SymbolHistoryStore _symbolHistoryStore = new();
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
            ApplyViewModelState();
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

            var period = periodOption.Value;
            var scoreFilterOption = GetSelectedScoreFilterOption();
            var fetchTask = _viewModel.FetchAsync(symbol, period, scoreFilterOption);
            ApplyViewModelState();

            var result = await fetchTask;

            if (result.Succeeded)
            {
                var backtestResult = _viewModel.RefreshBacktest(
                    backtestSettings,
                    scoreFilterOption,
                    _viewModel.StatusText);

                if (backtestResult.Succeeded)
                {
                    UpdateSymbolHistory(symbol);
                }
                else
                {
                    ShowOperationFailure(backtestResult);
                }
            }
            else
            {
                ShowOperationFailure(result);
            }

            ApplyViewModelState();
        }

        private void SaveAnalysisHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var result = _viewModel.SaveAnalysisHistory();
            ApplyViewModelState();

            if (!result.Succeeded)
            {
                ShowOperationFailure(result, "保存エラー");
                return;
            }

            MessageBox.Show(
                result.UserMessage,
                "保存完了",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void RefreshBacktestButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryCreateBacktestSettings(out var backtestSettings))
            {
                return;
            }

            var result = _viewModel.RefreshBacktest(
                backtestSettings,
                GetSelectedScoreFilterOption());
            ApplyViewModelState();

            if (!result.Succeeded)
            {
                ShowOperationFailure(result);
            }
        }

        private void ApplyViewModelState()
        {
            PricesDataGrid.ItemsSource = _viewModel.PriceRows;
            SignalsDataGrid.ItemsSource = _viewModel.SignalRows;
            BacktestSummaryPanel.DataContext = _viewModel.BacktestSummary;
            BacktestScoreBandSummaryDataGrid.ItemsSource = _viewModel.BacktestScoreBandSummaryRows;
            BacktestDataGrid.ItemsSource = _viewModel.BacktestRows;

            BacktestPlaceholderText.Visibility = _viewModel.BacktestSummary is null
                ? Visibility.Visible
                : Visibility.Collapsed;
            BacktestSummaryPanel.Visibility = _viewModel.BacktestSummary is null
                ? Visibility.Collapsed
                : Visibility.Visible;
            BacktestScoreBandSummaryTitle.Visibility = _viewModel.BacktestSummary is null
                ? Visibility.Collapsed
                : Visibility.Visible;
            BacktestScoreBandSummaryDataGrid.Visibility = _viewModel.BacktestSummary is null
                ? Visibility.Collapsed
                : Visibility.Visible;
            BacktestDataGrid.Visibility = _viewModel.BacktestSummary is null
                ? Visibility.Collapsed
                : Visibility.Visible;

            FetchButton.IsEnabled = !_viewModel.IsFetching;
            RefreshBacktestButton.IsEnabled = _viewModel.CanRefreshBacktest;
            SaveAnalysisHistoryButton.IsEnabled = _viewModel.CanSaveAnalysisHistory;
            SymbolComboBox.IsEnabled = !_viewModel.IsFetching;
            PeriodComboBox.IsEnabled = !_viewModel.IsFetching;
            SignalTypeComboBox.IsEnabled = !_viewModel.IsFetching;
            EntryDelayTextBox.IsEnabled = !_viewModel.IsFetching;
            HoldingBarsTextBox.IsEnabled = !_viewModel.IsFetching;
            LoadingBar.Visibility = _viewModel.IsFetching ? Visibility.Visible : Visibility.Collapsed;
            StatusText.Text = _viewModel.StatusText;
            Mouse.OverrideCursor = _viewModel.IsFetching ? Cursors.Wait : null;
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
            if (_viewModel.CurrentAnalysisResult is null)
            {
                return;
            }

            if (!TryCreateBacktestSettings(out var backtestSettings))
            {
                return;
            }

            var scoreFilterOption = GetSelectedScoreFilterOption();
            _viewModel.RefreshSignals(scoreFilterOption);
            var result = _viewModel.RefreshBacktest(
                backtestSettings,
                scoreFilterOption,
                _viewModel.StatusText);
            ApplyViewModelState();

            if (!result.Succeeded)
            {
                ShowOperationFailure(result);
            }
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

        private ScoreFilterOption GetSelectedScoreFilterOption()
        {
            return ScoreFilterComboBox.SelectedItem is ScoreFilterOption option
                ? option
                : ScoreFilterOptions.All.First(x => x.MinimumScore is null);
        }

        private void ShowOperationFailure(
            MainWindowOperationResult result,
            string title = "エラー")
        {
            if (!string.IsNullOrWhiteSpace(result.UserMessage))
            {
                MessageBox.Show(result.UserMessage, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (result.Exception is not null)
            {
                ShowFriendlyError(result.Exception, result.Stderr, result.ExitCode);
                return;
            }
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
