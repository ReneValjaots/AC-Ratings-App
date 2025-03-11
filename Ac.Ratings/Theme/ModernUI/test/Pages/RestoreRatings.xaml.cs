using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Ac.Ratings.Theme.ModernUI.test.Pages
{
    /// <summary>
    /// Interaction logic for RestoreRatings.xaml
    /// </summary>
    public partial class RestoreRatings : UserControl
    {
        public RestoreRatings()
        {
            InitializeComponent();
        }

        private void RestoreBackupButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            var window = Window.GetWindow(this) as SettingsWindow;
            if (window != null) {
                window.RestoreBackupButton_Click(sender, e);
            }
        }
    }
}
