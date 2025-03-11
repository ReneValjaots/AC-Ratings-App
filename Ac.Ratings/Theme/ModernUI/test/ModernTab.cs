using System.Windows;
using System.Windows.Controls;

namespace Ac.Ratings.Theme.ModernUI.test {
    /// <summary>
    /// Represents a control that contains multiple pages that share the same space on screen.
    /// </summary>
    public class ModernTab : Control {
        public static readonly DependencyProperty ContentLoaderProperty =
            DependencyProperty.Register(nameof(ContentLoader), typeof(IContentLoader), typeof(ModernTab), new PropertyMetadata(new DefaultContentLoader()));

        public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(nameof(Layout), typeof(TabLayout), typeof(ModernTab), new PropertyMetadata(TabLayout.Tab));

        public static readonly DependencyProperty ListWidthProperty = DependencyProperty.Register(nameof(ListWidth), typeof(GridLength), typeof(ModernTab), new PropertyMetadata(new GridLength(170)));

        public static readonly DependencyProperty LinksProperty = DependencyProperty.Register(nameof(Links), typeof(LinkCollection), typeof(ModernTab), new PropertyMetadata(OnLinksChanged));

        public static readonly DependencyProperty SelectedSourceProperty = DependencyProperty.Register(nameof(SelectedSource), typeof(Uri), typeof(ModernTab), new PropertyMetadata(OnSelectedSourceChanged));

        public event EventHandler<SourceEventArgs> SelectedSourceChanged;

        private ListBox _linkList;

        public ModernTab() {
            DefaultStyleKey = typeof(ModernTab);

            // create a default links collection
            SetCurrentValue(LinksProperty, new LinkCollection());
        }

        private static void OnLinksChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            ((ModernTab)o).UpdateSelection();
        }

        private static void OnSelectedSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            ((ModernTab)o).OnSelectedSourceChanged((Uri)e.OldValue, (Uri)e.NewValue);
        }

        private void OnSelectedSourceChanged(Uri oldValue, Uri newValue) {
            UpdateSelection();

            // raise SelectedSourceChanged event
            var handler = SelectedSourceChanged;
            if (handler != null) {
                handler(this, new SourceEventArgs(newValue));
            }
        }

        private void UpdateSelection() {
            if (_linkList == null || Links == null) {
                return;
            }

            // sync list selection with current source
            _linkList.SelectedItem = Links.FirstOrDefault(l => l.Source == SelectedSource);
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            if (_linkList != null) {
                _linkList.SelectionChanged -= OnLinkListSelectionChanged;
            }

            _linkList = GetTemplateChild("LinkList") as ListBox;
            if (_linkList != null) {
                _linkList.SelectionChanged += OnLinkListSelectionChanged;
            }

            UpdateSelection();
        }

        private void OnLinkListSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (_linkList.SelectedItem is Link link && link.Source != SelectedSource) {
                SetCurrentValue(SelectedSourceProperty, link.Source);
            }
        }

        public IContentLoader ContentLoader {
            get => (IContentLoader)GetValue(ContentLoaderProperty);
            set => SetValue(ContentLoaderProperty, value);
        }

        public TabLayout Layout {
            get => (TabLayout)GetValue(LayoutProperty); 
            set => SetValue(LayoutProperty, value);
        }

        public LinkCollection Links {
            get => (LinkCollection)GetValue(LinksProperty);
            set => SetValue(LinksProperty, value);
        }

        public GridLength ListWidth {
            get => (GridLength)GetValue(ListWidthProperty);
            set => SetValue(ListWidthProperty, value);
        }

        public Uri SelectedSource {
            get => (Uri)GetValue(SelectedSourceProperty);
            set => SetValue(SelectedSourceProperty, value);
        }
    }
}
