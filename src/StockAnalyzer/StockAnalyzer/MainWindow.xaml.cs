using StockAnalyzer.ViewModels;
using System.Text;
using System.Windows;

namespace StockAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel = new();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
            _viewModel.MessageRequested += ViewModel_MessageRequested;
            _viewModel.OperationFailed += ViewModel_OperationFailed;
            _viewModel.LoadSymbolHistory();
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

        private void ViewModel_MessageRequested(object? sender, UserMessageRequestedEventArgs e)
        {
            MessageBox.Show(
                e.Message,
                e.Title,
                MessageBoxButton.OK,
                ToMessageBoxImage(e.Kind));
        }

        private void ViewModel_OperationFailed(object? sender, MainWindowOperationResult e)
        {
            ShowOperationFailure(e);
        }

        private static MessageBoxImage ToMessageBoxImage(UserMessageKind kind)
        {
            return kind switch
            {
                UserMessageKind.Information => MessageBoxImage.Information,
                UserMessageKind.Warning => MessageBoxImage.Warning,
                UserMessageKind.Error => MessageBoxImage.Error,
                _ => MessageBoxImage.None
            };
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
