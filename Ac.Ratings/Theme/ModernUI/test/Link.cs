namespace Ac.Ratings.Theme.ModernUI.test {
    /// <summary>
    /// Represents a displayable link.
    /// </summary>
    public class Link : Displayable {
        private Uri source;

        public Uri Source {
            get => source;
            set {
                if (source != value) {
                    source = value;
                    OnPropertyChanged("Source");
                }
            }
        }
    }
}
