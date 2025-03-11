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
    /// Interaction logic for RootFolder.xaml
    /// </summary>
    public partial class RootFolder : UserControl
    {
        public RootFolder()
        {
            InitializeComponent();
        }

        private void ResetRootFolder_Click(object sender, System.Windows.RoutedEventArgs e) {
            var window = Window.GetWindow(this) as SettingsWindow;
            if (window != null) {
                window.ResetRootFolder_Click(sender, e);
            }
        }
    }
}
