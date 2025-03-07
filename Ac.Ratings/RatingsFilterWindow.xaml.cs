using Ac.Ratings.ViewModel;
using System.Windows;

namespace Ac.Ratings
{
    /// <summary>
    /// Interaction logic for RatingsFilterWindow.xaml
    /// </summary>
    public partial class RatingsFilterWindow : Window
    {
        public RatingsFilterViewModel ViewModel { get; }

        public RatingsFilterWindow(RatingsFilterViewModel viewModel) {
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = ViewModel;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }
    }
}
