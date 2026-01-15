using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KaiROS.AI.ViewModels;

namespace KaiROS.AI.Views
{
    public partial class ModelCatalogForm : Form
    {
        private ModelCatalogViewModel _viewModel;
        private Panel headerPanel;
        private Panel filtersPanel;
        private Panel modelsPanel;
        private Button addCustomModelButton;
        private Button allButton, smallButton, mediumButton, largeButton;
        private ComboBox orgFilter, familyFilter, variantFilter;
        private CheckBox recommendedCheckBox;
        private TextBox searchBox;
        private Panel downloadedModelsPanel;
        private Panel allModelsPanel;
        private FlowLayoutPanel downloadedModelsFlow;
        private FlowLayoutPanel allModelsFlow;

        public ModelCatalogForm(ModelCatalogViewModel viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
            SetupUI();
            BindViewModel();
        }

        private void InitializeComponent()
        {
            this.Text = "Model Catalog";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = SystemColors.Window;
            this.AutoScroll = true;
        }

        private void SetupUI()
        {
            this.SuspendLayout();

            // Header panel
            SetupHeader();

            // Filters panel
            SetupFilters();

            // Models display panel
            SetupModelsPanel();

            // Layout everything
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                RowStyles = { 
                    new RowStyle(SizeType.AutoSize), 
                    new RowStyle(SizeType.AutoSize), 
                    new RowStyle(SizeType.Percent, 100) 
                }
            };

            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(filtersPanel, 0, 1);
            mainLayout.Controls.Add(modelsPanel, 0, 2);

            this.Controls.Add(mainLayout);
            this.ResumeLayout();
        }

        private void SetupHeader()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(20)
            };

            var titleLabel = new Label
            {
                Text = "Model Catalog",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(0, 0),
                AutoSize = true
            };

            var subtitleLabel = new Label
            {
                Text = "Download and manage AI models for local inference",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(0, titleLabel.Height + 8),
                AutoSize = true
            };

            addCustomModelButton = new Button
            {
                Text = "➕ Add Custom Model",
                Size = new Size(180, 40),
                BackColor = Color.FromArgb(99, 102, 241), // Purple
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(headerPanel.Width - 200, 10)
            };
            addCustomModelButton.FlatAppearance.BorderSize = 0;
            addCustomModelButton.Click += AddCustomModelButton_Click;

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);
            headerPanel.Controls.Add(addCustomModelButton);
        }

        private void SetupFilters()
        {
            filtersPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = Color.FromArgb(245, 245, 245)
            };

            // Category buttons
            allButton = new Button
            {
                Text = "All",
                Size = new Size(60, 30),
                BackColor = SystemColors.Control,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(0, 0)
            };
            allButton.Click += (s, e) => _viewModel.FilterByCategoryCommand.Execute("all");

            smallButton = new Button
            {
                Text = "Small",
                Size = new Size(70, 30),
                BackColor = SystemColors.Control,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(allButton.Right + 8, 0)
            };
            smallButton.Click += (s, e) => _viewModel.FilterByCategoryCommand.Execute("small");

            mediumButton = new Button
            {
                Text = "Medium",
                Size = new Size(80, 30),
                BackColor = SystemColors.Control,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(smallButton.Right + 8, 0)
            };
            mediumButton.Click += (s, e) => _viewModel.FilterByCategoryCommand.Execute("medium");

            largeButton = new Button
            {
                Text = "Large",
                Size = new Size(70, 30),
                BackColor = SystemColors.Control,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(mediumButton.Right + 8, 0)
            };
            largeButton.Click += (s, e) => _viewModel.FilterByCategoryCommand.Execute("large");

            // Organization filter
            var orgLabel = new Label
            {
                Text = "Org:",
                Location = new Point(largeButton.Right + 20, 5),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            orgFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 100,
                Location = new Point(orgLabel.Right + 6, 2),
                FlatStyle = FlatStyle.Flat
            };
            orgFilter.SelectedValueChanged += (s, e) => _viewModel.SelectedOrganization = orgFilter.SelectedItem?.ToString();

            // Family filter
            var familyLabel = new Label
            {
                Text = "Family:",
                Location = new Point(orgFilter.Right + 20, 5),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            familyFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 90,
                Location = new Point(familyLabel.Right + 6, 2),
                FlatStyle = FlatStyle.Flat
            };
            familyFilter.SelectedValueChanged += (s, e) => _viewModel.SelectedFamily = familyFilter.SelectedItem?.ToString();

            // Variant filter
            var variantLabel = new Label
            {
                Text = "Variant:",
                Location = new Point(familyFilter.Right + 20, 5),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            variantFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120,
                Location = new Point(variantLabel.Right + 6, 2),
                FlatStyle = FlatStyle.Flat
            };
            variantFilter.SelectedValueChanged += (s, e) => _viewModel.SelectedVariant = variantFilter.SelectedItem?.ToString();

            // Recommended checkbox
            recommendedCheckBox = new CheckBox
            {
                Text = "★ Recommended Only",
                Location = new Point(variantFilter.Right + 20, 5),
                AutoSize = true,
                FlatStyle = FlatStyle.Flat
            };
            recommendedCheckBox.CheckedChanged += (s, e) => _viewModel.ShowRecommendedOnly = recommendedCheckBox.Checked;

            // Search box
            searchBox = new TextBox
            {
                Width = 200,
                Location = new Point(recommendedCheckBox.Right + 20, 2),
                PlaceholderText = "Search models..."
            };
            searchBox.TextChanged += (s, e) => _viewModel.SearchText = searchBox.Text;

            filtersPanel.Controls.Add(allButton);
            filtersPanel.Controls.Add(smallButton);
            filtersPanel.Controls.Add(mediumButton);
            filtersPanel.Controls.Add(largeButton);
            filtersPanel.Controls.Add(orgLabel);
            filtersPanel.Controls.Add(orgFilter);
            filtersPanel.Controls.Add(familyLabel);
            filtersPanel.Controls.Add(familyFilter);
            filtersPanel.Controls.Add(variantLabel);
            filtersPanel.Controls.Add(variantFilter);
            filtersPanel.Controls.Add(recommendedCheckBox);
            filtersPanel.Controls.Add(searchBox);
        }

        private void SetupModelsPanel()
        {
            modelsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            downloadedModelsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Visible = false
            };

            var downloadedTitlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10)
            };

            var downloadedTitleLabel = new Label
            {
                Text = "✓ Downloaded Models",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Green,
                Location = new Point(0, 10),
                AutoSize = true
            };

            downloadedTitlePanel.Controls.Add(downloadedTitleLabel);

            downloadedModelsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown
            };

            downloadedModelsPanel.Controls.Add(downloadedModelsFlow);
            downloadedModelsPanel.Controls.Add(downloadedTitlePanel);

            allModelsPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            var allModelsTitleLabel = new Label
            {
                Text = "All Models by Organization",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(10, 10),
                AutoSize = true
            };

            allModelsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(10)
            };

            allModelsPanel.Controls.Add(allModelsTitleLabel);
            allModelsPanel.Controls.Add(allModelsFlow);

            var modelsLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                RowStyles = { 
                    new RowStyle(SizeType.AutoSize), 
                    new RowStyle(SizeType.Percent, 100) 
                }
            };

            modelsLayoutPanel.Controls.Add(downloadedModelsPanel, 0, 0);
            modelsLayoutPanel.Controls.Add(allModelsPanel, 0, 1);

            modelsPanel.Controls.Add(modelsLayoutPanel);
        }

        private void BindViewModel()
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            RefreshUI();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => RefreshUI()));
            }
            else
            {
                RefreshUI();
            }
        }

        private void RefreshUI()
        {
            // Update filters dropdowns
            orgFilter.DataSource = new List<string> { "All" }.Concat(_viewModel.Organizations).ToList();
            familyFilter.DataSource = new List<string> { "All" }.Concat(_viewModel.Families).ToList();
            variantFilter.DataSource = new List<string> { "All" }.Concat(_viewModel.Variants).ToList();

            // Update visibility of downloaded models section
            downloadedModelsPanel.Visible = _viewModel.DownloadedModels.Any();

            // Populate downloaded models
            PopulateDownloadedModels();

            // Populate all models by organization
            PopulateAllModels();
        }

        private void PopulateDownloadedModels()
        {
            downloadedModelsFlow.Controls.Clear();

            foreach (var model in _viewModel.DownloadedModels)
            {
                var modelCard = CreateModelCard(model, true);
                downloadedModelsFlow.Controls.Add(modelCard);
            }
        }

        private void PopulateAllModels()
        {
            allModelsFlow.Controls.Clear();

            foreach (var group in _viewModel.GroupedModels)
            {
                var expander = CreateOrganizationExpander(group);
                allModelsFlow.Controls.Add(expander);
            }
        }

        private Panel CreateModelCard(dynamic modelItem, bool isDownloaded = false)
        {
            var cardPanel = new Panel
            {
                Size = new Size(500, 150),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5),
                BackColor = Color.White
            };

            var nameLabel = new Label
            {
                Text = modelItem.Model.DisplayName,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            var descriptionLabel = new Label
            {
                Text = modelItem.Model.Description,
                Font = new Font("Segoe UI", 9),
                Location = new Point(10, nameLabel.Bottom + 5),
                Size = new Size(cardPanel.Width - 20, 40),
                ForeColor = Color.Gray
            };

            var sizeLabel = new Label
            {
                Text = $"💾 {modelItem.Model.SizeText}",
                Font = new Font("Segoe UI", 9),
                Location = new Point(10, descriptionLabel.Bottom + 5),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            var ramLabel = new Label
            {
                Text = $"🧠 {modelItem.Model.MinRam} RAM",
                Font = new Font("Segoe UI", 9),
                Location = new Point(sizeLabel.Right + 20, descriptionLabel.Bottom + 5),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            var actionPanel = new Panel
            {
                Location = new Point(cardPanel.Width - 200, 10),
                Size = new Size(180, cardPanel.Height - 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            var loadButton = new Button
            {
                Text = "Load Model",
                Size = new Size(100, 30),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = !modelItem.IsActive
            };
            loadButton.FlatAppearance.BorderSize = 0;
            //loadButton.Click += (s, e) => modelItem.SetActiveCommand.Execute(null);

            var deleteButton = new Button
            {
                Text = "🗑",
                Size = new Size(30, 30),
                Location = new Point(loadButton.Right + 5, 0),
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            //deleteButton.Click += (s, e) => modelItem.DeleteCommand.Execute(null);

            var activeLabel = new Label
            {
                Text = "ACTIVE",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Green,
                Size = new Size(50, 20),
                Location = new Point(0, loadButton.Bottom + 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = modelItem.IsActive
            };

            actionPanel.Controls.Add(loadButton);
            actionPanel.Controls.Add(deleteButton);
            actionPanel.Controls.Add(activeLabel);

            cardPanel.Controls.Add(nameLabel);
            cardPanel.Controls.Add(descriptionLabel);
            cardPanel.Controls.Add(sizeLabel);
            cardPanel.Controls.Add(ramLabel);
            cardPanel.Controls.Add(actionPanel);

            return cardPanel;
        }

        private Panel CreateOrganizationExpander(dynamic orgGroup)
        {
            var expanderPanel = new Panel
            {
                Size = new Size(800, 100),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5),
                BackColor = Color.FromArgb(245, 245, 245)
            };

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.White
            };

            var orgNameLabel = new Label
            {
                Text = orgGroup.OrganizationName,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            var countLabel = new Label
            {
                Text = $"{orgGroup.ModelCount} models",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(99, 102, 241),
                Size = new Size(80, 25),
                Location = new Point(orgNameLabel.Right + 10, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };

            headerPanel.Controls.Add(orgNameLabel);
            headerPanel.Controls.Add(countLabel);

            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = orgGroup.IsExpanded
            };

            var modelsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                FlowDirection = FlowDirection.TopDown
            };

            foreach (var model in orgGroup.Models)
            {
                var modelCard = CreateModelCard(model);
                modelsFlow.Controls.Add(modelCard);
            }

            contentPanel.Controls.Add(modelsFlow);

            expanderPanel.Controls.Add(contentPanel);
            expanderPanel.Controls.Add(headerPanel);

            // Toggle expansion on header click
            headerPanel.Click += (s, e) =>
            {
                contentPanel.Visible = !contentPanel.Visible;
                orgGroup.IsExpanded = contentPanel.Visible;
            };

            return expanderPanel;
        }

        private void AddCustomModelButton_Click(object sender, EventArgs e)
        {
            _viewModel.AddCustomModelCommand.Execute(null);
        }
    }
}