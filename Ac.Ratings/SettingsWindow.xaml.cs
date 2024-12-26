using System.Windows;
using Ac.Ratings.Services;

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

        private void SetRootFolderButton_OnClick(object sender, RoutedEventArgs e) {
            //var folderDialog = new OpenFolderDialog {
            //    Title = "Select cars root folder",
            //    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            //};
            //if (folderDialog.ShowDialog() == true) {
            //    var folderName = folderDialog.FolderName;
            //    MessageBox.Show($"Cars root folder set to: ${folderName}");
            //}
        }

        private void TransferRatingsButton_OnClick(object sender, RoutedEventArgs e) {
            try {
                var decoder = new RatingsDecoder();
                decoder.InitializeRatingsDataFile();
                decoder.InitializeUserRatings();
                decoder.ExportDataFile();

                MessageBox.Show("Ratings exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) {
                MessageBox.Show($"An error occurred during export: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
