using System.Collections.ObjectModel;

namespace Ac.Ratings.Theme.ModernUI.test {
    /// <summary>
    /// Represents an observable collection of links.
    /// </summary>
    public class LinkCollection : ObservableCollection<Link> {
        public LinkCollection() { }
        public LinkCollection(IEnumerable<Link> links) {
            if (links == null) {
                throw new ArgumentNullException("links");
            }

            foreach (var link in links) {
                Add(link);
            }
        }
    }
}
