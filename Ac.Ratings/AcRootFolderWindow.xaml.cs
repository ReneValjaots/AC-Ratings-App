using System.IO;
using System.Windows;

namespace Ac.Ratings {
    /// <summary>
    /// Interaction logic for AcRootFolderWindow.xaml
    /// </summary>
    public partial class AcRootFolderWindow : Window {
        public string SelectedPath { get; private set; } = string.Empty;
        private bool _isCanceling = false;

        public AcRootFolderWindow() {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            if (Directory.Exists(RootFolderPath.Text)) {
                var rootPath = RootFolderPath.Text;
                var carsPath = Path.Combine(rootPath, "content", "cars");

                if (!Directory.Exists(carsPath)) {
                    MessageBox.Show(
                        "The selected Assetto Corsa root folder does not meet the required folder structure.\n" +
                        "Ensure that the root folder contains a 'content' subfolder with a 'cars' directory inside it.\n",
                        "Invalid Folder Structure", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SelectedPath = carsPath;
                DialogResult = true;
            }
            else {
                MessageBox.Show("The provided path does not exist. Please enter a valid path.", "Invalid path", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            _isCanceling = true; 
            Close(); 
        }

        private bool ConfirmExit() {
            var result = MessageBox.Show(
                "Exiting this window without selecting a valid root folder will close the application. Are you sure?",
                "Confirm Exit", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return result == MessageBoxResult.Yes;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (_isCanceling) {
                if (!ConfirmExit()) {
                    e.Cancel = true;
                    _isCanceling = false;
                    return;
                }

                _isCanceling = false;
            }
            else if (!DialogResult.HasValue) {
                if (!ConfirmExit()) {
                    e.Cancel = true;
                    return;
                }
            }

            if (!DialogResult.HasValue) {
                Environment.Exit(0);
            }
        }
    }
}
