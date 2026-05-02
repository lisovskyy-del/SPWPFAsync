using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SPWPFAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ExecuteButton_OnClick(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                await ExecuteCodeAsync(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(
                    "Analysis cancelled",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }

        private async Task ExecuteCodeAsync(CancellationToken token)
        {
            string text = TextBox.Text;

            if (string.IsNullOrEmpty(text))
            {
                ErrorMessageBox();
                return;
            }

            string result = await AnalyzeTextAsync(text, token);

            string filePath = "result.txt";

            await File.WriteAllTextAsync(filePath, result, token);

            MessageBox.Show($"Result saved to {filePath}");
        }

        private void ErrorMessageBox()
        {
            MessageBox.Show(
                "Text is empty or invalid!",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        private async Task<string> AnalyzeTextAsync(string text, CancellationToken token)
        {
            var sentences = text.Split(new char[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            var amountOfSentences = sentences.Length;
            int amountOfDigits = 0;
            int amountOfQuestions = 0;
            int amountOfExclamations = 0;
            var amountOfWords = text.Split(new char[] { ' ', '\r', '\n', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var ch in text)
            {
                token.ThrowIfCancellationRequested();

                if (char.IsDigit(ch))
                    amountOfDigits++;

                if (char.Equals(ch, '?'))
                    amountOfQuestions++;

                if (char.Equals(ch, '!'))
                    amountOfExclamations++;

                await Task.Delay(1, token); // імітація важкої роботи, для ефективності можна видалити
            }

            return
                $"Amount of sentences: {amountOfSentences}\n" +
                $"Amount of digits: {amountOfDigits}\n" +
                $"Amount of words: {amountOfWords.Length}\n" +
                $"Amount of questions: {amountOfQuestions}\n" +
                $"Amount of exclamations: {amountOfExclamations}";
        }
    }
}