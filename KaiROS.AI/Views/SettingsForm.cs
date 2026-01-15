using System;
using System.Drawing;
using System.Windows.Forms;
using KaiROS.AI.ViewModels;

namespace KaiROS.AI.Views
{
    public partial class SettingsForm : Form
    {
        private SettingsViewModel _viewModel;
        private TabControl settingsTabs;
        private Panel generalPanel, appearancePanel, advancedPanel;
        private TextBox modelPathTextBox, contextLengthTextBox, gpuLayersTextBox, temperatureTextBox;
        private NumericUpDown maxTokensUpDown, batchSizeUpDown, threadsUpDown;
        private ComboBox themeComboBox, hardwareComboBox;
        private CheckBox autoUpdateCheckBox, notificationsCheckBox, gpuCheckBox;
        private Button browseModelPathButton, saveSettingsButton, resetSettingsButton;

        public SettingsForm(SettingsViewModel viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
            SetupUI();
            BindViewModel();
        }

        private void InitializeComponent()
        {
            this.Text = "Settings";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = SystemColors.Window;
        }

        private void SetupUI()
        {
            this.SuspendLayout();

            // Create tab control
            settingsTabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };

            // General Settings Tab
            CreateGeneralTab();

            // Appearance Settings Tab
            CreateAppearanceTab();

            // Advanced Settings Tab
            CreateAdvancedTab();

            // Add tabs to control
            settingsTabs.TabPages.Add("General", "General Settings");
            settingsTabs.TabPages.Add("Appearance", "Appearance");
            settingsTabs.TabPages.Add("Advanced", "Advanced");

            // Add tab control to form
            this.Controls.Add(settingsTabs);

            this.ResumeLayout();
        }

        private void CreateGeneralTab()
        {
            generalPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var titleLabel = new Label
            {
                Text = "General Settings",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(0, 10),
                Size = new Size(300, 30)
            };

            // Model Path
            var modelPathLabel = new Label
            {
                Text = "Model Path:",
                Location = new Point(0, titleLabel.Bottom + 20),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            modelPathTextBox = new TextBox
            {
                Location = new Point(modelPathLabel.Right + 10, modelPathLabel.Top - 3),
                Size = new Size(400, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            browseModelPathButton = new Button
            {
                Text = "Browse...",
                Location = new Point(modelPathTextBox.Right + 10, modelPathLabel.Top - 3),
                Size = new Size(80, 25)
            };
            browseModelPathButton.Click += BrowseModelPathButton_Click;

            // Auto Update Check
            autoUpdateCheckBox = new CheckBox
            {
                Text = "Automatically check for updates",
                Location = new Point(0, modelPathTextBox.Bottom + 20),
                Size = new Size(300, 25),
                Checked = true
            };

            // Notifications
            notificationsCheckBox = new CheckBox
            {
                Text = "Enable desktop notifications",
                Location = new Point(0, autoUpdateCheckBox.Bottom + 10),
                Size = new Size(300, 25),
                Checked = true
            };

            // Context Length
            var contextLengthLabel = new Label
            {
                Text = "Context Length:",
                Location = new Point(0, notificationsCheckBox.Bottom + 20),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            contextLengthTextBox = new TextBox
            {
                Location = new Point(contextLengthLabel.Right + 10, contextLengthLabel.Top - 3),
                Size = new Size(100, 25)
            };

            // Temperature
            var temperatureLabel = new Label
            {
                Text = "Temperature:",
                Location = new Point(0, contextLengthTextBox.Bottom + 20),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            temperatureTextBox = new TextBox
            {
                Location = new Point(temperatureLabel.Right + 10, temperatureLabel.Top - 3),
                Size = new Size(100, 25)
            };

            // Save and Reset buttons
            saveSettingsButton = new Button
            {
                Text = "Save Settings",
                Location = new Point(0, temperatureTextBox.Bottom + 30),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            saveSettingsButton.FlatAppearance.BorderSize = 0;
            saveSettingsButton.Click += SaveSettingsButton_Click;

            resetSettingsButton = new Button
            {
                Text = "Reset to Defaults",
                Location = new Point(saveSettingsButton.Right + 20, saveSettingsButton.Top),
                Size = new Size(140, 35),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            resetSettingsButton.Click += ResetSettingsButton_Click;

            generalPanel.Controls.Add(titleLabel);
            generalPanel.Controls.Add(modelPathLabel);
            generalPanel.Controls.Add(modelPathTextBox);
            generalPanel.Controls.Add(browseModelPathButton);
            generalPanel.Controls.Add(autoUpdateCheckBox);
            generalPanel.Controls.Add(notificationsCheckBox);
            generalPanel.Controls.Add(contextLengthLabel);
            generalPanel.Controls.Add(contextLengthTextBox);
            generalPanel.Controls.Add(temperatureLabel);
            generalPanel.Controls.Add(temperatureTextBox);
            generalPanel.Controls.Add(saveSettingsButton);
            generalPanel.Controls.Add(resetSettingsButton);

            settingsTabs.TabPages[0].Controls.Add(generalPanel);
        }

        private void CreateAppearanceTab()
        {
            appearancePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var titleLabel = new Label
            {
                Text = "Appearance Settings",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(0, 10),
                Size = new Size(300, 30)
            };

            // Theme selection
            var themeLabel = new Label
            {
                Text = "Theme:",
                Location = new Point(0, titleLabel.Bottom + 20),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            themeComboBox = new ComboBox
            {
                Location = new Point(themeLabel.Right + 10, themeLabel.Top - 3),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            themeComboBox.Items.Add("Light");
            themeComboBox.Items.Add("Dark");
            themeComboBox.Items.Add("System");
            themeComboBox.SelectedIndex = 0;

            // Hardware acceleration
            var hardwareLabel = new Label
            {
                Text = "Hardware Acceleration:",
                Location = new Point(0, themeComboBox.Bottom + 20),
                Size = new Size(200, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            hardwareComboBox = new ComboBox
            {
                Location = new Point(hardwareLabel.Right + 10, hardwareLabel.Top - 3),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            hardwareComboBox.Items.Add("Auto");
            hardwareComboBox.Items.Add("CPU Only");
            hardwareComboBox.Items.Add("CUDA");
            hardwareComboBox.Items.Add("Vulkan");
            hardwareComboBox.SelectedIndex = 0;

            appearancePanel.Controls.Add(titleLabel);
            appearancePanel.Controls.Add(themeLabel);
            appearancePanel.Controls.Add(themeComboBox);
            appearancePanel.Controls.Add(hardwareLabel);
            appearancePanel.Controls.Add(hardwareComboBox);

            settingsTabs.TabPages[1].Controls.Add(appearancePanel);
        }

        private void CreateAdvancedTab()
        {
            advancedPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var titleLabel = new Label
            {
                Text = "Advanced Settings",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(0, 10),
                Size = new Size(300, 30)
            };

            // GPU Layers
            var gpuLayersLabel = new Label
            {
                Text = "GPU Layers:",
                Location = new Point(0, titleLabel.Bottom + 20),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            gpuLayersTextBox = new TextBox
            {
                Location = new Point(gpuLayersLabel.Right + 10, gpuLayersLabel.Top - 3),
                Size = new Size(100, 25)
            };

            // GPU Acceleration
            gpuCheckBox = new CheckBox
            {
                Text = "Enable GPU Acceleration",
                Location = new Point(0, gpuLayersTextBox.Bottom + 10),
                Size = new Size(250, 25),
                Checked = true
            };

            // Max Tokens
            var maxTokensLabel = new Label
            {
                Text = "Max Tokens:",
                Location = new Point(0, gpuCheckBox.Bottom + 20),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            maxTokensUpDown = new NumericUpDown
            {
                Location = new Point(maxTokensLabel.Right + 10, maxTokensLabel.Top - 3),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 10000,
                Value = 2048
            };

            // Batch Size
            var batchSizeLabel = new Label
            {
                Text = "Batch Size:",
                Location = new Point(0, maxTokensUpDown.Bottom + 20),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            batchSizeUpDown = new NumericUpDown
            {
                Location = new Point(batchSizeLabel.Right + 10, batchSizeLabel.Top - 3),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 1024,
                Value = 512
            };

            // Threads
            var threadsLabel = new Label
            {
                Text = "Threads:",
                Location = new Point(0, batchSizeUpDown.Bottom + 20),
                Size = new Size(120, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            threadsUpDown = new NumericUpDown
            {
                Location = new Point(threadsLabel.Right + 10, threadsLabel.Top - 3),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 16,
                Value = 8
            };

            advancedPanel.Controls.Add(titleLabel);
            advancedPanel.Controls.Add(gpuLayersLabel);
            advancedPanel.Controls.Add(gpuLayersTextBox);
            advancedPanel.Controls.Add(gpuCheckBox);
            advancedPanel.Controls.Add(maxTokensLabel);
            advancedPanel.Controls.Add(maxTokensUpDown);
            advancedPanel.Controls.Add(batchSizeLabel);
            advancedPanel.Controls.Add(batchSizeUpDown);
            advancedPanel.Controls.Add(threadsLabel);
            advancedPanel.Controls.Add(threadsUpDown);

            settingsTabs.TabPages[2].Controls.Add(advancedPanel);
        }

        private void BindViewModel()
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            LoadSettings();
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

        private void LoadSettings()
        {
            // Load settings from ViewModel into UI controls
            modelPathTextBox.Text = _viewModel.ModelPath;
            contextLengthTextBox.Text = _viewModel.ContextLength.ToString();
            temperatureTextBox.Text = _viewModel.Temperature.ToString("F2");
            autoUpdateCheckBox.Checked = _viewModel.AutoUpdateCheck;
            notificationsCheckBox.Checked = _viewModel.EnableNotifications;
            gpuCheckBox.Checked = _viewModel.EnableGpuAcceleration;
            gpuLayersTextBox.Text = _viewModel.GpuLayers.ToString();
            
            maxTokensUpDown.Value = Math.Max(maxTokensUpDown.Minimum, Math.Min(maxTokensUpDown.Maximum, _viewModel.MaxTokens));
            batchSizeUpDown.Value = Math.Max(batchSizeUpDown.Minimum, Math.Min(batchSizeUpDown.Maximum, _viewModel.BatchSize));
            threadsUpDown.Value = Math.Max(threadsUpDown.Minimum, Math.Min(threadsUpDown.Maximum, _viewModel.Threads));
            
            themeComboBox.SelectedItem = _viewModel.Theme;
            hardwareComboBox.SelectedItem = _viewModel.HardwareAcceleration;
        }

        private void UpdateUIFromViewModel()
        {
            LoadSettings();
        }

        private void BrowseModelPathButton_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select the folder containing your AI models";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    modelPathTextBox.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void SaveSettingsButton_Click(object sender, EventArgs e)
        {
            // Update ViewModel with values from UI
            _viewModel.ModelPath = modelPathTextBox.Text;
            _viewModel.ContextLength = int.TryParse(contextLengthTextBox.Text, out int ctxLen) ? ctxLen : 2048;
            _viewModel.Temperature = double.TryParse(temperatureTextBox.Text, out double temp) ? temp : 0.7;
            _viewModel.AutoUpdateCheck = autoUpdateCheckBox.Checked;
            _viewModel.EnableNotifications = notificationsCheckBox.Checked;
            _viewModel.EnableGpuAcceleration = gpuCheckBox.Checked;
            _viewModel.GpuLayers = int.TryParse(gpuLayersTextBox.Text, out int gpuLayers) ? gpuLayers : 20;
            
            _viewModel.MaxTokens = (int)maxTokensUpDown.Value;
            _viewModel.BatchSize = (int)batchSizeUpDown.Value;
            _viewModel.Threads = (int)threadsUpDown.Value;
            
            _viewModel.Theme = themeComboBox.SelectedItem?.ToString() ?? "System";
            _viewModel.HardwareAcceleration = hardwareComboBox.SelectedItem?.ToString() ?? "Auto";

            // Save settings
            _viewModel.SaveSettingsCommand.Execute(null);
            MessageBox.Show("Settings saved successfully!", "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ResetSettingsButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all settings to their defaults?", 
                "Confirm Reset", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                _viewModel.ResetSettingsCommand.Execute(null);
                LoadSettings(); // Reload the UI with default values
            }
        }
    }
}