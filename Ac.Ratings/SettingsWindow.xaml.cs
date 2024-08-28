using System.Windows;

namespace Ac.Ratings {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window {
        private readonly MainWindow _mainWindow;
        public SettingsWindow(MainWindow mainWindow) {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void ResetRatingsButton_Click(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show(
                "Are you sure you want to reset all ratings? This action cannot be undone.",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes) {
                _mainWindow.ResetAllRatingsInDatabase();

                MessageBox.Show("All ratings have been reset successfully.", "Reset Complete", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else {
                MessageBox.Show("Reset operation canceled.", "Cancel", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}
