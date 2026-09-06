using CPBarcodeManagerForVigo.Models;
using CPBarcodeManagerForVigo.Services;

namespace CPBarcodeManagerForVigo;

public sealed class MainForm : Form
{
    // Processing tab controls
    private readonly TextBox _processingSourceFolderText = new();
    private readonly TextBox _processingOutputFolderText = new();
    private readonly NumericUpDown _skipChars = new();
    private readonly NumericUpDown _takeChars = new();
    private readonly NumericUpDown _bottomMargin = new();
    private readonly NumericUpDown _barcodeWidthMm = new();
    private readonly NumericUpDown _barcodeHeightMm = new();
    private readonly ComboBox _barcodePosition = new();
    private readonly TextBox _logText = new();
    private readonly Label _statusLabel = new();
    private readonly Label _summaryLabel = new();
    private readonly ProgressBar _progressBar = new();
    private readonly Button _startButton = new();
    private readonly Button _previewButton = new();
    private readonly Label _monitorStatusLabel = new();

    // Settings tab controls
    private readonly TextBox _settingsSourceFolderText = new();
    private readonly TextBox _settingsOutputFolderText = new();
    private readonly CheckBox _autoMonitorEnabled = new();
    private readonly NumericUpDown _monitorIntervalSeconds = new();

    private readonly System.Windows.Forms.Timer _monitorTimer = new();
    private readonly Dictionary<string, FileSnapshot> _fileSnapshots =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _blockedFiles =
        new(StringComparer.OrdinalIgnoreCase);

    private bool _monitorProcessing;
    private AppSettings _settings;

    private sealed record FileSnapshot(
        long Length,
        DateTime LastWriteUtc,
        int StableChecks);

    public MainForm()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

        _settings = SettingsService.Load();

        BuildUi();
        LoadSettingsIntoUi();
        ConfigureAutomaticMonitoring();

        FormClosed += (_, _) => _monitorTimer.Stop();
    }

    private void BuildUi()
    {
        Text = "CP Barcode Manager for Vigo";

        // Start large enough to be usable immediately on business laptops.
        ClientSize = new Size(1040, 950);
        MinimumSize = new Size(900, 700);

        StartPosition = FormStartPosition.CenterScreen;

        var tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Padding = new Point(10, 5)
        };

        Controls.Add(tabControl);

        var processingTab = new TabPage
        {
            Text = "Processing",
            Padding = new Padding(12),
            AutoScroll = false
        };
        tabControl.TabPages.Add(processingTab);
        BuildProcessingTab(processingTab);

        var settingsTab = new TabPage
        {
            Text = "Settings",
            Padding = new Padding(12),
            AutoScroll = true
        };
        tabControl.TabPages.Add(settingsTab);
        BuildSettingsTab(settingsTab);
    }

    private void BuildProcessingTab(TabPage tab)
    {
        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 9,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        for (int i = 0; i < 8; i++)
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        tab.Controls.Add(main);

        // HEADER
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "CP Barcode Manager for Vigo",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 2, 0, 2)
        };

        var version = new Label
        {
            Text = "Version 0.2.0",
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(16, 5, 0, 2)
        };

        header.Controls.Add(title, 0, 0);
        header.Controls.Add(version, 1, 0);
        main.Controls.Add(header, 0, 0);

        // FOLDERS
        var foldersGroup = NewGroupBox("Folders");

        var folders = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0)
        };
        folders.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        folders.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        folders.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        folders.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        folders.Controls.Add(NewLabel("Import Folder:"), 0, 0);
        ConfigureReadOnlyPathBox(_processingSourceFolderText);
        folders.Controls.Add(_processingSourceFolderText, 1, 0);

        folders.Controls.Add(NewLabel("Output Folder:"), 0, 1);
        ConfigureReadOnlyPathBox(_processingOutputFolderText);
        folders.Controls.Add(_processingOutputFolderText, 1, 1);

        foldersGroup.Controls.Add(folders);
        main.Controls.Add(foldersGroup, 0, 1);

        // BARCODE SETTINGS
        var barcodeGroup = NewGroupBox("Barcode Settings");

        var barcode = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 8,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0)
        };
        barcode.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        barcode.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        for (int i = 0; i < 8; i++)
            barcode.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AddSettingRow(barcode, 0, "Barcode Type:", NewValueLabel("Code 128"));

        ConfigureNumber(_skipChars, 0, 20);
        AddSettingRow(barcode, 1, "Skip first characters:", _skipChars);

        ConfigureNumber(_takeChars, 1, 30);
        AddSettingRow(barcode, 2, "Read next digits:", _takeChars);

        AddSettingRow(barcode, 3, "Apply to:", NewValueLabel("Every page"));
        AddSettingRow(barcode, 4, "Position:", NewValueLabel("Configured in Settings"));
        AddSettingRow(barcode, 5, "Barcode size:", NewValueLabel("Configured in Settings"));
        AddSettingRow(barcode, 6, "Bottom margin:", NewValueLabel("Configured in Settings"));
        AddSettingRow(barcode, 7, "Source PDFs:", NewValueLabel("Removed only after verified output"));

        barcodeGroup.Controls.Add(barcode);
        main.Controls.Add(barcodeGroup, 0, 2);

        // BUTTONS
        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8)
        };

        ConfigureActionButton(_previewButton, "Preview", 110);
        _previewButton.Click += PreviewFirstFile;

        ConfigureActionButton(_startButton, "Start Processing", 155);
        _startButton.Click += StartProcessing;

        var exit = new Button();
        ConfigureActionButton(exit, "Exit", 90);
        exit.Click += (_, _) => Close();

        buttons.Controls.Add(_previewButton);
        buttons.Controls.Add(_startButton);
        buttons.Controls.Add(exit);
        main.Controls.Add(buttons, 0, 3);

        // PROGRESS
        _progressBar.Dock = DockStyle.Top;
        _progressBar.Height = 22;
        _progressBar.Margin = new Padding(0, 0, 0, 5);
        main.Controls.Add(_progressBar, 0, 4);

        // STATUS
        _statusLabel.Text = "Ready.";
        _statusLabel.AutoSize = true;
        _statusLabel.Margin = new Padding(0, 0, 0, 3);
        main.Controls.Add(_statusLabel, 0, 5);

        // SUMMARY
        _summaryLabel.Text = "Files Found: 0    Processed: 0    Ready for Vigo: 0    Needs Attention: 0    Critical: 0";
        _summaryLabel.AutoSize = true;
        _summaryLabel.Margin = new Padding(0, 0, 0, 4);
        main.Controls.Add(_summaryLabel, 0, 6);

        // AUTOMATION STATUS
        _monitorStatusLabel.Text = "Automation: Disabled";
        _monitorStatusLabel.AutoSize = true;
        _monitorStatusLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _monitorStatusLabel.Margin = new Padding(0, 0, 0, 8);
        main.Controls.Add(_monitorStatusLabel, 0, 7);

        // LOG
        var logGroup = new GroupBox
        {
            Text = "Processing Log",
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 8, 10, 10),
            Margin = new Padding(0)
        };

        _logText.Multiline = true;
        _logText.ReadOnly = true;
        _logText.ScrollBars = ScrollBars.Vertical;
        _logText.Dock = DockStyle.Fill;
        _logText.Font = new Font("Consolas", 9F);
        _logText.Text = "Ready...";
        _logText.WordWrap = false;

        logGroup.Controls.Add(_logText);
        main.Controls.Add(logGroup, 0, 8);
    }

    private void BuildSettingsTab(TabPage tab)
    {
        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 5,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        for (int i = 0; i < 5; i++)
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        tab.Controls.Add(main);

        // HEADER
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, 10)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var title = new Label
        {
            Text = "Company OneDrive / SharePoint Folders",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 2, 0, 4)
        };

        var subtitle = new Label
        {
            Text = "Choose the folders used for incoming Customer Paperwork and Vigo-ready PDFs.",
            AutoSize = true,
            Margin = new Padding(0)
        };

        header.Controls.Add(title, 0, 0);
        header.Controls.Add(subtitle, 0, 1);
        main.Controls.Add(header, 0, 0);

        // FOLDER LOCATIONS
        var folderGroup = NewGroupBox("Folder Locations");

        var folderTable = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 3,
            RowCount = 3,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0)
        };

        folderTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        folderTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        folderTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        for (int i = 0; i < 3; i++)
            folderTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        folderTable.Controls.Add(NewLabel("Import Folder:"), 0, 0);
        ConfigureSettingsPathBox(_settingsSourceFolderText);
        folderTable.Controls.Add(_settingsSourceFolderText, 1, 0);

        var importButton = NewSelectFolderButton();
        importButton.Click += BrowseSettingsSource;
        folderTable.Controls.Add(importButton, 2, 0);

        folderTable.Controls.Add(NewLabel("Output Folder:"), 0, 1);
        ConfigureSettingsPathBox(_settingsOutputFolderText);
        folderTable.Controls.Add(_settingsOutputFolderText, 1, 1);

        var outputButton = NewSelectFolderButton();
        outputButton.Click += BrowseSettingsOutput;
        folderTable.Controls.Add(outputButton, 2, 1);

        var saveButton = new Button
        {
            Text = "Save Settings",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(130, 34),
            Padding = new Padding(10, 3, 10, 3),
            Margin = new Padding(0, 8, 0, 0),
            Anchor = AnchorStyles.Left
        };
        saveButton.Click += SaveSettingsClick;

        folderTable.Controls.Add(new Label { AutoSize = true }, 0, 2);
        folderTable.Controls.Add(saveButton, 1, 2);

        folderGroup.Controls.Add(folderTable);
        main.Controls.Add(folderGroup, 0, 1);

        // BARCODE CONFIGURATION
        var barcodeConfigGroup = NewGroupBox("Barcode Configuration");

        var barcodeConfig = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 3,
            RowCount = 4,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0)
        };

        barcodeConfig.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        barcodeConfig.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        barcodeConfig.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        for (int i = 0; i < 4; i++)
            barcodeConfig.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        barcodeConfig.Controls.Add(NewLabel("Position:"), 0, 0);

        _barcodePosition.DropDownStyle = ComboBoxStyle.DropDownList;
        _barcodePosition.Items.AddRange(
            new object[] { "Bottom Left", "Bottom Centre", "Bottom Right" });
        _barcodePosition.Width = 230;
        _barcodePosition.Margin = new Padding(0, 2, 0, 6);
        barcodeConfig.Controls.Add(_barcodePosition, 1, 0);

        ConfigureNumber(_barcodeWidthMm, 25, 100);
        barcodeConfig.Controls.Add(NewLabel("Barcode width:"), 0, 1);
        barcodeConfig.Controls.Add(_barcodeWidthMm, 1, 1);
        barcodeConfig.Controls.Add(NewValueLabel("mm"), 2, 1);

        ConfigureNumber(_barcodeHeightMm, 8, 30);
        barcodeConfig.Controls.Add(NewLabel("Barcode height:"), 0, 2);
        barcodeConfig.Controls.Add(_barcodeHeightMm, 1, 2);
        barcodeConfig.Controls.Add(NewValueLabel("mm"), 2, 2);

        ConfigureNumber(_bottomMargin, 5, 50);
        barcodeConfig.Controls.Add(NewLabel("Bottom margin:"), 0, 3);
        barcodeConfig.Controls.Add(_bottomMargin, 1, 3);
        barcodeConfig.Controls.Add(NewValueLabel("mm"), 2, 3);

        barcodeConfigGroup.Controls.Add(barcodeConfig);
        main.Controls.Add(barcodeConfigGroup, 0, 2);

        // PROCESSING CONFIGURATION
        var configGroup = NewGroupBox("Processing Configuration");

        var config = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 5,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0)
        };
        config.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        config.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        for (int i = 0; i < 5; i++)
            config.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AddConfigRow(config, 0, "Barcode Type", "Code 128");
        AddConfigRow(config, 1, "Filename rule", "Skip first 2 characters, read next 8 digits");
        AddConfigRow(config, 2, "PDF pages", "Every page");
        AddConfigRow(config, 3, "Successful source", "Delete only after verified output");
        AddConfigRow(config, 4, "Processing error", "Move original to Output as ERROR_...");

        configGroup.Controls.Add(config);
        main.Controls.Add(configGroup, 0, 3);

        // AUTOMATION
        var automationGroup = NewGroupBox("Automatic Folder Monitoring");

        var automation = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 3,
            RowCount = 3,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0)
        };

        automation.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        automation.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        automation.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        _autoMonitorEnabled.Text = "Automatically monitor Import Folder";
        _autoMonitorEnabled.AutoSize = true;
        _autoMonitorEnabled.Margin = new Padding(0, 4, 0, 8);
        automation.Controls.Add(_autoMonitorEnabled, 0, 0);
        automation.SetColumnSpan(_autoMonitorEnabled, 3);

        ConfigureNumber(_monitorIntervalSeconds, 2, 60);
        automation.Controls.Add(NewLabel("Scan interval:"), 0, 1);
        automation.Controls.Add(_monitorIntervalSeconds, 1, 1);
        automation.Controls.Add(NewValueLabel("seconds"), 2, 1);

        var note = new Label
        {
            Text = "A new PDF must be unchanged across two scans and available for exclusive read before it is processed.",
            AutoSize = true,
            MaximumSize = new Size(780, 0),
            Margin = new Padding(0, 6, 0, 0)
        };
        automation.Controls.Add(note, 0, 2);
        automation.SetColumnSpan(note, 3);

        automationGroup.Controls.Add(automation);
        main.Controls.Add(automationGroup, 0, 4);
    }

    private static GroupBox NewGroupBox(string text)
    {
        return new GroupBox
        {
            Text = text,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(12, 8, 12, 10),
            Margin = new Padding(0, 0, 0, 8)
        };
    }

    private static Label NewLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            AutoEllipsis = false,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 5, 12, 6)
        };
    }

    private static Label NewValueLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            AutoEllipsis = false,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 5, 0, 5)
        };
    }

    private static Button NewSelectFolderButton()
    {
        return new Button
        {
            Text = "Select Folder",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(120, 32),
            Padding = new Padding(8, 2, 8, 2),
            Margin = new Padding(8, 2, 0, 6),
            Anchor = AnchorStyles.Left
        };
    }

    private static void ConfigureActionButton(Button button, string text, int minimumWidth)
    {
        button.Text = text;
        button.AutoSize = true;
        button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        button.MinimumSize = new Size(minimumWidth, 34);
        button.Padding = new Padding(10, 3, 10, 3);
        button.Margin = new Padding(0, 0, 8, 0);
    }

    private static void ConfigureReadOnlyPathBox(TextBox textBox)
    {
        textBox.ReadOnly = true;
        textBox.BackColor = SystemColors.Control;
        textBox.Dock = DockStyle.Fill;
        textBox.Margin = new Padding(0, 2, 0, 6);
        textBox.MinimumSize = new Size(200, 0);
    }

    private static void ConfigureSettingsPathBox(TextBox textBox)
    {
        textBox.ReadOnly = false;
        textBox.Dock = DockStyle.Fill;
        textBox.Margin = new Padding(0, 2, 0, 6);
        textBox.MinimumSize = new Size(250, 0);
    }

    private static void AddSettingRow(TableLayoutPanel table, int row, string labelText, Control valueControl)
    {
        table.Controls.Add(NewLabel(labelText), 0, row);
        valueControl.Anchor = AnchorStyles.Left;
        table.Controls.Add(valueControl, 1, row);
    }

    private static void AddConfigRow(TableLayoutPanel table, int row, string labelText, string valueText)
    {
        table.Controls.Add(NewLabel(labelText + ":"), 0, row);
        table.Controls.Add(NewValueLabel(valueText), 1, row);
    }

    private static void ConfigureNumber(NumericUpDown input, int min, int max)
    {
        input.Minimum = min;
        input.Maximum = max;
        input.AutoSize = false;
        input.Width = 90;
        input.Margin = new Padding(0, 2, 0, 4);
    }

    private void LoadSettingsIntoUi()
    {
        _processingSourceFolderText.Text = _settings.SourceFolder;
        _processingOutputFolderText.Text = _settings.OutputFolder;
        _settingsSourceFolderText.Text = _settings.SourceFolder;
        _settingsOutputFolderText.Text = _settings.OutputFolder;
        _skipChars.Value = _settings.SkipCharacters;
        _takeChars.Value = _settings.TakeCharacters;
        _bottomMargin.Value = _settings.BottomMarginMm;
        _barcodeWidthMm.Value = _settings.BarcodeWidthMm;
        _barcodeHeightMm.Value = _settings.BarcodeHeightMm;
        _autoMonitorEnabled.Checked = _settings.AutoMonitorEnabled;
        _monitorIntervalSeconds.Value = Math.Clamp(
            _settings.MonitorIntervalSeconds,
            (int)_monitorIntervalSeconds.Minimum,
            (int)_monitorIntervalSeconds.Maximum);

        var positionIndex = _barcodePosition.Items.IndexOf(_settings.BarcodePosition);
        _barcodePosition.SelectedIndex = positionIndex >= 0 ? positionIndex : 1;
    }

    private void UpdateProcessingFoldersDisplay()
    {
        _processingSourceFolderText.Text = _settings.SourceFolder;
        _processingOutputFolderText.Text = _settings.OutputFolder;
    }

    private void SaveUiIntoSettings()
    {
        // Folder paths are controlled in Settings; the Processing tab is read-only.
        _settings.SourceFolder = _processingSourceFolderText.Text;
        _settings.OutputFolder = _processingOutputFolderText.Text;
        _settings.SkipCharacters = (int)_skipChars.Value;
        _settings.TakeCharacters = (int)_takeChars.Value;
        _settings.BottomMarginMm = (int)_bottomMargin.Value;
        _settings.BarcodeWidthMm = (int)_barcodeWidthMm.Value;
        _settings.BarcodeHeightMm = (int)_barcodeHeightMm.Value;
        _settings.BarcodePosition =
            _barcodePosition.SelectedItem?.ToString() ?? "Bottom Centre";
        _settings.AutoMonitorEnabled = _autoMonitorEnabled.Checked;
        _settings.MonitorIntervalSeconds = (int)_monitorIntervalSeconds.Value;

        SettingsService.Save(_settings);
    }

    private void BrowseSettingsSource(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Select Import Folder"
        };

        if (Directory.Exists(_settingsSourceFolderText.Text))
            dlg.SelectedPath = _settingsSourceFolderText.Text;

        if (dlg.ShowDialog(this) == DialogResult.OK)
            _settingsSourceFolderText.Text = dlg.SelectedPath;
    }

    private void BrowseSettingsOutput(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Select Output Folder"
        };

        if (Directory.Exists(_settingsOutputFolderText.Text))
            dlg.SelectedPath = _settingsOutputFolderText.Text;

        if (dlg.ShowDialog(this) == DialogResult.OK)
            _settingsOutputFolderText.Text = dlg.SelectedPath;
    }

    private void SaveSettingsClick(object? sender, EventArgs e)
    {
        var importFolder = _settingsSourceFolderText.Text.Trim();
        var outputFolder = _settingsOutputFolderText.Text.Trim();

        if (string.IsNullOrWhiteSpace(importFolder) ||
            string.IsNullOrWhiteSpace(outputFolder))
        {
            MessageBox.Show(
                this,
                "Both Import Folder and Output Folder must be selected.",
                "Validation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (!Directory.Exists(importFolder))
        {
            MessageBox.Show(
                this,
                "Import Folder does not exist.",
                "Validation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (!Directory.Exists(outputFolder))
        {
            MessageBox.Show(
                this,
                "Output Folder does not exist.",
                "Validation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        _settings.SourceFolder = importFolder;
        _settings.OutputFolder = outputFolder;
        _settings.BarcodePosition =
            _barcodePosition.SelectedItem?.ToString() ?? "Bottom Centre";
        _settings.BarcodeWidthMm = (int)_barcodeWidthMm.Value;
        _settings.BarcodeHeightMm = (int)_barcodeHeightMm.Value;
        _settings.BottomMarginMm = (int)_bottomMargin.Value;
        _settings.AutoMonitorEnabled = _autoMonitorEnabled.Checked;
        _settings.MonitorIntervalSeconds = (int)_monitorIntervalSeconds.Value;

        SettingsService.Save(_settings);

        UpdateProcessingFoldersDisplay();
        ApplyMonitoringSettings();

        MessageBox.Show(
            this,
            "Settings saved successfully.",
            "Success",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void PreviewFirstFile(object? sender, EventArgs e)
    {
        SaveUiIntoSettings();

        var validation = ValidateFolders();
        if (validation != null)
        {
            MessageBox.Show(
                this,
                validation,
                "Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var firstPdf = Directory
            .GetFiles(_settings.SourceFolder, "*.pdf")
            .OrderBy(x => x)
            .FirstOrDefault();

        if (firstPdf == null)
        {
            MessageBox.Show(
                this,
                "No PDF files found in Import Folder.",
                "Preview",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        try
        {
            var value = BarcodeValueExtractor.Extract(
                Path.GetFileNameWithoutExtension(firstPdf),
                _settings.SkipCharacters,
                _settings.TakeCharacters);

            MessageBox.Show(
                this,
                $"First file:\n{Path.GetFileName(firstPdf)}\n\n" +
                $"Barcode value:\n{value}\n\n" +
                $"Placement:\nEvery page, {_settings.BarcodePosition}, " +
                $"{_settings.BarcodeWidthMm} x {_settings.BarcodeHeightMm} mm, " +
                $"{_settings.BottomMarginMm} mm bottom margin.",
                "Preview",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Preview Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void StartProcessing(object? sender, EventArgs e)
    {
        SaveUiIntoSettings();

        var validation = ValidateFolders();
        if (validation != null)
        {
            MessageBox.Show(
                this,
                validation,
                "Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var pdfFiles = Directory
            .GetFiles(_settings.SourceFolder, "*.pdf")
            .OrderBy(x => x)
            .ToList();

        if (pdfFiles.Count == 0)
        {
            MessageBox.Show(
                this,
                "No PDF files found in Import Folder.",
                "Start Processing",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        ToggleProcessing(false);
        _logText.Clear();

        _progressBar.Minimum = 0;
        _progressBar.Maximum = pdfFiles.Count;
        _progressBar.Value = 0;

        var results = new List<ProcessResult>();
        var settingsSnapshot = CloneSettings(_settings);

        for (int i = 0; i < pdfFiles.Count; i++)
        {
            var file = pdfFiles[i];

            _statusLabel.Text =
                $"Processing {i + 1} / {pdfFiles.Count}: {Path.GetFileName(file)}";

            Application.DoEvents();

            var result = ProcessFileForVigoWorkflow(file, settingsSnapshot);
            results.Add(result);

            if (result.Status == "Critical Error")
                _blockedFiles.Add(file);

            AppendLog(result);
            _progressBar.Value = i + 1;
            UpdateSummary(pdfFiles.Count, results);

            Application.DoEvents();
        }

        var logPath = CsvLogService.SaveLog(_settings.OutputFolder, results);

        var sourceRemaining = CountSourcePdfs();

        _statusLabel.Text =
            sourceRemaining == 0
                ? "Completed. Nothing left behind."
                : $"Completed with attention required. Source folder remaining: {sourceRemaining}";

        _logText.AppendText(
            Environment.NewLine +
            $"CSV log saved: {logPath}" +
            Environment.NewLine +
            $"Source folder remaining: {sourceRemaining}" +
            Environment.NewLine);

        ToggleProcessing(true);

        var ready = results.Count(r => r.Status == "Ready for Vigo");
        var attention = results.Count(r => r.Status == "Needs Attention");
        var critical = results.Count(r => r.Status == "Critical Error");

        MessageBox.Show(
            this,
            $"Processing completed.\n\n" +
            $"Files found: {pdfFiles.Count}\n" +
            $"Ready for Vigo: {ready}\n" +
            $"Needs attention: {attention}\n" +
            $"Critical errors: {critical}\n" +
            $"Source folder remaining: {sourceRemaining}\n\n" +
            $"Log:\n{logPath}",
            critical == 0 && sourceRemaining == 0
                ? "Completed"
                : "Completed with Attention Required",
            MessageBoxButtons.OK,
            critical == 0 && sourceRemaining == 0
                ? MessageBoxIcon.Information
                : MessageBoxIcon.Warning);
    }

    private void ConfigureAutomaticMonitoring()
    {
        _monitorTimer.Tick += MonitorTimerTick;
        ApplyMonitoringSettings();
    }

    private void ApplyMonitoringSettings()
    {
        _monitorTimer.Stop();
        _fileSnapshots.Clear();

        var seconds = Math.Clamp(_settings.MonitorIntervalSeconds, 2, 60);
        _monitorTimer.Interval = seconds * 1000;

        if (_settings.AutoMonitorEnabled)
        {
            _monitorStatusLabel.Text =
                $"Automation: Monitoring every {seconds} seconds";
            _monitorTimer.Start();
        }
        else
        {
            _monitorStatusLabel.Text = "Automation: Disabled";
        }
    }

    private async void MonitorTimerTick(object? sender, EventArgs e)
    {
        if (_monitorProcessing || !_settings.AutoMonitorEnabled)
            return;

        var validation = ValidateFolders();
        if (validation != null)
        {
            _monitorStatusLabel.Text = "Automation: Waiting for valid folder settings";
            return;
        }

        List<string> stableFiles;

        try
        {
            stableFiles = FindStablePdfFiles();
        }
        catch (Exception ex)
        {
            _monitorStatusLabel.Text = $"Automation: Folder check failed - {ex.Message}";
            return;
        }

        if (stableFiles.Count == 0)
        {
            _monitorStatusLabel.Text =
                $"Automation: Monitoring - {DateTime.Now:HH:mm:ss}";
            return;
        }

        _monitorProcessing = true;
        ToggleProcessing(false);

        try
        {
            var settingsSnapshot = CloneSettings(_settings);

            foreach (var file in stableFiles)
            {
                if (!File.Exists(file) || _blockedFiles.Contains(file))
                    continue;

                _statusLabel.Text =
                    $"Automatic processing: {Path.GetFileName(file)}";
                _monitorStatusLabel.Text =
                    $"Automation: Processing {Path.GetFileName(file)}";

                var result = await Task.Run(
                    () => ProcessFileForVigoWorkflow(file, settingsSnapshot));

                AppendLog(result);

                if (result.Status == "Critical Error")
                {
                    _blockedFiles.Add(file);
                }
                else
                {
                    _fileSnapshots.Remove(file);
                }
            }

            var sourceRemaining = CountSourcePdfs();

            _summaryLabel.Text =
                $"Automatic mode    Source Folder Remaining: {sourceRemaining}";

            _statusLabel.Text =
                sourceRemaining == 0
                    ? "Automatic processing complete. Nothing left behind."
                    : $"Automatic processing complete. Source folder remaining: {sourceRemaining}";
        }
        finally
        {
            ToggleProcessing(true);
            _monitorProcessing = false;

            if (_settings.AutoMonitorEnabled)
            {
                _monitorStatusLabel.Text =
                    $"Automation: Monitoring every {_settings.MonitorIntervalSeconds} seconds";
            }
        }
    }

    private List<string> FindStablePdfFiles()
    {
        var currentFiles = Directory
            .GetFiles(_settings.SourceFolder, "*.pdf")
            .OrderBy(x => x)
            .ToList();

        var existingSet =
            new HashSet<string>(
                currentFiles,
                StringComparer.OrdinalIgnoreCase);

        foreach (var knownPath in _fileSnapshots.Keys.ToList())
        {
            if (!existingSet.Contains(knownPath))
                _fileSnapshots.Remove(knownPath);
        }

        var stableFiles = new List<string>();

        foreach (var file in currentFiles)
        {
            if (_blockedFiles.Contains(file))
                continue;

            var info = new FileInfo(file);
            var currentLength = info.Length;
            var currentWriteUtc = info.LastWriteTimeUtc;

            if (_fileSnapshots.TryGetValue(file, out var previous) &&
                previous.Length == currentLength &&
                previous.LastWriteUtc == currentWriteUtc)
            {
                var updated = previous with
                {
                    StableChecks = previous.StableChecks + 1
                };

                _fileSnapshots[file] = updated;

                if (updated.StableChecks >= 1 &&
                    CanOpenForExclusiveRead(file))
                {
                    stableFiles.Add(file);
                }
            }
            else
            {
                _fileSnapshots[file] =
                    new FileSnapshot(
                        currentLength,
                        currentWriteUtc,
                        0);
            }
        }

        return stableFiles;
    }

    private static bool CanOpenForExclusiveRead(string filePath)
    {
        try
        {
            using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None);

            return stream.Length >= 0;
        }
        catch
        {
            return false;
        }
    }

    private ProcessResult ProcessFileForVigoWorkflow(
        string sourcePdfPath,
        AppSettings settings)
    {
        if (!WaitUntilFileReady(sourcePdfPath))
        {
            return new ProcessResult
            {
                FileName = Path.GetFileName(sourcePdfPath),
                Status = "Critical Error",
                Message =
                    "Source PDF is still locked or changing. It was left in the Import Folder."
            };
        }

        var service = new PdfBarcodeService();
        var result = service.ProcessPdf(sourcePdfPath, settings);

        if (result.Status == "Success")
        {
            if (TryDeleteSourceWithRetry(sourcePdfPath))
            {
                result.Status = "Ready for Vigo";
                result.Message +=
                    " Output verified. Source PDF removed from Import Folder.";
                return result;
            }

            result.Status = "Critical Error";
            result.Message +=
                " Output was created and verified, but the source PDF could not be removed. " +
                "The source was left in the Import Folder to prevent silent data loss.";
            return result;
        }

        var processingError = result.Message;

        try
        {
            var errorOutputPath =
                MoveOriginalToNeedsAttention(
                    sourcePdfPath,
                    settings.OutputFolder);

            return new ProcessResult
            {
                FileName = Path.GetFileName(sourcePdfPath),
                CustomerReference = result.CustomerReference,
                OutputPath = errorOutputPath,
                Status = "Needs Attention",
                Message =
                    $"Barcode processing failed: {processingError} " +
                    $"Original PDF moved to Output Folder as {Path.GetFileName(errorOutputPath)}."
            };
        }
        catch (Exception moveEx)
        {
            return new ProcessResult
            {
                FileName = Path.GetFileName(sourcePdfPath),
                CustomerReference = result.CustomerReference,
                Status = "Critical Error",
                Message =
                    $"Barcode processing failed: {processingError} " +
                    $"The original PDF could not be moved to Output Folder: {moveEx.Message} " +
                    "The source was left in the Import Folder."
            };
        }
    }

    private static bool WaitUntilFileReady(string filePath)
    {
        long? previousLength = null;
        DateTime? previousWriteUtc = null;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                var info = new FileInfo(filePath);

                if (!info.Exists)
                    return false;

                var length = info.Length;
                var writeUtc = info.LastWriteTimeUtc;

                if (previousLength == length &&
                    previousWriteUtc == writeUtc &&
                    CanOpenForExclusiveRead(filePath))
                {
                    return true;
                }

                previousLength = length;
                previousWriteUtc = writeUtc;
            }
            catch
            {
                // Retry below.
            }

            System.Threading.Thread.Sleep(500);
        }

        return CanOpenForExclusiveRead(filePath);
    }

    private static bool TryDeleteSourceWithRetry(string sourcePdfPath)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                if (!File.Exists(sourcePdfPath))
                    return true;

                File.Delete(sourcePdfPath);

                if (!File.Exists(sourcePdfPath))
                    return true;
            }
            catch
            {
                // Retry below.
            }

            System.Threading.Thread.Sleep(500);
        }

        return !File.Exists(sourcePdfPath);
    }

    private static string MoveOriginalToNeedsAttention(
        string sourcePdfPath,
        string outputFolder)
    {
        Directory.CreateDirectory(outputFolder);

        var originalName = Path.GetFileName(sourcePdfPath);
        var destinationPath =
            GetUniqueErrorOutputPath(
                outputFolder,
                $"ERROR_{originalName}");

        File.Copy(
            sourcePdfPath,
            destinationPath,
            overwrite: false);

        var sourceInfo = new FileInfo(sourcePdfPath);
        var destinationInfo = new FileInfo(destinationPath);

        if (!destinationInfo.Exists ||
            destinationInfo.Length != sourceInfo.Length)
        {
            try
            {
                if (File.Exists(destinationPath))
                    File.Delete(destinationPath);
            }
            catch
            {
                // Best-effort cleanup only.
            }

            throw new IOException(
                "Error-file copy verification failed.");
        }

        if (!TryDeleteSourceWithRetry(sourcePdfPath))
        {
            throw new IOException(
                $"The file was copied to {destinationPath}, but the source could not be removed.");
        }

        return destinationPath;
    }

    private static string GetUniqueErrorOutputPath(
        string outputFolder,
        string fileName)
    {
        var path = Path.Combine(outputFolder, fileName);

        if (!File.Exists(path))
            return path;

        var name = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);

        for (int i = 1; i < 10000; i++)
        {
            var candidate =
                Path.Combine(
                    outputFolder,
                    $"{name}_{i}{ext}");

            if (!File.Exists(candidate))
                return candidate;
        }

        throw new IOException(
            "Unable to create a unique ERROR output file name.");
    }

    private int CountSourcePdfs()
    {
        try
        {
            return Directory.Exists(_settings.SourceFolder)
                ? Directory.GetFiles(_settings.SourceFolder, "*.pdf").Length
                : 0;
        }
        catch
        {
            return -1;
        }
    }

    private static AppSettings CloneSettings(AppSettings settings)
    {
        return new AppSettings
        {
            SourceFolder = settings.SourceFolder,
            OutputFolder = settings.OutputFolder,
            SkipCharacters = settings.SkipCharacters,
            TakeCharacters = settings.TakeCharacters,
            BarcodePosition = settings.BarcodePosition,
            BarcodeWidthMm = settings.BarcodeWidthMm,
            BarcodeHeightMm = settings.BarcodeHeightMm,
            BottomMarginMm = settings.BottomMarginMm,
            AutoMonitorEnabled = settings.AutoMonitorEnabled,
            MonitorIntervalSeconds = settings.MonitorIntervalSeconds
        };
    }

    private string? ValidateFolders()
    {
        if (string.IsNullOrWhiteSpace(_settings.SourceFolder) ||
            !Directory.Exists(_settings.SourceFolder))
        {
            return "Please configure a valid Import Folder in the Settings tab.";
        }

        if (string.IsNullOrWhiteSpace(_settings.OutputFolder) ||
            !Directory.Exists(_settings.OutputFolder))
        {
            return "Please configure a valid Output Folder in the Settings tab.";
        }

        var sourceFullPath =
            Path.GetFullPath(_settings.SourceFolder)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        var outputFullPath =
            Path.GetFullPath(_settings.OutputFolder)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (string.Equals(
            sourceFullPath,
            outputFullPath,
            StringComparison.OrdinalIgnoreCase))
        {
            return "Import Folder and Output Folder must be different.";
        }

        return null;
    }

    private void ToggleProcessing(bool enabled)
    {
        _startButton.Enabled = enabled;
        _previewButton.Enabled = enabled;
    }

    private void AppendLog(ProcessResult result)
    {
        _logText.AppendText(
            $"{DateTime.Now:HH:mm:ss} | {result.Status} | {result.FileName} | " +
            $"Ref: {result.CustomerReference} | {result.Message}{Environment.NewLine}");
    }

    private void UpdateSummary(int total, IReadOnlyList<ProcessResult> results)
    {
        var ready = results.Count(r => r.Status == "Ready for Vigo");
        var attention = results.Count(r => r.Status == "Needs Attention");
        var critical = results.Count(r => r.Status == "Critical Error");

        _summaryLabel.Text =
            $"Files Found: {total}    " +
            $"Processed: {results.Count}    " +
            $"Ready for Vigo: {ready}    " +
            $"Needs Attention: {attention}    " +
            $"Critical: {critical}";
    }
}