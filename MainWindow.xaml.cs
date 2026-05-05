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

            bool countSentences = SentencesCheckBox.IsChecked ?? false;
            bool countWords = WordsCheckBox.IsChecked ?? false;
            bool countDigits = DigitsCheckBox.IsChecked ?? false;
            bool countQuestions = QuestionsCheckBox.IsChecked ?? false;
            bool countExclamations = ExclamationsCheckBox.IsChecked ?? false;
            bool resultInFile = ResultInFileCheckBox.IsChecked ?? false;

            string result = await AnalyzeTextAsync(text, token, countSentences, countWords, countDigits, countQuestions, countExclamations);

            if (resultInFile)
            {
                string filePath = "result.txt";

                await File.WriteAllTextAsync(filePath, result, token);

                MessageBox.Show($"Result saved to {filePath}", "Result", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result, "Result", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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

        private async Task<string> AnalyzeTextAsync(string text, CancellationToken token,
            bool countSentences,
            bool countWords,
            bool countDigits,
            bool countQuestions,
            bool countExclamations)
        {
            var sb = new StringBuilder();
            int amountOfSentences = 0;
            int amountOfWords = 0;
            int amountOfDigits = 0;
            int amountOfQuestions = 0;
            int amountOfExclamations = 0;

            if (countSentences)
            {
                amountOfSentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;
            }

            if (countWords)
            {
                amountOfWords = text.Split(new[] { ' ', '\r', '\n', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;
            }

            foreach (var ch in text)
            {
                token.ThrowIfCancellationRequested();

                if (countDigits && char.IsDigit(ch))
                    Interlocked.Increment(ref amountOfDigits);

                if (countQuestions && ch == '?')
                    Interlocked.Increment(ref amountOfQuestions);

                if (countExclamations && ch == '!')
                    Interlocked.Increment(ref amountOfExclamations);

                await Task.Delay(1, token); // імітація важкої роботи, для ефективності можна видалити
            }

            if (countSentences)
                sb.AppendLine($"Sentences: {amountOfSentences}");

            if (countWords)
                sb.AppendLine($"Words: {amountOfWords}");

            if (countDigits)
                sb.AppendLine($"Digits: {amountOfDigits}");

            if (countQuestions)
                sb.AppendLine($"Questions: {amountOfQuestions}");

            if (countExclamations)
                sb.AppendLine($"Exclamations: {amountOfExclamations}");

            return sb.ToString();
        }
    }
}