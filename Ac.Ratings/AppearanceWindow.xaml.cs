using Ac.Ratings.Theme.ModernUI.Helpers;
using Ac.Ratings.ViewModel;

namespace Ac.Ratings {
    /// <summary>
    /// Interaction logic for AppearanceSettingsWindow.xaml
    /// </summary>
    public partial class AppearanceWindow : ModernWindowBase {
        public AppearanceWindow() {
            InitializeComponent();
            DataContext = new AppearanceViewModel();
        }
    }
}
