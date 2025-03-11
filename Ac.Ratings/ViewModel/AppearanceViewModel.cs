using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using Ac.Ratings.Core;
using Ac.Ratings.Theme.ModernUI.Helpers;

namespace Ac.Ratings.ViewModel;

public class AppearanceViewModel : ObservableObject {
    private const string FontSmall = "small";
    private const string FontLarge = "large";
    private const string PaletteMetro = "metro";
    private const string PaletteWP = "windows phone";

    private readonly Color[] _metroAccentColors = new Color[]
    {
            Color.FromRgb(0x33, 0x99, 0xff), // blue
            Color.FromRgb(0x00, 0xab, 0xa9), // teal
            Color.FromRgb(0x33, 0x99, 0x33), // green
            Color.FromRgb(0x8c, 0xbf, 0x26), // lime
            Color.FromRgb(0xf0, 0x96, 0x09), // orange
            Color.FromRgb(0xff, 0x45, 0x00), // orange red
            Color.FromRgb(0xe5, 0x14, 0x00), // red
            Color.FromRgb(0xff, 0x00, 0x97), // magenta
            Color.FromRgb(0xa2, 0x00, 0xff), // purple
    };

    private readonly Color[] _wpAccentColors = new Color[]
    {
            Color.FromRgb(0xa4, 0xc4, 0x00), // lime
            Color.FromRgb(0x60, 0xa9, 0x17), // green
            Color.FromRgb(0x00, 0x8a, 0x00), // emerald
            Color.FromRgb(0x00, 0xab, 0xa9), // teal
            Color.FromRgb(0x1b, 0xa1, 0xe2), // cyan
            Color.FromRgb(0x00, 0x50, 0xef), // cobalt
            Color.FromRgb(0x6a, 0x00, 0xff), // indigo
            Color.FromRgb(0xaa, 0x00, 0xff), // violet
            Color.FromRgb(0xf4, 0x72, 0xd0), // pink
            Color.FromRgb(0xd8, 0x00, 0x73), // magenta
            Color.FromRgb(0xa2, 0x00, 0x25), // crimson
            Color.FromRgb(0xe5, 0x14, 0x00), // red
            Color.FromRgb(0xfa, 0x68, 0x00), // orange
            Color.FromRgb(0xf0, 0xa3, 0x0a), // amber
            Color.FromRgb(0xe3, 0xc8, 0x00), // yellow
            Color.FromRgb(0x82, 0x5a, 0x2c), // brown
            Color.FromRgb(0x6d, 0x87, 0x64), // olive
            Color.FromRgb(0x64, 0x76, 0x87), // steel
            Color.FromRgb(0x76, 0x60, 0x8a), // mauve
            Color.FromRgb(0x87, 0x79, 0x4e), // taupe
    };

    private string _selectedPalette = PaletteWP;
    private Color _selectedAccentColor;
    private ObservableCollection<Link> _themes = new ObservableCollection<Link>();
    private Link _selectedTheme;
    private string _selectedFontSize;

    public AppearanceViewModel() {
        _themes.Add(new Link { DisplayName = "dark", Source = AppearanceManager.DarkThemeSource });
        _themes.Add(new Link { DisplayName = "black", Source = AppearanceManager.BlackThemeSource });
        _themes.Add(new Link { DisplayName = "light", Source = AppearanceManager.LightThemeSource });

        _selectedFontSize = "small"; // Default, no font size management in your AppearanceManager yet
        SyncThemeAndColor();

        // Subscribe to AppearanceManager changes
        PropertyChangedEventManager.AddHandler(AppearanceManager.Current, OnAppearanceManagerPropertyChanged, nameof(AppearanceManager.AccentColor));
        PropertyChangedEventManager.AddHandler(AppearanceManager.Current, OnAppearanceManagerPropertyChanged, nameof(AppearanceManager.ThemeSource));
    }

    private void SyncThemeAndColor() {
        _selectedTheme = _themes.FirstOrDefault(l => l.Source.Equals(AppearanceManager.Current.AccentColor)); // Simplified for your case
        _selectedAccentColor = AppearanceManager.Current.AccentColor;
        OnPropertyChanged(nameof(SelectedTheme));
        OnPropertyChanged(nameof(SelectedAccentColor));
    }

    private void OnAppearanceManagerPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(AppearanceManager.AccentColor) ||
            e.PropertyName == nameof(AppearanceManager.ThemeSource)) {
            SyncThemeAndColor();
        }
    }

    public ObservableCollection<Link> Themes {
        get => _themes;
    }

    public string[] FontSizes {
        get => new[] { FontSmall, FontLarge };
    }

    public string[] Palettes {
        get => new[] { PaletteMetro, PaletteWP };
    }

    public Color[] AccentColors {
        get => _selectedPalette == PaletteMetro ? _metroAccentColors : _wpAccentColors;
    }

    public string SelectedPalette {
        get => _selectedPalette;
        set {
            if (_selectedPalette != value) {
                _selectedPalette = value;
                OnPropertyChanged(nameof(SelectedPalette));
                OnPropertyChanged(nameof(AccentColors));
                SelectedAccentColor = AccentColors.FirstOrDefault();
            }
        }
    }

    public Link SelectedTheme {
        get => _selectedTheme;
        set {
            if (_selectedTheme != value) {
                _selectedTheme = value;
                OnPropertyChanged(nameof(SelectedTheme));
                // Update theme if you implement theme switching in AppearanceManager
                AppearanceManager.Current.ThemeSource = value.Source;
            }
        }
    }

    public string SelectedFontSize {
        get => _selectedFontSize;
        set {
            if (_selectedFontSize != value) {
                _selectedFontSize = value;
                OnPropertyChanged(nameof(SelectedFontSize));
                // Add font size logic to AppearanceManager if needed
            }
        }
    }

    public Color SelectedAccentColor {
        get => _selectedAccentColor;
        set {
            if (_selectedAccentColor != value) {
                _selectedAccentColor = value;
                OnPropertyChanged(nameof(SelectedAccentColor));
                AppearanceManager.Current.AccentColor = value;
            }
        }
    }
}

// Simple Link class (replace if you have your own)
public class Link : ObservableObject {
    private string _displayName;
    private Uri _source;

    public string DisplayName {
        get => _displayName;
        set => SetField(ref _displayName, value);
    }

    public Uri Source {
        get => _source;
        set => SetField(ref _source, value);
    }
}
