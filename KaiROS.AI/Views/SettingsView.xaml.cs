using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using KaiROS.AI.ViewModels;

namespace KaiROS.AI.Views;

public partial class SettingsView : System.Windows.Controls.UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }
    
    private void OpenApiUrl_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && vm.IsApiEnabled)
        {
            var url = $"http://localhost:{vm.ApiPort}/";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
    
    private void FeedbackHub_Click(object sender, RoutedEventArgs e)
    {
        // Open Feedback Hub with app context
        var feedbackUri = "feedback-hub:?appid=34488AvnishKumar.KaiROSAI_gph07xvrc9pap";
        try
        {
            Process.Start(new ProcessStartInfo(feedbackUri) { UseShellExecute = true });
        }
        catch
        {
            // Fallback: open Microsoft Store feedback page or email
            Process.Start(new ProcessStartInfo("mailto:support@kairosai.app?subject=KaiROS AI Feedback") { UseShellExecute = true });
        }
    }
}

