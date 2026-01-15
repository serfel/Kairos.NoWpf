using System;
using System.Drawing;
using System.Windows.Forms;
using KaiROS.AI.ViewModels;

namespace KaiROS.AI;

public partial class MainForm : Form
{
    private MainViewModel _viewModel;
    private Panel sidebarPanel;
    private Panel contentPanel;
    private ListBox navigationList;
    private Label statusLabel;
    private NotifyIcon trayIcon;
    private ContextMenuStrip trayMenu;

    public MainForm(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        SetupUI();
        SetupTrayIcon();
        BindViewModel();
    }

    private void InitializeComponent()
    {
        this.Text = "KaiROS AI";
        this.Size = new Size(1200, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = SystemColors.Window;
        this.Resize += MainForm_Resize;
    }

    private void SetupUI()
    {
        // Create main layout
        this.SuspendLayout();

        // Create sidebar panel (navigation)
        sidebarPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 260,
            BackColor = Color.FromArgb(245, 245, 245),
            BorderStyle = BorderStyle.FixedSingle
        };

        // Create logo/title area
        var titlePanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            Padding = new Padding(24, 24, 24, 16)
        };

        var logoLabel = new Label
        {
            Text = "KaiROS AI",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(99, 102, 241), // Purple-like color
            AutoSize = true
        };

        var subtitleLabel = new Label
        {
            Text = "Local AI Assistant",
            Font = new Font("Segoe UI", 8, FontStyle.Regular),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(0, logoLabel.Height + 2)
        };

        titlePanel.Controls.Add(logoLabel);
        titlePanel.Controls.Add(subtitleLabel);

        // Create navigation list
        navigationList = new ListBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            IntegralHeight = false,
            ItemHeight = 30,
            SelectionMode = SelectionMode.One
        };

        navigationList.Items.Add("📦 Models");
        navigationList.Items.Add("💬 Chat");
        navigationList.Items.Add("📄 Documents");
        navigationList.Items.Add("⚙️ Settings");

        navigationList.SelectedIndexChanged += NavigationList_SelectedIndexChanged;

        // Create status footer
        var statusFooter = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 80,
            BackColor = Color.FromArgb(250, 250, 250),
            Padding = new Padding(16),
            BorderStyle = BorderStyle.FixedSingle
        };

        var activeModelLabel = new Label
        {
            Text = "Active Model",
            Font = new Font("Segoe UI", 8, FontStyle.Regular),
            ForeColor = Color.Gray,
            AutoSize = true
        };

        var modelValueLabel = new Label
        {
            Name = "lblActiveModel",
            Text = "No model loaded",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.Black,
            AutoSize = true,
            Location = new Point(0, activeModelLabel.Height + 4)
        };

        var hardwareLabel = new Label
        {
            Name = "lblHardwareInfo",
            Text = "Detecting hardware...",
            Font = new Font("Segoe UI", 8, FontStyle.Regular),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(0, activeModelLabel.Height + modelValueLabel.Height + 12)
        };

        statusFooter.Controls.Add(activeModelLabel);
        statusFooter.Controls.Add(modelValueLabel);
        statusFooter.Controls.Add(hardwareLabel);

        sidebarPanel.Controls.Add(navigationList);
        sidebarPanel.Controls.Add(statusFooter);
        sidebarPanel.Controls.Add(titlePanel);

        // Create content panel
        contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(32)
        };

        // Create status label at bottom
        statusLabel = new Label
        {
            Text = "Ready",
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = Color.FromArgb(245, 245, 245),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 8, 16, 8)
        };

        // Add controls to form
        this.Controls.Add(contentPanel);
        this.Controls.Add(sidebarPanel);
        this.Controls.Add(statusLabel);

        this.ResumeLayout();
    }

    private void SetupTrayIcon()
    {
        trayIcon = new NotifyIcon
        {
            Text = "KaiROS AI",
            Visible = false
        };

        // Try to load icon
        try
        {
            var iconPath = System.IO.Path.Combine(Application.StartupPath, "Assets", "app.ico");
            if (System.IO.File.Exists(iconPath))
            {
                trayIcon.Icon = new Icon(iconPath);
            }
        }
        catch { /* Ignore icon loading errors */ }

        // Create context menu
        trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("🗨️ New Chat", null, TrayMenu_NewChat_Click);
        trayMenu.Items.Add("⚙️ Settings", null, TrayMenu_Settings_Click);
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add("🔄 Restore", null, TrayMenu_Restore_Click);
        trayMenu.Items.Add("❌ Exit", null, TrayMenu_Exit_Click);

        trayIcon.ContextMenuStrip = trayMenu;
    }

    private void BindViewModel()
    {
        // Bind to ViewModel properties
        // Since we can't use data binding like in WPF, we'll update manually
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
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

    private void UpdateUIFromViewModel()
    {
        // Update active model label
        var activeModelLabel = sidebarPanel.Controls.Find("lblActiveModel", true)[0] as Label;
        if (activeModelLabel != null)
        {
            activeModelLabel.Text = _viewModel.ActiveModelName ?? "No model loaded";
        }

        // Update hardware info label
        var hardwareLabel = sidebarPanel.Controls.Find("lblHardwareInfo", true)[0] as Label;
        if (hardwareLabel != null)
        {
            hardwareLabel.Text = _viewModel.HardwareInfo ?? "Detecting hardware...";
        }

        // Update status label
        statusLabel.Text = _viewModel.StatusText ?? "Ready";
    }

    private void NavigationList_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (navigationList.SelectedIndex >= 0)
        {
            switch (navigationList.SelectedIndex)
            {
                case 0: // Models
                    _viewModel.NavigateToModelCatalogCommand.Execute(null);
                    break;
                case 1: // Chat
                    _viewModel.NavigateToChatCommand.Execute(null);
                    break;
                case 2: // Documents
                    _viewModel.NavigateToDocumentsCommand.Execute(null);
                    break;
                case 3: // Settings
                    _viewModel.NavigateToSettingsCommand.Execute(null);
                    break;
            }
        }
    }

    private void MainForm_Resize(object sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Minimized)
        {
            this.Hide();
            trayIcon.Visible = true;
            trayIcon.ShowBalloonTip(1000, "KaiROS AI", "Application running in background.", ToolTipIcon.Info);
        }
    }

    private void ShowForm()
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.Activate();
        trayIcon.Visible = false;
    }

    private void TrayMenu_NewChat_Click(object sender, EventArgs e)
    {
        ShowForm();
        _viewModel.NavigateToChatCommand.Execute(null);
    }

    private void TrayMenu_Settings_Click(object sender, EventArgs e)
    {
        ShowForm();
        _viewModel.NavigateToSettingsCommand.Execute(null);
    }

    private void TrayMenu_Restore_Click(object sender, EventArgs e)
    {
        ShowForm();
    }

    private void TrayMenu_Exit_Click(object sender, EventArgs e)
    {
        trayIcon.Visible = false;
        Application.Exit();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        trayIcon?.Dispose();
        base.OnFormClosing(e);
    }
}