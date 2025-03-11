namespace Ac.Ratings.Theme.ModernUI.test {
    /// <summary>
    /// Provides data for events related to uri sources.
    /// </summary>
    public class SourceEventArgs(Uri source) : EventArgs {
        public Uri Source { get; private set; } = source;
    }
}
