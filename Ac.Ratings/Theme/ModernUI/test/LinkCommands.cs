using System.Windows.Input;

namespace Ac.Ratings.Theme.ModernUI.test {
    /// <summary>
    /// The routed link commands.
    /// </summary>
    public static class LinkCommands {
        private static RoutedUICommand navigateLink = new RoutedUICommand("Navigate Link", "NavigateLink", typeof(LinkCommands));

        public static RoutedUICommand NavigateLink => navigateLink;
    }
}
