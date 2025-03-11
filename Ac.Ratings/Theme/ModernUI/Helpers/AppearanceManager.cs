using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Ac.Ratings.Core;

namespace Ac.Ratings.Theme.ModernUI.Helpers
{
    /// <summary>
    /// Manages the theme, font size and accent colors for a Modern UI application.
    /// </summary>
    public class AppearanceManager
        : NotifyPropertyChanged
    {
        /// <summary>
        /// The location of the dark theme resource dictionary.
        /// </summary>
        public static readonly Uri DarkThemeSource = new Uri("Theme/ModernUI/Assets/ModernUI.Dark.xaml", UriKind.Relative);
        /// <summary>
        /// The location of the light theme resource dictionary.
        /// </summary>
        public static readonly Uri LightThemeSource = new Uri("Theme/ModernUI/Assets/ModernUI.Light.xaml", UriKind.Relative);
        public static readonly Uri BlackThemeSource = new Uri("Theme/ModernUI/Assets/ModernUI.Black.xaml", UriKind.Relative);

        /// <summary>
        /// The resource key for the accent color.
        /// </summary>
        public const string KeyAccentColor = "AccentColor";
        /// <summary>
        /// The resource key for the accent brush.
        /// </summary>
        public const string KeyAccent = "Accent";

        private static AppearanceManager current = new AppearanceManager();

        /// <summary>
        /// Initializes a new instance of the <see cref="AppearanceManager"/> class.
        /// </summary>
        private AppearanceManager()
        {
            DarkThemeCommand = new RelayCommandModern(o => ThemeSource = DarkThemeSource, o => !DarkThemeSource.Equals(ThemeSource));
            LightThemeCommand = new RelayCommandModern(o => ThemeSource = LightThemeSource, o => !LightThemeSource.Equals(ThemeSource));
            BlackThemeCommand = new RelayCommandModern(o => ThemeSource = BlackThemeSource, o => !BlackThemeSource.Equals(ThemeSource));
            //SetThemeCommand = new RelayCommandModern(o => {
            //    var uri = NavigationHelper.ToUri(o);
            //    if (uri != null) {
            //        ThemeSource = uri;
            //    }
            //}, o => o is Uri || o is string);
            AccentColorCommand = new RelayCommandModern(o => {
                if (o is Color) {
                    AccentColor = (Color)o;
                }
                else {
                    // parse color from string
                    var str = o as string;
                    if (str != null) {
                        AccentColor = (Color)ColorConverter.ConvertFromString(str);
                    }
                }
            }, o => o is Color || o is string);
        }

        private ResourceDictionary GetThemeDictionary()
        {
            // determine the current theme by looking at the app resources and return the first dictionary having the resource key 'WindowBackground' defined.
            return (from dict in Application.Current.Resources.MergedDictionaries
                    where dict.Contains("WindowBackground")
                    select dict).FirstOrDefault();
        }

        private Uri GetThemeSource()
        {
            var dict = GetThemeDictionary();
            if (dict != null) {
                return dict.Source;
            }

            // could not determine the theme dictionary
            return null;
        }

        private void SetThemeSource(Uri source, bool useThemeAccentColor)
        {
            if (source == null) {
                throw new ArgumentNullException("source");
            }

            var oldThemeDict = GetThemeDictionary();
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            var themeDict = new ResourceDictionary { Source = source };

            // if theme defines an accent color, use it
            var accentColor = themeDict[KeyAccentColor] as Color?;
            if (accentColor.HasValue) {
                // remove from the theme dictionary and apply globally if useThemeAccentColor is true
                themeDict.Remove(KeyAccentColor);

                if (useThemeAccentColor) {
                    ApplyAccentColor(accentColor.Value);
                }
            }

            // add new before removing old theme to avoid dynamicresource not found warnings
            dictionaries.Add(themeDict);

            // remove old theme
            if (oldThemeDict != null) {
                dictionaries.Remove(oldThemeDict);
            }

            OnPropertyChanged("ThemeSource");
        }

        private void ApplyAccentColor(Color accentColor)
        {
            // set accent color and brush resources
            Application.Current.Resources[KeyAccentColor] = accentColor;
            Application.Current.Resources[KeyAccent] = new SolidColorBrush(accentColor);
        }

        private Color GetAccentColor()
        {
            var accentColor = Application.Current.Resources[KeyAccentColor] as Color?;

            if (accentColor.HasValue) {
                return accentColor.Value;
            }

            // default color: teal
            return Color.FromArgb(0xff, 0x1b, 0xa1, 0xe2);
        }

        private void SetAccentColor(Color value)
        {
            ApplyAccentColor(value);

            // re-apply theme to ensure brushes referencing AccentColor are updated
            var themeSource = GetThemeSource();
            if (themeSource != null) {
                SetThemeSource(themeSource, false);
            }

            OnPropertyChanged("AccentColor");
        }

        /// <summary>
        /// Gets the current <see cref="AppearanceManager"/> instance.
        /// </summary>
        public static AppearanceManager Current
        {
            get { return current; }
        }

        public ICommand DarkThemeCommand { get; private set; }
        public ICommand LightThemeCommand { get; private set; }
        public ICommand BlackThemeCommand { get; private set; }
        public ICommand SetThemeCommand { get; private set; }
        /// <summary>
        /// The command that sets the accent color.
        /// </summary>
        public ICommand AccentColorCommand { get; private set; }

        /// <summary>
        /// Gets or sets the current theme source.
        /// </summary>
        public Uri ThemeSource
        {
            get { return GetThemeSource(); }
            set { SetThemeSource(value, true); }
        }

        /// <summary>
        /// Gets or sets the accent color.
        /// </summary>
        public Color AccentColor
        {
            get { return GetAccentColor(); }
            set { SetAccentColor(value); }
        }
    }
}
