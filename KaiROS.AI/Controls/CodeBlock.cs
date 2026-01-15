using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace KaiROS.AI.Controls;

public partial class CodeBlock : UserControl
{
    private string _code = string.Empty;
    private string _language = "code";
    
    private TextBox codeContent;
    private Label languageLabel;
    private Button copyButton;
    private Label copyText;
    private Label copyIcon;
    private Timer resetTimer;

    public CodeBlock()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        // Main panel layout
        this.Size = new Size(400, 200);
        this.BorderStyle = BorderStyle.FixedSingle;
        
        // Language label
        languageLabel = new Label();
        languageLabel.Location = new Point(10, 5);
        languageLabel.Size = new Size(100, 20);
        languageLabel.ForeColor = Color.Gray;
        languageLabel.Font = new Font("Consolas", 8F, FontStyle.Regular);
        languageLabel.Text = _language.ToLower();
        this.Controls.Add(languageLabel);

        // Code content textbox (read-only)
        codeContent = new TextBox();
        codeContent.Location = new Point(10, 30);
        codeContent.Multiline = true;
        codeContent.ScrollBars = ScrollBars.Vertical;
        codeContent.ReadOnly = true;
        codeContent.BackColor = Color.FromArgb(30, 30, 46); // Catppuccin Mocha base
        codeContent.ForeColor = Color.FromArgb(205, 214, 244); // Text color
        codeContent.Font = new Font("Consolas", 9F, FontStyle.Regular);
        codeContent.BorderStyle = BorderStyle.None;
        codeContent.WordWrap = false;
        codeContent.AcceptsReturn = true;
        codeContent.AcceptsTab = true;
        this.Controls.Add(codeContent);

        // Copy button panel
        var copyPanel = new Panel();
        copyPanel.Location = new Point(this.Width - 80, 5);
        copyPanel.Size = new Size(70, 20);
        copyPanel.BackColor = Color.Transparent;
        this.Controls.Add(copyPanel);

        // Copy icon
        copyIcon = new Label();
        copyIcon.Location = new Point(0, 2);
        copyIcon.Size = new Size(16, 16);
        copyIcon.Text = "📋";
        copyIcon.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
        copyPanel.Controls.Add(copyIcon);

        // Copy text
        copyText = new Label();
        copyText.Location = new Point(20, 0);
        copyText.Size = new Size(50, 20);
        copyText.Text = "Copy";
        copyText.ForeColor = Color.White;
        copyText.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
        copyText.Click += CopyButton_Click;
        copyPanel.Controls.Add(copyText);

        // Make entire copy area clickable
        copyPanel.Cursor = Cursors.Hand;
        copyPanel.Click += CopyButton_Click;
        
        // Timer for resetting copy button text
        resetTimer = new Timer();
        resetTimer.Interval = 2000; // 2 seconds
        resetTimer.Tick += ResetTimer_Tick;
        
        // Set tab order
        codeContent.Dock = DockStyle.Fill;
        codeContent.Margin = new Padding(0, 30, 10, 10);
    }

    public string Code
    {
        get => _code;
        set
        {
            _code = value;
            ApplySyntaxHighlighting();
        }
    }
    
    public string CodeLanguage
    {
        get => _language;
        set
        {
            _language = value;
            languageLabel.Text = value.ToLower();
        }
    }
    
    private void CopyButton_Click(object sender, EventArgs e)
    {
        try
        {
            Clipboard.SetText(_code);
            copyText.Text = "Copied!";
            copyIcon.Text = "✓";
            
            // Start timer to reset the button text
            resetTimer.Start();
        }
        catch
        {
            // Ignore clipboard errors
        }
    }
    
    private void ResetTimer_Tick(object sender, EventArgs e)
    {
        copyText.Text = "Copy";
        copyIcon.Text = "📋";
        resetTimer.Stop();
    }
    
    private void ApplySyntaxHighlighting()
    {
        if (string.IsNullOrEmpty(_code))
        {
            codeContent.Text = string.Empty;
            return;
        }

        // Temporarily disable redrawing to reduce flicker
        codeContent.SuspendLayout();
        
        // Clear existing text
        codeContent.Clear();
        
        // Apply syntax highlighting
        var highlightedSegments = GetHighlightedCode(_code, _language);
        foreach (var segment in highlightedSegments)
        {
            codeContent.SelectionStart = codeContent.Text.Length;
            codeContent.SelectionLength = 0;
            codeContent.SelectionColor = segment.Color;
            codeContent.SelectionFont = segment.Font;
            codeContent.AppendText(segment.Text);
        }
        
        // Reset selection
        codeContent.SelectionStart = 0;
        codeContent.SelectionLength = 0;
        codeContent.SelectionColor = codeContent.ForeColor;
        
        codeContent.ResumeLayout();
    }
    
    private struct HighlightedSegment
    {
        public string Text { get; set; }
        public Color Color { get; set; }
        public Font Font { get; set; }
    }
    
    private List<HighlightedSegment> GetHighlightedCode(string code, string language)
    {
        var segments = new List<HighlightedSegment>();
        
        // Define colors (Catppuccin Mocha inspired)
        var keywordColor = Color.FromArgb(203, 166, 247); // Mauve - keywords
        var stringColor = Color.FromArgb(166, 227, 161);  // Green - strings
        var commentColor = Color.FromArgb(108, 112, 134); // Overlay0 - comments
        var numberColor = Color.FromArgb(250, 179, 135);  // Peach - numbers
        var typeColor = Color.FromArgb(137, 180, 250);    // Blue - types
        var functionColor = Color.FromArgb(249, 226, 175); // Yellow - functions
        var defaultColor = Color.FromArgb(205, 214, 244); // Text - default
        
        var boldFont = new Font(codeContent.Font, FontStyle.Bold);
        var regularFont = new Font(codeContent.Font, FontStyle.Regular);
        
        // Language-specific keywords
        var keywords = GetKeywords(language);
        var types = GetTypes(language);
        
        // Process each line
        var lines = code.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            
            // Check for comment
            if (IsComment(line, language))
            {
                segments.Add(new HighlightedSegment
                {
                    Text = line + (i < lines.Length - 1 ? "\n" : ""),
                    Color = commentColor,
                    Font = regularFont
                });
            }
            else
            {
                var tokens = Tokenize(line);
                foreach (var token in tokens)
                {
                    if (keywords.Contains(token.ToLower()) || keywords.Contains(token))
                    {
                        segments.Add(new HighlightedSegment
                        {
                            Text = token,
                            Color = keywordColor,
                            Font = boldFont
                        });
                    }
                    else if (types.Contains(token))
                    {
                        segments.Add(new HighlightedSegment
                        {
                            Text = token,
                            Color = typeColor,
                            Font = regularFont
                        });
                    }
                    else if (IsString(token))
                    {
                        segments.Add(new HighlightedSegment
                        {
                            Text = token,
                            Color = stringColor,
                            Font = regularFont
                        });
                    }
                    else if (IsNumber(token))
                    {
                        segments.Add(new HighlightedSegment
                        {
                            Text = token,
                            Color = numberColor,
                            Font = regularFont
                        });
                    }
                    else if (token.EndsWith("("))
                    {
                        segments.Add(new HighlightedSegment
                        {
                            Text = token.TrimEnd('('),
                            Color = functionColor,
                            Font = regularFont
                        });
                        segments.Add(new HighlightedSegment
                        {
                            Text = "(",
                            Color = defaultColor,
                            Font = regularFont
                        });
                    }
                    else
                    {
                        segments.Add(new HighlightedSegment
                        {
                            Text = token,
                            Color = defaultColor,
                            Font = regularFont
                        });
                    }
                }
                
                if (i < lines.Length - 1)
                {
                    segments.Add(new HighlightedSegment
                    {
                        Text = "\n",
                        Color = defaultColor,
                        Font = regularFont
                    });
                }
            }
        }
        
        return segments;
    }
    
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        // Update copy button position when resized
        if (this.Controls.Contains(languageLabel) && copyText != null)
        {
            var copyPanel = copyText.Parent;
            if (copyPanel != null)
            {
                copyPanel.Location = new Point(this.Width - 80, 5);
            }
        }
    }
}