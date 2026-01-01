using System.IO;
using System.Windows;
using System.Windows.Media;

namespace KaiROS.AI.Services;

public interface IThemeService
{
    string CurrentTheme { get; }
    void SetTheme(string themeName);
    void LoadSavedTheme();
}

public class ThemeService : IThemeService
{
    private readonly string _settingsPath;
    
    public string CurrentTheme { get; private set; } = "Dark";
    
    public ThemeService()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _settingsPath = Path.Combine(localAppData, "KaiROS.AI", "theme.txt");
    }
    
    public void SetTheme(string themeName)
    {
        var app = System.Windows.Application.Current;
        if (app == null) return;
        
        var isLight = themeName == "Light";
        
        // Update brush colors in place
        UpdateBrush(app, "BackgroundBrush", isLight ? System.Windows.Media.Color.FromRgb(248, 250, 252) : System.Windows.Media.Color.FromRgb(15, 15, 35));
        UpdateBrush(app, "SurfaceBrush", isLight ? System.Windows.Media.Color.FromRgb(255, 255, 255) : System.Windows.Media.Color.FromRgb(26, 26, 46));
        UpdateBrush(app, "SurfaceLightBrush", isLight ? System.Windows.Media.Color.FromRgb(241, 245, 249) : System.Windows.Media.Color.FromRgb(37, 37, 58));
        UpdateBrush(app, "CardBrush", isLight ? System.Windows.Media.Color.FromRgb(255, 255, 255) : System.Windows.Media.Color.FromRgb(22, 22, 42));
        UpdateBrush(app, "BorderBrush", isLight ? System.Windows.Media.Color.FromRgb(226, 232, 240) : System.Windows.Media.Color.FromRgb(45, 45, 68));
        UpdateBrush(app, "TextPrimaryBrush", isLight ? System.Windows.Media.Color.FromRgb(30, 41, 59) : System.Windows.Media.Color.FromRgb(249, 250, 251));
        UpdateBrush(app, "TextSecondaryBrush", isLight ? System.Windows.Media.Color.FromRgb(100, 116, 139) : System.Windows.Media.Color.FromRgb(156, 163, 175));
        UpdateBrush(app, "TextMutedBrush", isLight ? System.Windows.Media.Color.FromRgb(148, 163, 184) : System.Windows.Media.Color.FromRgb(107, 114, 128));
        
        CurrentTheme = themeName;
        
        // Save preference
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            File.WriteAllText(_settingsPath, themeName);
        }
        catch { /* Ignore save errors */ }
    }
    
    private static void UpdateBrush(System.Windows.Application app, string key, System.Windows.Media.Color color)
    {
        // Create a new brush and replace the resource (XAML brushes are frozen/read-only)
        app.Resources[key] = new SolidColorBrush(color);
    }
    
    public void LoadSavedTheme()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var savedTheme = File.ReadAllText(_settingsPath).Trim();
                if (savedTheme == "Light")
                {
                    SetTheme("Light");
                }
            }
        }
        catch { /* Ignore load errors */ }
    }
}

