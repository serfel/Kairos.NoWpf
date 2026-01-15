using System.Drawing;
using System.IO;

namespace KaiROS.AI.Services;

public interface IThemeService
{
    string CurrentTheme { get; }
    Color BackgroundColor { get; }
    Color SurfaceColor { get; }
    Color SurfaceLightColor { get; }
    Color CardColor { get; }
    Color BorderColor { get; }
    Color TextPrimaryColor { get; }
    Color TextSecondaryColor { get; }
    Color TextMutedColor { get; }
    void SetTheme(string themeName);
    void LoadSavedTheme();
}

public class ThemeService : IThemeService
{
    private readonly string _settingsPath;
    
    public string CurrentTheme { get; private set; } = "Dark";
    public Color BackgroundColor { get; private set; } = Color.FromArgb(15, 15, 35);
    public Color SurfaceColor { get; private set; } = Color.FromArgb(26, 26, 46);
    public Color SurfaceLightColor { get; private set; } = Color.FromArgb(37, 37, 58);
    public Color CardColor { get; private set; } = Color.FromArgb(22, 22, 42);
    public Color BorderColor { get; private set; } = Color.FromArgb(45, 45, 68);
    public Color TextPrimaryColor { get; private set; } = Color.FromArgb(249, 250, 251);
    public Color TextSecondaryColor { get; private set; } = Color.FromArgb(156, 163, 175);
    public Color TextMutedColor { get; private set; } = Color.FromArgb(107, 114, 128);
    
    public ThemeService()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _settingsPath = Path.Combine(localAppData, "KaiROS.AI", "theme.txt");
    }
    
    public void SetTheme(string themeName)
    {
        var isLight = themeName == "Light";
        
        // Set colors based on theme
        BackgroundColor = isLight ? Color.FromArgb(248, 250, 252) : Color.FromArgb(15, 15, 35);
        SurfaceColor = isLight ? Color.FromArgb(255, 255, 255) : Color.FromArgb(26, 26, 46);
        SurfaceLightColor = isLight ? Color.FromArgb(241, 245, 249) : Color.FromArgb(37, 37, 58);
        CardColor = isLight ? Color.FromArgb(255, 255, 255) : Color.FromArgb(22, 22, 42);
        BorderColor = isLight ? Color.FromArgb(226, 232, 240) : Color.FromArgb(45, 45, 68);
        TextPrimaryColor = isLight ? Color.FromArgb(30, 41, 59) : Color.FromArgb(249, 250, 251);
        TextSecondaryColor = isLight ? Color.FromArgb(100, 116, 139) : Color.FromArgb(156, 163, 175);
        TextMutedColor = isLight ? Color.FromArgb(148, 163, 184) : Color.FromArgb(107, 114, 128);
        
        CurrentTheme = themeName;
        
        // Save preference
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            File.WriteAllText(_settingsPath, themeName);
        }
        catch { /* Ignore save errors */ }
    }
    
    public void LoadSavedTheme()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var savedTheme = File.ReadAllText(_settingsPath).Trim();
                if (savedTheme == "Light" || savedTheme == "Dark")
                {
                    SetTheme(savedTheme);
                }
            }
        }
        catch { /* Ignore load errors */ }
    }
}

