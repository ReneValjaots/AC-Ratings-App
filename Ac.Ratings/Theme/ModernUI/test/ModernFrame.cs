using System.Windows;
using System.Windows.Controls;

namespace Ac.Ratings.Theme.ModernUI.test
{
    /// <summary>
    /// A simple content frame implementation with navigation support.
    /// </summary>
    public class ModernFrame : ContentControl {
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(Uri), typeof(ModernFrame), new PropertyMetadata(OnSourceChanged));

        public static readonly DependencyProperty ContentLoaderProperty =
            DependencyProperty.Register(nameof(ContentLoader), typeof(IContentLoader), typeof(ModernFrame), new PropertyMetadata(null));

        public Uri Source {
            get => (Uri)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public IContentLoader ContentLoader {
            get => (IContentLoader)GetValue(ContentLoaderProperty);
            set => SetValue(ContentLoaderProperty, value);
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var frame = (ModernFrame)d;
            frame.LoadContent((Uri)e.NewValue);
        }

        private async void LoadContent(Uri source) {
            if (source == null) {
                System.Diagnostics.Debug.WriteLine("ModernFrame: Source is null");
                return;
            }

            var loader = ContentLoader ?? new DefaultContentLoader();
            var content = await loader.LoadContentAsync(source, CancellationToken.None);
            if (content == null) {
                System.Diagnostics.Debug.WriteLine($"ModernFrame: Failed to load content for URI: {source.OriginalString}");
            }
            else {
                System.Diagnostics.Debug.WriteLine($"ModernFrame: Successfully loaded content for URI: {source.OriginalString}");
            }

            Content = content;
        }
    }
}
