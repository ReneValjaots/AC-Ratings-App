using Ac.Ratings.Core;

namespace Ac.Ratings.Theme.ModernUI.test {
    /// <summary>
    /// Provides a base implementation for objects that are displayed in the UI.
    /// </summary>
    public abstract class Displayable : NotifyPropertyChanged {
        private string displayName;
        public string DisplayName {
            get => displayName;
            set {
                if (displayName != value) {
                    displayName = value;
                    OnPropertyChanged("DisplayName");
                }
            }
        }
    }
}
