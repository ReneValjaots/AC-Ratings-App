namespace Ac.Ratings.Theme.ModernUI.test {
    /// <summary>
    /// The contract for loading content.
    /// </summary>
    public interface IContentLoader {
        Task<object> LoadContentAsync(Uri uri, CancellationToken cancellationToken);
    }
}
