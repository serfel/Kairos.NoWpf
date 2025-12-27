using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using KaiROS.AI.ViewModels;

namespace KaiROS.AI;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Set window icon from file
        try
        {
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "app.ico");
            if (File.Exists(iconPath))
            {
                Icon = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
            }
        }
        catch { /* Ignore icon loading errors */ }
        
        Loaded += async (s, e) =>
        {
            await viewModel.InitializeAsync();
        };
    }
}