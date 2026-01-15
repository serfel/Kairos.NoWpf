using System;
using System.Drawing;
using System.Windows.Forms;

namespace KaiROS.AI.Views
{
    public partial class AddCustomModelForm : Form
    {
        public TextBox ModelNameTextBox { get; private set; }
        public TextBox ModelPathTextBox { get; private set; }
        public TextBox DescriptionTextBox { get; private set; }
        public TextBox SizeTextBox { get; private set; }
        public TextBox MinRamTextBox { get; private set; }
        public Button BrowseButton { get; private set; }
        public Button SaveButton { get; private set; }
        public Button CancelButton { get; private set; }

        public AddCustomModelForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void InitializeComponent()
        {
            this.Text = "Add Custom Model";
            this.Size = new Size(500, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = SystemColors.Window;
        }

        private void SetupUI()
        {
            this.SuspendLayout();

            // Labels and inputs
            var nameLabel = new Label
            {
                Text = "Model Name:",
                Location = new Point(20, 20),
                Size = new Size(100, 25)
            };

            ModelNameTextBox = new TextBox
            {
                Location = new Point(130, 20),
                Size = new Size(330, 25)
            };

            var pathLabel = new Label
            {
                Text = "Model Path:",
                Location = new Point(20, 60),
                Size = new Size(100, 25)
            };

            ModelPathTextBox = new TextBox
            {
                Location = new Point(130, 60),
                Size = new Size(290, 25)
            };

            BrowseButton = new Button
            {
                Text = "Browse",
                Location = new Point(430, 60),
                Size = new Size(30, 25)
            };
            BrowseButton.Click += BrowseButton_Click;

            var descriptionLabel = new Label
            {
                Text = "Description:",
                Location = new Point(20, 100),
                Size = new Size(100, 25)
            };

            DescriptionTextBox = new TextBox
            {
                Location = new Point(130, 100),
                Size = new Size(330, 80),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            var sizeLabel = new Label
            {
                Text = "Size (GB):",
                Location = new Point(20, 200),
                Size = new Size(100, 25)
            };

            SizeTextBox = new TextBox
            {
                Location = new Point(130, 200),
                Size = new Size(100, 25)
            };

            var ramLabel = new Label
            {
                Text = "Min RAM (GB):",
                Location = new Point(20, 240),
                Size = new Size(100, 25)
            };

            MinRamTextBox = new TextBox
            {
                Location = new Point(130, 240),
                Size = new Size(100, 25)
            };

            // Buttons
            SaveButton = new Button
            {
                Text = "Save",
                Location = new Point(250, 300),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            SaveButton.FlatAppearance.BorderSize = 0;
            SaveButton.DialogResult = DialogResult.OK;

            CancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(370, 300),
                Size = new Size(100, 35),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            CancelButton.DialogResult = DialogResult.Cancel;

            // Add controls to form
            this.Controls.Add(nameLabel);
            this.Controls.Add(ModelNameTextBox);
            this.Controls.Add(pathLabel);
            this.Controls.Add(ModelPathTextBox);
            this.Controls.Add(BrowseButton);
            this.Controls.Add(descriptionLabel);
            this.Controls.Add(DescriptionTextBox);
            this.Controls.Add(sizeLabel);
            this.Controls.Add(SizeTextBox);
            this.Controls.Add(ramLabel);
            this.Controls.Add(MinRamTextBox);
            this.Controls.Add(SaveButton);
            this.Controls.Add(CancelButton);

            this.ResumeLayout();
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Model Files|*.bin;*.gguf;*.ggml|All Files|*.*";
                openFileDialog.Title = "Select Model File";
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ModelPathTextBox.Text = openFileDialog.FileName;
                    
                    // If model name is empty, suggest a name based on file name
                    if (string.IsNullOrEmpty(ModelNameTextBox.Text))
                    {
                        ModelNameTextBox.Text = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    }
                }
            }
        }
    }
}