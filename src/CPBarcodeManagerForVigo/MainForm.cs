using CPBarcodeManagerForVigo.Models;
using CPBarcodeManagerForVigo.Services;

namespace CPBarcodeManagerForVigo;

public sealed class MainForm : Form
{
    private readonly TextBox _sourceFolderText = new();
    private readonly Label _sourceFolderLabel = new();
    private readonly Label _outputFolderLabel = new();
    private readonly TextBox _outputFolderText = new();
    private readonly NumericUpDown _skipChars = new();
    private readonly NumericUpDown _takeChars = new();
    private readonly NumericUpDown _bottomMargin = new();
    private readonly TextBox _logText = new();
    private readonly Label _statusLabel = new();
    private readonly Label _summaryLabel = new();
    private readonly ProgressBar _progressBar = new();
    private readonly Button _startButton = new();
    private readonly Button _previewButton = new();

    private AppSettings _settings;

    public MainForm()
    {
        _settings = SettingsService.Load();
        BuildUi();
        LoadSettingsIntoUi();
    }

    private void BuildUi()
    {
        Text = "CP Barcode Manager for Vigo";
        Width = 980;
        Height = 720;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(850, 620);

        var tabControl = new TabControl { Dock = DockStyle.Fill, Padding = new Point(10, 3) };
        var processingTab = new TabPage("Processing");
        var settingsTab = new TabPage("Settings");

        BuildProcessingTab(processingTab);
        BuildSettingsTab(settingsTab);

        tabControl.TabPages.Add(processingTab);
        tabControl.TabPages.Add(settingsTab);

        Controls.Add(tabControl);
    }

    private void BuildProcessingTab(TabPage processingTab)
    {
        var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(16) };
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 390));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        processingTab.Controls.Add(main);

        var left = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8
        };
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.Controls.Add(left, 0, 0);

        var title = new Label
        {
            Text = "CP Barcode Manager for Vigo",
            Font = new Font(Font.FontFamily, 16, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 20)
        };
        left.Controls.Add(title);

        left.Controls.Add(CreateReadOnlyFolderDisplay("Source Folder", _sourceFolderLabel));
        left.Controls.Add(CreateReadOnlyFolderDisplay("Output Folder", _outputFolderLabel));
        left.Controls.Add(CreateButtons());

        _summaryLabel.Text = "Files Found: 0\nProcessed: 0\nSuccessful: 0\nErrors: 0";
        _summaryLabel.AutoSize = true;
        _summaryLabel.Margin = new Padding(0, 16, 0, 12);
        left.Controls.Add(_summaryLabel);

        _progressBar.Dock = DockStyle.Top;
        _progressBar.Height = 24;
        _progressBar.Margin = new Padding(0, 0, 16, 12);
        left.Controls.Add(_progressBar);

        _statusLabel.Text = "Ready.";
        _statusLabel.AutoSize = true;
        _statusLabel.Margin = new Padding(0, 4, 0, 0);
        left.Controls.Add(_statusLabel);

        var right = new GroupBox
        {
            Text = "Processing Log",
            Dock = DockStyle.Fill,
            Padding = new Padding(12)
        };
        main.Controls.Add(right, 1, 0);

        _logText.Multiline = true;
        _logText.ScrollBars = ScrollBars.Vertical;
        _logText.ReadOnly = true;
        _logText.Dock = DockStyle.Fill;
        _logText.Font = new Font("Consolas", 10);
        _logText.Text = "Ready...";
        right.Controls.Add(_logText);
    }

    private void BuildSettingsTab(TabPage settingsTab)
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(16) };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        settingsTab.Controls.Add(panel);

        panel.Controls.Add(CreateFolderPicker("Import Folder", _sourceFolderText, BrowseSource));
        panel.Controls.Add(CreateFolderPicker("Output Folder", _outputFolderText, BrowseOutput));
        panel.Controls.Add(CreateSettingsGroup());

        var saveButton = new Button { Text = "Save Settings", Width = 120, Height = 34, Margin = new Padding(0, 16, 0, 0) };
        saveButton.Click += SaveSettingsClick;
        panel.Controls.Add(saveButton);
    }

    private Control CreateReadOnlyFolderDisplay(string label, Label displayLabel)
    {
        var group = new GroupBox { Text = label, Dock = DockStyle.Top, Height = 60, Margin = new Padding(0, 0, 16, 12) };
        displayLabel.Dock = DockStyle.Fill;
        displayLabel.Padding = new Padding(10, 0, 10, 0);
        displayLabel.TextAlign = ContentAlignment.MiddleLeft;
        group.Controls.Add(displayLabel);
        return group;
    }

    private Control CreateFolderPicker(string label, TextBox textBox, EventHandler browseHandler)
    {
        var group = new GroupBox { Text = label, Dock = DockStyle.Top, Height = 78, Margin = new Padding(0, 0, 16, 12) };
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(8) };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        textBox.Dock = DockStyle.Fill;
        textBox.ReadOnly = true;
        var browse = new Button { Text = "Browse", Dock = DockStyle.Fill };
        browse.Click += browseHandler;
        panel.Controls.Add(textBox, 0, 0);
        panel.Controls.Add(browse, 1, 0);
        group.Controls.Add(panel);
        return group;
    }

    private Control CreateSettingsGroup()
    {
        var group = new GroupBox { Text = "Barcode Settings", Dock = DockStyle.Top, Height = 180, Margin = new Padding(0, 0, 16, 12) };
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10) };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));

        AddSettingRow(table, "Barcode Type", new Label { Text = "Code 128", AutoSize = true });
        ConfigureNumber(_skipChars, 0, 20);
        AddSettingRow(table, "Skip first characters", _skipChars);
        ConfigureNumber(_takeChars, 1, 30);
        AddSettingRow(table, "Read next digits", _takeChars);
        AddSettingRow(table, "Apply to", new Label { Text = "First page only", AutoSize = true });
        AddSettingRow(table, "Position", new Label { Text = "Bottom Center", AutoSize = true });
        ConfigureNumber(_bottomMargin, 0, 100);
        AddSettingRow(table, "Bottom margin (mm)", _bottomMargin);

        group.Controls.Add(table);
        return group;
    }

    private static void ConfigureNumber(NumericUpDown input, int min, int max)
    {
        input.Minimum = min;
        input.Maximum = max;
        input.Dock = DockStyle.Left;
        input.Width = 80;
    }

    private static void AddSettingRow(TableLayoutPanel table, string labelText, Control control)
    {
        int row = table.RowCount;
        table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var label = new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 5, 0, 5) };
        control.Anchor = AnchorStyles.Left;
        control.Margin = new Padding(0, 5, 0, 5);
        table.Controls.Add(label, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private Control CreateButtons()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, FlowDirection = FlowDirection.LeftToRight, Margin = new Padding(0, 0, 16, 0) };
        _previewButton.Text = "Preview First File";
        _previewButton.Width = 130;
        _previewButton.Height = 34;
        _previewButton.Click += PreviewFirstFile;

        _startButton.Text = "Start Processing";
        _startButton.Width = 130;
        _startButton.Height = 34;
        _startButton.Click += StartProcessing;

        var exit = new Button { Text = "Exit", Width = 80, Height = 34 };
        exit.Click += (_, _) => Close();

        panel.Controls.Add(_previewButton);
        panel.Controls.Add(_startButton);
        panel.Controls.Add(exit);
        return panel;
    }

    private void LoadSettingsIntoUi()
    {
        _sourceFolderText.Text = _settings.SourceFolder;
        _outputFolderText.Text = _settings.OutputFolder;
        _sourceFolderLabel.Text = _settings.SourceFolder;
        _outputFolderLabel.Text = _settings.OutputFolder;
        _skipChars.Value = _settings.SkipCharacters;
        _takeChars.Value = _settings.TakeCharacters;
        _bottomMargin.Value = _settings.BottomMarginMm;
    }

    private void SaveUiIntoSettings()
    {
        _settings.SourceFolder = _sourceFolderText.Text;
        _settings.OutputFolder = _outputFolderText.Text;
        _settings.SkipCharacters = (int)_skipChars.Value;
        _settings.TakeCharacters = (int)_takeChars.Value;
        _settings.BottomMarginMm = (int)_bottomMargin.Value;
        SettingsService.Save(_settings);
    }

    private void SaveSettingsClick(object? sender, EventArgs e)
    {
        SaveUiIntoSettings();
        // Update read-only labels on Processing tab
        _sourceFolderLabel.Text = _settings.SourceFolder;
        _outputFolderLabel.Text = _settings.OutputFolder;
        MessageBox.Show(this, "Settings have been saved.", "Save Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BrowseSource(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog { Description = "Select folder with SAP Customer Paperwork PDFs" };
        if (Directory.Exists(_sourceFolderText.Text)) dlg.SelectedPath = _sourceFolderText.Text;
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _sourceFolderText.Text = dlg.SelectedPath;
            if (string.IsNullOrWhiteSpace(_outputFolderText.Text))
                _outputFolderText.Text = Path.Combine(dlg.SelectedPath, "Output");
        }
    }

    private void BrowseOutput(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog { Description = "Select output folder" };
        if (Directory.Exists(_outputFolderText.Text)) dlg.SelectedPath = _outputFolderText.Text;
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _outputFolderText.Text = dlg.SelectedPath;
        }
    }

    private void PreviewFirstFile(object? sender, EventArgs e)
    {
        var validation = ValidateFolders();
        if (validation != null)
        {
            MessageBox.Show(this, validation, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var firstPdf = Directory.GetFiles(_settings.SourceFolder, "*.pdf").OrderBy(x => x).FirstOrDefault();
        if (firstPdf == null)
        {
            MessageBox.Show(this, "No PDF files found in Source Folder.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var value = BarcodeValueExtractor.Extract(Path.GetFileNameWithoutExtension(firstPdf), _settings.SkipCharacters, _settings.TakeCharacters);
            MessageBox.Show(this,
                $"First file:\n{Path.GetFileName(firstPdf)}\n\nBarcode value:\n{value}\n\nPlacement:\nFirst page only, Bottom Center, {_settings.BottomMarginMm} mm bottom margin.",
                "Preview",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void StartProcessing(object? sender, EventArgs e)
    {
        var validation = ValidateFolders();
        if (validation != null)
        {
            MessageBox.Show(this, validation, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var pdfFiles = Directory.GetFiles(_settings.SourceFolder, "*.pdf").OrderBy(x => x).ToList();
        if (pdfFiles.Count == 0)
        {
            MessageBox.Show(this, "No PDF files found in Source Folder.", "Start Processing", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        ToggleProcessing(false);
        _logText.Clear();
        _progressBar.Minimum = 0;
        _progressBar.Maximum = pdfFiles.Count;
        _progressBar.Value = 0;

        var service = new PdfBarcodeService();
        var results = new List<ProcessResult>();

        for (int i = 0; i < pdfFiles.Count; i++)
        {
            var file = pdfFiles[i];
            _statusLabel.Text = $"Processing {i + 1} / {pdfFiles.Count}: {Path.GetFileName(file)}";
            Application.DoEvents();

            var result = service.ProcessPdf(file, _settings);
            results.Add(result);

            AppendLog(result);
            _progressBar.Value = i + 1;
            UpdateSummary(pdfFiles.Count, results);
            Application.DoEvents();
        }

        var logPath = CsvLogService.SaveLog(_settings.OutputFolder, results);
        _statusLabel.Text = "Completed.";
        _logText.AppendText(Environment.NewLine + $"CSV log saved: {logPath}" + Environment.NewLine);
        ToggleProcessing(true);

        MessageBox.Show(this,
            $"Processing completed.\n\nFiles found: {pdfFiles.Count}\nSuccessful: {results.Count(r => r.Status == "Success")}\nErrors: {results.Count(r => r.Status != "Success")}\n\nLog:\n{logPath}",
            "Completed",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private string? ValidateFolders()
    {
        if (string.IsNullOrWhiteSpace(_settings.SourceFolder) || !Directory.Exists(_settings.SourceFolder))
            return "Please select a valid Source Folder.";

        if (string.IsNullOrWhiteSpace(_settings.OutputFolder))
            return "Please select a valid Output Folder.";

        return null;
    }

    private void ToggleProcessing(bool enabled)
    {
        _startButton.Enabled = enabled;
        _previewButton.Enabled = enabled;
    }

    private void AppendLog(ProcessResult result)
    {
        _logText.AppendText($"{DateTime.Now:HH:mm:ss} | {result.Status} | {result.FileName} | Ref: {result.CustomerReference} | {result.Message}{Environment.NewLine}");
    }

    private void UpdateSummary(int total, IReadOnlyList<ProcessResult> results)
    {
        var successful = results.Count(r => r.Status == "Success");
        var errors = results.Count - successful;
        _summaryLabel.Text = $"Files Found: {total}\nProcessed: {results.Count}\nSuccessful: {successful}\nErrors: {errors}";
    }
}
