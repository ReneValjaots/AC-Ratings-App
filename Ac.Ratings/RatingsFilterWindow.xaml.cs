﻿using Ac.Ratings.ViewModel;
using System.Windows;
using Ac.Ratings.Theme;
using Ac.Ratings.Theme.ModernUI.Helpers;

namespace Ac.Ratings {
    /// <summary>
    /// Interaction logic for RatingsFilterWindow.xaml
    /// </summary>
    public partial class RatingsFilterWindow : ModernWindowBase {
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
