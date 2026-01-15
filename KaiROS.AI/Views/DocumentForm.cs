using System;
using System.Drawing;
using System.Windows.Forms;
using KaiROS.AI.ViewModels;

namespace KaiROS.AI.Views
{
    public partial class DocumentForm : Form
    {
        private DocumentViewModel _viewModel;
        private Panel headerPanel;
        private Panel contentPanel;
        private Panel uploadPanel;
        private ListBox documentsListBox;
        private Button uploadButton;
        private Button analyzeButton;
        private Button clearButton;
        private TextBox documentPreview;
        private Label selectedDocumentLabel;
        private ProgressBar progressBar;

        public DocumentForm(DocumentViewModel viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
            SetupUI();
            BindViewModel();
        }

        private void InitializeComponent()
        {
            this.Text = "Documents";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = SystemColors.Window;
        }

        private void SetupUI()
        {
            this.SuspendLayout();

            // Header panel
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(245, 245, 245)
            };

            var titleLabel = new Label
            {
                Text = "Document Management",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(0, 10),
                Size = new Size(300, 30)
            };

            uploadButton = new Button
            {
                Text = "Upload Document",
                Size = new Size(150, 35),
                Location = new Point(headerPanel.Width - 170, 10),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            uploadButton.FlatAppearance.BorderSize = 0;
            uploadButton.Click += UploadButton_Click;

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(uploadButton);

            // Upload panel
            uploadPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            var instructionsLabel = new Label
            {
                Text = "Upload PDF, DOCX, TXT, or other document formats for analysis",
                Location = new Point(0, 10),
                Size = new Size(500, 25),
                ForeColor = Color.Gray
            };

            uploadPanel.Controls.Add(instructionsLabel);

            // Content panel with splitter
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 300,
                Panel1 = { Padding = new Padding(10) },
                Panel2 = { Padding = new Padding(10) }
            };

            // Left panel - Document list
            var listLabel = new Label
            {
                Text = "Uploaded Documents",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(0, 5),
                Size = new Size(200, 25)
            };

            documentsListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 30
            };
            documentsListBox.DrawItem += DocumentsListBox_DrawItem;
            documentsListBox.SelectedIndexChanged += DocumentsListBox_SelectedIndexChanged;

            var listPanel = new Panel { Dock = DockStyle.Fill };
            listPanel.Controls.Add(documentsListBox);
            listPanel.Controls.Add(listLabel);

            // Right panel - Document preview and actions
            var previewLabel = new Label
            {
                Text = "Document Preview",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(0, 5),
                Size = new Size(200, 25)
            };

            documentPreview = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 10)
            };

            var actionsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                Padding = new Padding(5)
            };

            analyzeButton = new Button
            {
                Text = "Analyze Document",
                Size = new Size(150, 35),
                Location = new Point(5, 10),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            analyzeButton.FlatAppearance.BorderSize = 0;
            analyzeButton.Click += AnalyzeButton_Click;

            clearButton = new Button
            {
                Text = "Remove Document",
                Size = new Size(150, 35),
                Location = new Point(analyzeButton.Right + 10, 10),
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            clearButton.FlatAppearance.BorderSize = 0;
            clearButton.Click += ClearButton_Click;

            selectedDocumentLabel = new Label
            {
                Text = "No document selected",
                Location = new Point(5, 55),
                Size = new Size(400, 25),
                ForeColor = Color.Gray
            };

            progressBar = new ProgressBar
            {
                Location = new Point(5, 80),
                Size = new Size(300, 15),
                Visible = false
            };

            actionsPanel.Controls.Add(analyzeButton);
            actionsPanel.Controls.Add(clearButton);
            actionsPanel.Controls.Add(selectedDocumentLabel);
            actionsPanel.Controls.Add(progressBar);

            var previewPanel = new Panel { Dock = DockStyle.Fill };
            previewPanel.Controls.Add(actionsPanel);
            previewPanel.Controls.Add(documentPreview);
            previewPanel.Controls.Add(previewLabel);

            splitContainer.Panel1.Controls.Add(listPanel);
            splitContainer.Panel2.Controls.Add(previewPanel);

            contentPanel.Controls.Add(splitContainer);

            // Layout main form
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
            mainLayout.Controls.Add(uploadPanel, 0, 1);
            mainLayout.Controls.Add(contentPanel, 0, 2);

            this.Controls.Add(mainLayout);
            this.ResumeLayout();
        }

        private void BindViewModel()
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            RefreshDocumentList();
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
            RefreshDocumentList();
            
            // Update progress bar
            if (_viewModel.IsProcessing)
            {
                progressBar.Visible = true;
                progressBar.Value = Math.Min(100, Math.Max(0, (int)(_viewModel.ProcessingProgress * 100)));
            }
            else
            {
                progressBar.Visible = false;
            }
        }

        private void RefreshDocumentList()
        {
            documentsListBox.Items.Clear();
            foreach (var doc in _viewModel.Documents)
            {
                documentsListBox.Items.Add(doc);
            }
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Documents|*.pdf;*.docx;*.txt;*.rtf;*.odt;*.html|PDF Files|*.pdf|Word Documents|*.docx|Text Files|*.txt|All Files|*.*";
                openFileDialog.Multiselect = true;
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var fileName in openFileDialog.FileNames)
                    {
                        _viewModel.UploadDocumentCommand.Execute(fileName);
                    }
                }
            }
        }

        private void AnalyzeButton_Click(object sender, EventArgs e)
        {
            if (documentsListBox.SelectedItem != null)
            {
                var selectedDoc = documentsListBox.SelectedItem.ToString();
                _viewModel.AnalyzeDocumentCommand.Execute(selectedDoc);
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            if (documentsListBox.SelectedItem != null)
            {
                var selectedDoc = documentsListBox.SelectedItem.ToString();
                _viewModel.RemoveDocumentCommand.Execute(selectedDoc);
            }
        }

        private void DocumentsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (documentsListBox.SelectedItem != null)
            {
                var selectedDoc = documentsListBox.SelectedItem.ToString();
                selectedDocumentLabel.Text = $"Selected: {selectedDoc}";
                
                // For preview, we'll just show the filename and some metadata
                documentPreview.Text = $"Document: {selectedDoc}\n\n" +
                                      $"Type: {System.IO.Path.GetExtension(selectedDoc)}\n" +
                                      $"Size: N/A\n\n" +
                                      "Full content preview would be displayed here.";
            }
            else
            {
                selectedDocumentLabel.Text = "No document selected";
                documentPreview.Text = "Select a document to preview";
            }
        }

        private void DocumentsListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            
            if (e.Index >= 0)
            {
                var document = documentsListBox.Items[e.Index].ToString();
                var textColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected ? 
                    Color.White : Color.Black;
                var bgColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected ? 
                    Color.FromArgb(99, 102, 241) : e.BackColor;
                
                using (var brush = new SolidBrush(bgColor))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
                
                using (var brush = new SolidBrush(textColor))
                {
                    e.Graphics.DrawString(document, e.Font, brush, e.Bounds.X, e.Bounds.Y + 5);
                }
                
                if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
                {
                    e.DrawFocusRectangle();
                }
            }
        }
    }
}