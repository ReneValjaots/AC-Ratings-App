using System.Windows.Controls;

namespace Ac.Ratings.Theme.ModernUI.test
{
    /// <summary>
    /// Loads XAML files using Application.LoadComponent.
    /// </summary>
    public class DefaultContentLoader : IContentLoader {
        public Task<object> LoadContentAsync(Uri uri, CancellationToken cancellationToken) {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            string pageName = uri.OriginalString.TrimStart('/').Replace(".xaml", "");
            string namespacePart = "Ac.Ratings.Theme.ModernUI.test.Pages";
            string typeName = $"{namespacePart}.{pageName}";
            var type = Type.GetType(typeName);
            if (type != null && typeof(UserControl).IsAssignableFrom(type)) {
                return Task.FromResult(Activator.CreateInstance(type));
            }

            return Task.FromResult<object>(null);
        }
    }
}
