using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using KaiROS.AI.Controls;
using KaiROS.AI.Services;
using WpfBrush = System.Windows.Media.Brush;

namespace KaiROS.AI.Converters;

/// <summary>
/// Converts message content to a list of UI elements with markdown formatting
/// </summary>
public class MarkdownContentConverter : IValueConverter
{
    private static readonly Regex BoldPattern = new(@"\*\*(.+?)\*\*", RegexOptions.Compiled);
    private static readonly Regex ItalicPattern = new(@"\*(.+?)\*", RegexOptions.Compiled);
    private static readonly Regex InlineCodePattern = new(@"`([^`]+)`", RegexOptions.Compiled);
    private static readonly Regex ListItemPattern = new(@"^[\s]*[-*•]\s+(.+)$", RegexOptions.Multiline | RegexOptions.Compiled);
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string content || string.IsNullOrEmpty(content))
            return new StackPanel();
        
        var panel = new StackPanel();
        var segments = MarkdownParser.Parse(content);
        
        foreach (var segment in segments)
        {
            if (segment.Type == SegmentType.CodeBlock)
            {
                var codeBlock = new CodeBlock
                {
                    Code = segment.Content,
                    CodeLanguage = segment.Language,
                    Margin = new Thickness(0, 4, 0, 4)
                };
                panel.Children.Add(codeBlock);
            }
            else
            {
                // Check for list items
                var lines = segment.Content.Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    var listMatch = ListItemPattern.Match(line);
                    if (listMatch.Success)
                    {
                        // List item with bullet point
                        var itemPanel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, Margin = new Thickness(12, 2, 0, 2) };
                        itemPanel.Children.Add(new TextBlock 
                        { 
                            Text = "•  ", 
                            Foreground = (WpfBrush)System.Windows.Application.Current.Resources["AccentBrush"],
                            FontWeight = FontWeights.Bold
                        });
                        itemPanel.Children.Add(CreateFormattedTextBlock(listMatch.Groups[1].Value.Trim()));
                        panel.Children.Add(itemPanel);
                    }
                    else
                    {
                        panel.Children.Add(CreateFormattedTextBlock(line));
                    }
                }
            }
        }
        
        return panel;
    }
    
    private TextBlock CreateFormattedTextBlock(string text)
    {
        var textBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 2, 0, 2)
        };
        
        // Process text for inline formatting
        var currentIndex = 0;
        var allMatches = new List<(int Index, int Length, string Text, string Type)>();
        
        // Find all bold matches
        foreach (Match match in BoldPattern.Matches(text))
            allMatches.Add((match.Index, match.Length, match.Groups[1].Value, "bold"));
        
        // Find all inline code matches
        foreach (Match match in InlineCodePattern.Matches(text))
            allMatches.Add((match.Index, match.Length, match.Groups[1].Value, "code"));
        
        // Sort by index
        allMatches = allMatches.OrderBy(m => m.Index).ToList();
        
        if (allMatches.Count == 0)
        {
            textBlock.Inlines.Add(new Run(text) 
            { 
                Foreground = (WpfBrush)System.Windows.Application.Current.Resources["TextPrimaryBrush"] 
            });
        }
        else
        {
            foreach (var match in allMatches)
            {
                // Add text before this match
                if (match.Index > currentIndex)
                {
                    textBlock.Inlines.Add(new Run(text.Substring(currentIndex, match.Index - currentIndex))
                    {
                        Foreground = (WpfBrush)System.Windows.Application.Current.Resources["TextPrimaryBrush"]
                    });
                }
                
                // Add formatted content
                if (match.Type == "bold")
                {
                    textBlock.Inlines.Add(new Run(match.Text)
                    {
                        FontWeight = FontWeights.Bold,
                        Foreground = (WpfBrush)System.Windows.Application.Current.Resources["TextPrimaryBrush"]
                    });
                }
                else if (match.Type == "code")
                {
                    textBlock.Inlines.Add(new Run(match.Text)
                    {
                        FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                        Background = (WpfBrush)System.Windows.Application.Current.Resources["SurfaceLightBrush"],
                        Foreground = (WpfBrush)System.Windows.Application.Current.Resources["AccentBrush"]
                    });
                }
                
                currentIndex = match.Index + match.Length;
            }
            
            // Add remaining text
            if (currentIndex < text.Length)
            {
                textBlock.Inlines.Add(new Run(text.Substring(currentIndex))
                {
                    Foreground = (WpfBrush)System.Windows.Application.Current.Resources["TextPrimaryBrush"]
                });
            }
        }
        
        return textBlock;
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

