using System;
using System.Drawing;
using System.Windows.Forms;
using KaiROS.AI.ViewModels;

namespace KaiROS.AI.Views
{
    public partial class ChatForm : Form
    {
        private ChatViewModel _viewModel;
        private Panel sidebarPanel;
        private Panel mainPanel;
        private Panel headerPanel;
        private TextBox messageInput;
        private RichTextBox messagesDisplay;
        private Button sendButton;
        private Button stopButton;
        private Button clearButton;
        private Button exportButton;
        private Button newChatButton;
        private ListBox sessionsListBox;
        private Label activeModelLabel;
        private Panel statsPanel;
        private Label tokensPerSecondLabel;
        private Label totalTokensLabel;
        private Label memoryUsageLabel;
        private Label elapsedTimeLabel;
        private Label contextWindowLabel;
        private Label gpuLayersLabel;
        private Timer uiUpdateTimer;

        public ChatForm(ChatViewModel viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
            SetupUI();
            BindViewModel();
        }

        private void InitializeComponent()
        {
            this.Text = "Chat";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = SystemColors.Window;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.AutoScroll = true;
        }

        private void SetupUI()
        {
            this.SuspendLayout();

            // Create main layout panels
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(245, 245, 245),
                BorderStyle = BorderStyle.FixedSingle
            };

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            // Setup sidebar
            SetupSidebar();

            // Setup main content
            SetupMainContent();

            // Stats panel at bottom
            SetupStatsPanel();

            // Add controls to form
            this.Controls.Add(statsPanel);
            this.Controls.Add(mainPanel);
            this.Controls.Add(sidebarPanel);

            this.ResumeLayout();
        }

        private void SetupSidebar()
        {
            // New Chat Button
            newChatButton = new Button
            {
                Text = "+ New Chat",
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(99, 102, 241), // Purple
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            newChatButton.FlatAppearance.BorderSize = 0;
            newChatButton.Click += NewChatButton_Click;

            // Sessions List
            sessionsListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 40
            };
            sessionsListBox.DrawItem += SessionsListBox_DrawItem;

            // Layout in sidebar
            var sidebarLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            sidebarLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            sidebarLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            
            sidebarLayoutPanel.Controls.Add(newChatButton, 0, 0);
            sidebarLayoutPanel.Controls.Add(sessionsListBox, 0, 1);
            
            sidebarPanel.Controls.Add(sidebarLayoutPanel);
        }

        private void SetupMainContent()
        {
            // Header panel
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(16)
            };

            var titleLabel = new Label
            {
                Text = "Chat",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(0, 0),
                AutoSize = true
            };

            activeModelLabel = new Label
            {
                Text = "No model loaded",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(0, titleLabel.Height + 4),
                AutoSize = true
            };

            // Right-aligned buttons
            clearButton = new Button
            {
                Text = "Clear",
                Size = new Size(75, 30),
                Location = new Point(headerPanel.Width - 240, 10),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            clearButton.Click += ClearButton_Click;

            exportButton = new Button
            {
                Text = "Export ▼",
                Size = new Size(80, 30),
                Location = new Point(headerPanel.Width - 160, 10),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            exportButton.Click += ExportButton_Click;

            var searchButton = new Button
            {
                Text = "🔍",
                Size = new Size(40, 30),
                Location = new Point(headerPanel.Width - 75, 10),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            searchButton.Click += SearchButton_Click;

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(activeModelLabel);
            headerPanel.Controls.Add(clearButton);
            headerPanel.Controls.Add(exportButton);
            headerPanel.Controls.Add(searchButton);

            // Messages display area using Panel instead of RichTextBox for better control
            messagesDisplay = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10)
            };

            // Input area
            var inputPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(245, 245, 245)
            };

            messageInput = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Height = 50,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                MaxLength = 2000
            };
            messageInput.KeyDown += MessageInput_KeyDown;

            sendButton = new Button
            {
                Text = "Send ➤",
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(inputPanel.Width - 180, 10)
            };
            sendButton.FlatAppearance.BorderSize = 0;
            sendButton.Click += SendButton_Click;

            stopButton = new Button
            {
                Text = "⏹ Stop",
                Size = new Size(80, 30),
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false,
                Location = new Point(inputPanel.Width - 90, 10)
            };
            stopButton.FlatAppearance.BorderSize = 0;
            stopButton.Click += StopButton_Click;

            var inputContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 100, 0)
            };
            inputContainer.Controls.Add(messageInput);

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 180
            };
            buttonPanel.Controls.Add(sendButton);
            buttonPanel.Controls.Add(stopButton);

            inputPanel.Controls.Add(inputContainer);
            inputPanel.Controls.Add(buttonPanel);

            // Layout main panel
            var mainLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                RowStyles = { 
                    new RowStyle(SizeType.Absolute, 80), 
                    new RowStyle(SizeType.Percent, 100), 
                    new RowStyle(SizeType.Absolute, 80) 
                }
            };

            mainLayoutPanel.Controls.Add(headerPanel, 0, 0);
            mainLayoutPanel.Controls.Add(messagesDisplay, 0, 1);
            mainLayoutPanel.Controls.Add(inputPanel, 0, 2);

            mainPanel.Controls.Add(mainLayoutPanel);
        }

        private void SetupStatsPanel()
        {
            statsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            tokensPerSecondLabel = new Label
            {
                Text = "⚡ 0 tok/s",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(10, 12),
                AutoSize = true
            };

            totalTokensLabel = new Label
            {
                Text = "📊 0 tokens",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(100, 12),
                AutoSize = true
            };

            memoryUsageLabel = new Label
            {
                Text = "💾 N/A",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(200, 12),
                AutoSize = true
            };

            elapsedTimeLabel = new Label
            {
                Text = "⏱ 0s",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(280, 12),
                AutoSize = true
            };

            contextWindowLabel = new Label
            {
                Text = "📐 Ctx: 0",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(350, 12),
                AutoSize = true
            };

            gpuLayersLabel = new Label
            {
                Text = "🎮 GPU: 0",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(430, 12),
                AutoSize = true
            };

            statsPanel.Controls.Add(tokensPerSecondLabel);
            statsPanel.Controls.Add(totalTokensLabel);
            statsPanel.Controls.Add(memoryUsageLabel);
            statsPanel.Controls.Add(elapsedTimeLabel);
            statsPanel.Controls.Add(contextWindowLabel);
            statsPanel.Controls.Add(gpuLayersLabel);
        }

        private void BindViewModel()
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            // Setup timer for frequent updates
            uiUpdateTimer = new Timer { Interval = 100 };
            uiUpdateTimer.Tick += UIUpdateTimer_Tick;
            uiUpdateTimer.Start();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateUIFromViewModel()));
            }
            else
            {
                UpdateUIFromViewModel();
            }
        }

        private void UIUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateUIFromViewModel();
        }

        private void UpdateUIFromViewModel()
        {
            // Update active model label
            if (_viewModel.ActiveModelInfo != null)
            {
                activeModelLabel.Text = _viewModel.ActiveModelInfo;
            }

            // Update buttons state
            sendButton.Enabled = !_viewModel.IsGenerating;
            stopButton.Visible = _viewModel.IsGenerating;

            // Update stats
            tokensPerSecondLabel.Text = $"⚡ {_viewModel.TokensPerSecond:F1} tok/s";
            totalTokensLabel.Text = $"📊 {_viewModel.TotalTokens} tokens";
            memoryUsageLabel.Text = $"💾 {_viewModel.MemoryUsage}";
            elapsedTimeLabel.Text = $"⏱ {_viewModel.ElapsedTime}";
            contextWindowLabel.Text = $"📐 Ctx: {_viewModel.ContextWindow}";
            gpuLayersLabel.Text = $"🎮 GPU: {_viewModel.GpuLayers}";

            // Update messages display
            UpdateMessagesDisplay();
        }

        private void UpdateMessagesDisplay()
        {
            messagesDisplay.Clear();
            foreach (var message in _viewModel.Messages)
            {
                messagesDisplay.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
                
                if (message.IsUser)
                {
                    messagesDisplay.SelectionColor = Color.Blue;
                    messagesDisplay.AppendText($"You: ");
                }
                else
                {
                    messagesDisplay.SelectionColor = Color.Green;
                    messagesDisplay.AppendText($"Assistant: ");
                }
                
                messagesDisplay.SelectionFont = new Font("Segoe UI", 10, FontStyle.Regular);
                messagesDisplay.SelectionColor = Color.Black;
                
                // Parse the message content for code blocks
                var segments = Services.MarkdownParser.Parse(message.Content);
                
                foreach (var segment in segments)
                {
                    switch (segment.Type)
                    {
                        case Services.SegmentType.Text:
                            messagesDisplay.AppendText($"{segment.Content}");
                            break;
                        case Services.SegmentType.CodeBlock:
                            // Display code block with syntax highlighting
                            messagesDisplay.AppendText("\n");
                            messagesDisplay.AppendText($"```{segment.Language}\n{segment.Content}\n```\n");
                            break;
                        case Services.SegmentType.InlineCode:
                            // For inline code, we could use a different style, but for now treat as regular text
                            messagesDisplay.AppendText($"{segment.Content}");
                            break;
                    }
                }
                
                messagesDisplay.AppendText("\n\n");
            }
            
            messagesDisplay.ScrollToCaret();
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(messageInput.Text))
            {
                _viewModel.SendMessageCommand.Execute(messageInput.Text);
                messageInput.Clear();
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            _viewModel.StopGenerationCommand.Execute(null);
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            _viewModel.ClearChatCommand.Execute(null);
        }

        private void NewChatButton_Click(object sender, EventArgs e)
        {
            _viewModel.NewSessionCommand.Execute(null);
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Export as Markdown (.md)", null, (s, e) => _viewModel.ExportChatAsMarkdownCommand.Execute(null));
            menu.Items.Add("Export as JSON (.json)", null, (s, e) => _viewModel.ExportChatAsJsonCommand.Execute(null));
            menu.Items.Add("Export as Text (.txt)", null, (s, e) => _viewModel.ExportChatAsTextCommand.Execute(null));
            
            var btn = sender as Button;
            menu.Show(btn, 0, btn.Height);
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Search functionality would be implemented here.");
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                SendButton_Click(sender, e);
                e.Handled = true;
            }
        }

        private void SessionsListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            
            if (e.Index >= 0)
            {
                var session = _viewModel.Sessions[e.Index];
                var text = $"{session.Title} ({session.MessageCount} messages)";
                
                e.Graphics.DrawString(text, e.Font, Brushes.Black, e.Bounds);
                
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    e.DrawFocusRectangle();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                uiUpdateTimer?.Stop();
                uiUpdateTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}