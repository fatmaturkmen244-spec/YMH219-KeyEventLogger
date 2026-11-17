using KeyEventLoggerApp.Models;
using KeyEventLoggerApp.Services;

namespace KeyEventLoggerApp;

public sealed class MainForm : Form
{
    private readonly KeyLoggerSession _session = new();
    private readonly LogFileExporter _exporter = new();

    private readonly Label _lblRecordingBadge;
    private readonly Label _lblFocusState;
    private readonly Button _btnStart;
    private readonly Button _btnStop;
    private readonly Button _btnClear;
    private readonly Button _btnExport;
    private readonly CheckBox _chkKeyUp;
    private readonly CheckBox _chkKeyPress;

    private readonly TextBox _txtSandbox;
    private readonly ListBox _lstEvents;
    private readonly RichTextBox _rtbTerminalPreview;

    private readonly DateTimePicker _dtpStart;
    private readonly DateTimePicker _dtpEnd;
    private readonly CheckBox _chkEnableDateFilter;
    private readonly TextBox _txtKeySearch;

    private readonly Label _lblTotalKeysVal;
    private readonly Label _lblTotalEventsVal;
    private readonly Label _lblLastKeyVal;
    private readonly Panel _pnlTopKeysContainer;
    private readonly Label _lblFooterBanner;

    public MainForm()
    {
        Text = "YMH219 — Key Event Logger | Eğitsel Klavye Olay Analizörü";
        ClientSize = new Size(980, 660);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(11, 15, 25);
        ForeColor = Color.FromArgb(226, 232, 240);
        KeyPreview = true;

        // 1. Üst Başlık ve Güvenlik Kapsamı Şeridi (Cyber-Terminal Header)
        Panel headerPanel = new()
        {
            Dock = DockStyle.Top,
            Height = 76,
            BackColor = Color.FromArgb(15, 23, 42)
        };
        Controls.Add(headerPanel);

        Panel neonTopLine = new()
        {
            Dock = DockStyle.Top,
            Height = 3,
            BackColor = Color.FromArgb(16, 185, 129)
        };
        headerPanel.Controls.Add(neonTopLine);

        Label lblTitle = new()
        {
            Text = "⌨  KEY EVENT LOGGER // TERMINAL ANALYZER",
            Font = new Font("Consolas", 15.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 211, 153),
            Location = new Point(20, 14),
            AutoSize = true
        };
        headerPanel.Controls.Add(lblTitle);

        Label lblScopeNotice = new()
        {
            Text = "Eğitsel Kapsam: Yalnızca bu uygulama penceresi odaktayken (Focused) basılan tuş olayları yakalanır.",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(148, 163, 184),
            Location = new Point(22, 44),
            AutoSize = true
        };
        headerPanel.Controls.Add(lblScopeNotice);

        _lblRecordingBadge = new Label
        {
            Text = "● KAYIT AKTİF (DİNLENİYOR)",
            Font = new Font("Consolas", 10.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(6, 78, 59),
            BackColor = Color.FromArgb(52, 211, 153),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(710, 14),
            Size = new Size(250, 30)
        };
        headerPanel.Controls.Add(_lblRecordingBadge);

        _lblFocusState = new Label
        {
            Text = "Pencere Odağı: AKTİF",
            Font = new Font("Consolas", 8.8f),
            ForeColor = Color.FromArgb(56, 189, 248),
            Location = new Point(712, 48),
            Size = new Size(248, 20),
            TextAlign = ContentAlignment.MiddleRight
        };
        headerPanel.Controls.Add(_lblFocusState);

        // 2. Kontrol Butonları Çubuğu
        Panel toolbarPanel = new()
        {
            Location = new Point(18, 88),
            Size = new Size(944, 52),
            BackColor = Color.FromArgb(17, 24, 39)
        };
        Controls.Add(toolbarPanel);

        _btnStart = CreateToolbarButton("▶ Kaydı Başlat", 12, 9, 138, Color.FromArgb(16, 185, 129), Color.FromArgb(6, 46, 34));
        _btnStart.Click += (_, _) => SetRecordingState(true);
        toolbarPanel.Controls.Add(_btnStart);

        _btnStop = CreateToolbarButton("■ Kaydı Durdur", 158, 9, 138, Color.FromArgb(245, 158, 11), Color.FromArgb(30, 20, 5));
        _btnStop.Click += (_, _) => SetRecordingState(false);
        toolbarPanel.Controls.Add(_btnStop);

        _btnClear = CreateToolbarButton("🗑 Listeyi Temizle", 304, 9, 148, Color.FromArgb(239, 68, 68), Color.White);
        _btnClear.Click += (_, _) => ClearAllLogs();
        toolbarPanel.Controls.Add(_btnClear);

        _btnExport = CreateToolbarButton("💾 .TXT Olarak Kaydet", 460, 9, 172, Color.FromArgb(14, 165, 233), Color.FromArgb(8, 28, 48));
        _btnExport.Click += (_, _) => ExportLogsWithDialog();
        toolbarPanel.Controls.Add(_btnExport);

        _chkKeyUp = new CheckBox
        {
            Text = "KeyUp Kaydet",
            Checked = true,
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(203, 213, 225),
            Location = new Point(652, 16),
            AutoSize = true
        };
        _chkKeyUp.CheckedChanged += (_, _) => _session.LogKeyUpEvents = _chkKeyUp.Checked;
        toolbarPanel.Controls.Add(_chkKeyUp);

        _chkKeyPress = new CheckBox
        {
            Text = "KeyPress (Karakter) Kaydet",
            Checked = true,
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(203, 213, 225),
            Location = new Point(768, 16),
            AutoSize = true
        };
        _chkKeyPress.CheckedChanged += (_, _) => _session.LogKeyPressEvents = _chkKeyPress.Checked;
        toolbarPanel.Controls.Add(_chkKeyPress);

        // 3. Sol/Orta Alan: Test Yazım Kutusu + Filtreleme + Olay Listesi
        Panel leftContainer = new()
        {
            Location = new Point(18, 150),
            Size = new Size(636, 466),
            BackColor = Color.FromArgb(17, 24, 39)
        };
        Controls.Add(leftContainer);

        Label lblSandbox = new()
        {
            Text = "CANLI KLAVYE TEST ALANI (Form aktifken buraya veya doğrudan klavyeye basabilirsiniz):",
            Font = new Font("Consolas", 9.2f, FontStyle.Bold),
            ForeColor = Color.FromArgb(56, 189, 248),
            Location = new Point(14, 10),
            AutoSize = true
        };
        leftContainer.Controls.Add(lblSandbox);

        _txtSandbox = new TextBox
        {
            Location = new Point(14, 32),
            Size = new Size(608, 28),
            Font = new Font("Consolas", 11f),
            BackColor = Color.FromArgb(9, 13, 22),
            ForeColor = Color.FromArgb(110, 231, 183),
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "Klavye olaylarını test etmek için buraya metin yazın..."
        };
        leftContainer.Controls.Add(_txtSandbox);

        // Tarih/Saat Aralığı ve Tuş Filtreleme Paneli (İsteğe Bağlı Geliştirme)
        Panel filterBar = new()
        {
            Location = new Point(14, 68),
            Size = new Size(608, 42),
            BackColor = Color.FromArgb(24, 33, 52)
        };
        leftContainer.Controls.Add(filterBar);

        _chkEnableDateFilter = new CheckBox
        {
            Text = "Saat Filtresi:",
            Font = new Font("Segoe UI", 8.8f),
            ForeColor = Color.FromArgb(203, 213, 225),
            Location = new Point(10, 11),
            AutoSize = true
        };
        _chkEnableDateFilter.CheckedChanged += (_, _) => RefreshDisplayedLogs();
        filterBar.Controls.Add(_chkEnableDateFilter);

        _dtpStart = new DateTimePicker
        {
            Format = DateTimePickerFormat.Time,
            ShowUpDown = true,
            Font = new Font("Consolas", 9f),
            Location = new Point(104, 9),
            Size = new Size(88, 24),
            Value = DateTime.Today
        };
        _dtpStart.ValueChanged += (_, _) => RefreshDisplayedLogs();
        filterBar.Controls.Add(_dtpStart);

        Label lblDash = new()
        {
            Text = "—",
            ForeColor = Color.FromArgb(148, 163, 184),
            Location = new Point(195, 12),
            AutoSize = true
        };
        filterBar.Controls.Add(lblDash);

        _dtpEnd = new DateTimePicker
        {
            Format = DateTimePickerFormat.Time,
            ShowUpDown = true,
            Font = new Font("Consolas", 9f),
            Location = new Point(212, 9),
            Size = new Size(88, 24),
            Value = DateTime.Today.AddDays(1).AddSeconds(-1)
        };
        _dtpEnd.ValueChanged += (_, _) => RefreshDisplayedLogs();
        filterBar.Controls.Add(_dtpEnd);

        Label lblSearch = new()
        {
            Text = "Tuş Ara:",
            Font = new Font("Segoe UI", 8.8f),
            ForeColor = Color.FromArgb(203, 213, 225),
            Location = new Point(312, 12),
            AutoSize = true
        };
        filterBar.Controls.Add(lblSearch);

        _txtKeySearch = new TextBox
        {
            Location = new Point(366, 9),
            Size = new Size(142, 24),
            Font = new Font("Consolas", 9.5f),
            BackColor = Color.FromArgb(11, 15, 25),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "Örn: Space, Enter"
        };
        _txtKeySearch.TextChanged += (_, _) => RefreshDisplayedLogs();
        filterBar.Controls.Add(_txtKeySearch);

        Button btnResetFilter = new()
        {
            Text = "Sıfırla",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            BackColor = Color.FromArgb(51, 65, 85),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Location = new Point(516, 8),
            Size = new Size(82, 26),
            TabStop = false
        };
        btnResetFilter.FlatAppearance.BorderSize = 0;
        btnResetFilter.Click += (_, _) =>
        {
            _chkEnableDateFilter.Checked = false;
            _txtKeySearch.Clear();
            RefreshDisplayedLogs();
        };
        filterBar.Controls.Add(btnResetFilter);

        Label lblListHeader = new()
        {
            Text = "YAKALANAN KLAVYE OLAYLARI LİSTESİ (ListBox):",
            Font = new Font("Consolas", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(16, 185, 129),
            Location = new Point(14, 118),
            AutoSize = true
        };
        leftContainer.Controls.Add(lblListHeader);

        _lstEvents = new ListBox
        {
            Location = new Point(14, 138),
            Size = new Size(608, 210),
            Font = new Font("Consolas", 9.2f),
            BackColor = Color.FromArgb(7, 10, 18),
            ForeColor = Color.FromArgb(167, 243, 208),
            BorderStyle = BorderStyle.FixedSingle,
            HorizontalScrollbar = true
        };
        leftContainer.Controls.Add(_lstEvents);

        Label lblRtbHeader = new()
        {
            Text = "ZENGİN METİN GÜNLÜK AKIŞI (RichTextBox):",
            Font = new Font("Consolas", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(56, 189, 248),
            Location = new Point(14, 354),
            AutoSize = true
        };
        leftContainer.Controls.Add(lblRtbHeader);

        _rtbTerminalPreview = new RichTextBox
        {
            Location = new Point(14, 374),
            Size = new Size(608, 78),
            Font = new Font("Consolas", 8.8f),
            BackColor = Color.FromArgb(7, 10, 18),
            ForeColor = Color.FromArgb(125, 211, 252),
            BorderStyle = BorderStyle.FixedSingle,
            ReadOnly = true
        };
        leftContainer.Controls.Add(_rtbTerminalPreview);

        // 4. Sağ Panel: Tuş İstatistikleri ve En Sık Kullanılan Tuşlar (İsteğe Bağlı Geliştirme)
        Panel rightStatsPanel = new()
        {
            Location = new Point(666, 150),
            Size = new Size(296, 466),
            BackColor = Color.FromArgb(17, 24, 39)
        };
        Controls.Add(rightStatsPanel);

        Label lblStatsTitle = new()
        {
            Text = "📊 TUŞ İSTATİSTİKLERİ",
            Font = new Font("Consolas", 11.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 211, 153),
            Location = new Point(16, 12),
            AutoSize = true
        };
        rightStatsPanel.Controls.Add(lblStatsTitle);

        _lblTotalKeysVal = CreateMetricBox(rightStatsPanel, "TOPLAM TUŞ BASIMI (KeyDown)", "0", 16, 42, Color.FromArgb(52, 211, 153));
        _lblTotalEventsVal = CreateMetricBox(rightStatsPanel, "TOPLAM OLAY KAYDI", "0", 16, 108, Color.FromArgb(56, 189, 248));
        _lblLastKeyVal = CreateMetricBox(rightStatsPanel, "SON BASILAN TUŞ & KODU", "-", 16, 174, Color.FromArgb(251, 191, 36));

        Label lblTopKeysHeader = new()
        {
            Text = "EN SIK KULLANILAN TUŞLAR (TOP 6):",
            Font = new Font("Consolas", 9.2f, FontStyle.Bold),
            ForeColor = Color.FromArgb(167, 139, 250),
            Location = new Point(16, 248),
            AutoSize = true
        };
        rightStatsPanel.Controls.Add(lblTopKeysHeader);

        _pnlTopKeysContainer = new Panel
        {
            Location = new Point(16, 270),
            Size = new Size(264, 182),
            BackColor = Color.FromArgb(11, 15, 25)
        };
        rightStatsPanel.Controls.Add(_pnlTopKeysContainer);

        // 5. Alt Bilgi Çubuğu
        _lblFooterBanner = new Label
        {
            Text = "Durum: Kayıt sistemi aktif. Tuş olayları milisaniye hassasiyetinde yakalanıyor.",
            Font = new Font("Consolas", 9f),
            ForeColor = Color.FromArgb(167, 243, 208),
            BackColor = Color.FromArgb(15, 23, 42),
            Dock = DockStyle.Bottom,
            Height = 32,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(18, 0, 0, 0)
        };
        Controls.Add(_lblFooterBanner);

        // Olay abonelikleri
        KeyDown += MainForm_KeyDown;
        KeyUp += MainForm_KeyUp;
        KeyPress += MainForm_KeyPress;
        Activated += (_, _) => _lblFocusState.Text = "Pencere Odağı: AKTİF (Dinleniyor)";
        Deactivate += (_, _) => _lblFocusState.Text = "Pencere Odağı: PASİF (Kayıt Dışı)";

        SetRecordingState(true);
    }

    private static Button CreateToolbarButton(string text, int x, int y, int width, Color bgColor, Color fgColor)
    {
        Button btn = new()
        {
            Text = text,
            Font = new Font("Segoe UI Semibold", 9.2f, FontStyle.Bold),
            BackColor = bgColor,
            ForeColor = fgColor,
            FlatStyle = FlatStyle.Flat,
            Location = new Point(x, y),
            Size = new Size(width, 34),
            Cursor = Cursors.Hand,
            TabStop = false
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private static Label CreateMetricBox(Control parent, string caption, string initialValue, int x, int y, Color accentColor)
    {
        Panel box = new()
        {
            Location = new Point(x, y),
            Size = new Size(264, 58),
            BackColor = Color.FromArgb(11, 15, 25)
        };

        Panel leftStripe = new()
        {
            Dock = DockStyle.Left,
            Width = 4,
            BackColor = accentColor
        };
        box.Controls.Add(leftStripe);

        Label lblCap = new()
        {
            Text = caption,
            Font = new Font("Segoe UI", 7.8f, FontStyle.Bold),
            ForeColor = Color.FromArgb(148, 163, 184),
            Location = new Point(12, 6),
            AutoSize = true
        };
        box.Controls.Add(lblCap);

        Label lblVal = new()
        {
            Text = initialValue,
            Font = new Font("Consolas", 14.5f, FontStyle.Bold),
            ForeColor = accentColor,
            Location = new Point(12, 24),
            Size = new Size(244, 28)
        };
        box.Controls.Add(lblVal);

        parent.Controls.Add(box);
        return lblVal;
    }

    private void SetRecordingState(bool active)
    {
        if (active)
        {
            _session.StartRecording();
            _lblRecordingBadge.Text = "● KAYIT AKTİF (DİNLENİYOR)";
            _lblRecordingBadge.BackColor = Color.FromArgb(52, 211, 153);
            _lblRecordingBadge.ForeColor = Color.FromArgb(6, 78, 59);
            _btnStart.Enabled = false;
            _btnStop.Enabled = true;
            _lblFooterBanner.Text = "Durum: Klavye olay kaydı AKTİF. Yalnızca bu pencere odaktayken basılan tuşlar listelenir.";
        }
        else
        {
            _session.StopRecording();
            _lblRecordingBadge.Text = "■ KAYIT DURDURULDU";
            _lblRecordingBadge.BackColor = Color.FromArgb(245, 158, 11);
            _lblRecordingBadge.ForeColor = Color.FromArgb(28, 25, 23);
            _btnStart.Enabled = true;
            _btnStop.Enabled = false;
            _lblFooterBanner.Text = "Durum: Klavye olay kaydı DURDURULDU. Kayıtları inceleyebilir veya .txt dosyasına aktarabilirsiniz.";
        }

        UpdateStatisticsPanel();
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        KeyLogEntry? entry = _session.RecordKeyDown(e.KeyCode, e.KeyValue, e.Modifiers);
        if (entry != null)
        {
            _lblLastKeyVal.Text = $"{entry.KeyName} (Kod: {entry.KeyValue})";
            AppendEntryToUi(entry);
        }
    }

    private void MainForm_KeyUp(object? sender, KeyEventArgs e)
    {
        KeyLogEntry? entry = _session.RecordKeyUp(e.KeyCode, e.KeyValue, e.Modifiers);
        if (entry != null)
        {
            AppendEntryToUi(entry);
        }
    }

    private void MainForm_KeyPress(object? sender, KeyPressEventArgs e)
    {
        KeyLogEntry? entry = _session.RecordKeyPress(e.KeyChar);
        if (entry != null)
        {
            AppendEntryToUi(entry);
        }
    }

    private void AppendEntryToUi(KeyLogEntry entry)
    {
        if (MatchesCurrentFilter(entry))
        {
            string line = entry.ToFormattedLogLine();
            _lstEvents.Items.Add(line);
            _lstEvents.TopIndex = Math.Max(0, _lstEvents.Items.Count - 1);

            _rtbTerminalPreview.AppendText(line + Environment.NewLine);
            _rtbTerminalPreview.ScrollToCaret();
        }

        UpdateStatisticsPanel();
    }

    private bool MatchesCurrentFilter(KeyLogEntry entry)
    {
        if (_chkEnableDateFilter.Checked)
        {
            TimeSpan t = entry.Timestamp.TimeOfDay;
            if (t < _dtpStart.Value.TimeOfDay || t > _dtpEnd.Value.TimeOfDay)
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(_txtKeySearch.Text))
        {
            string term = _txtKeySearch.Text.Trim();
            bool match = entry.KeyName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                         entry.EventKind.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                         (entry.Character != null && entry.Character.Contains(term, StringComparison.OrdinalIgnoreCase));
            if (!match)
            {
                return false;
            }
        }

        return true;
    }

    private void RefreshDisplayedLogs()
    {
        DateTime? start = null;
        DateTime? end = null;

        if (_chkEnableDateFilter.Checked)
        {
            start = DateTime.Today.Add(_dtpStart.Value.TimeOfDay);
            end = DateTime.Today.Add(_dtpEnd.Value.TimeOfDay);
        }

        IReadOnlyList<KeyLogEntry> filtered = _session.FilterEntries(start, end, _txtKeySearch.Text);

        _lstEvents.BeginUpdate();
        _lstEvents.Items.Clear();
        foreach (KeyLogEntry entry in filtered)
        {
            _lstEvents.Items.Add(entry.ToFormattedLogLine());
        }
        if (_lstEvents.Items.Count > 0)
        {
            _lstEvents.TopIndex = _lstEvents.Items.Count - 1;
        }
        _lstEvents.EndUpdate();

        _rtbTerminalPreview.Clear();
        foreach (KeyLogEntry entry in filtered.TakeLast(25))
        {
            _rtbTerminalPreview.AppendText(entry.ToFormattedLogLine() + Environment.NewLine);
        }

        UpdateStatisticsPanel();
    }

    private void UpdateStatisticsPanel()
    {
        _lblTotalKeysVal.Text = _session.TotalKeyDownCount.ToString();
        _lblTotalEventsVal.Text = _session.TotalEventsCount.ToString();

        _pnlTopKeysContainer.Controls.Clear();
        IReadOnlyList<KeyUsageStatistic> topKeys = _session.GetTopKeys(6);

        if (topKeys.Count == 0)
        {
            Label lblEmpty = new()
            {
                Text = "Henüz tuş verisi yok.\nKayıt aktifken tuşlara basın.",
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(12, 16),
                AutoSize = true
            };
            _pnlTopKeysContainer.Controls.Add(lblEmpty);
            return;
        }

        int y = 8;
        foreach (KeyUsageStatistic stat in topKeys)
        {
            Label lblKey = new()
            {
                Text = $"{stat.KeyName}",
                Font = new Font("Consolas", 8.8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(226, 232, 240),
                Location = new Point(10, y),
                Size = new Size(130, 16)
            };
            _pnlTopKeysContainer.Controls.Add(lblKey);

            Label lblCount = new()
            {
                Text = $"{stat.Count}x (%{stat.Percentage:0.0})",
                Font = new Font("Consolas", 8.5f),
                ForeColor = Color.FromArgb(52, 211, 153),
                Location = new Point(145, y),
                Size = new Size(110, 16),
                TextAlign = ContentAlignment.MiddleRight
            };
            _pnlTopKeysContainer.Controls.Add(lblCount);

            Panel barBg = new()
            {
                Location = new Point(10, y + 17),
                Size = new Size(244, 6),
                BackColor = Color.FromArgb(30, 41, 59)
            };
            int fillWidth = Math.Max(6, (int)Math.Round(244 * (stat.Percentage / 100.0)));
            Panel barFill = new()
            {
                Location = new Point(0, 0),
                Size = new Size(Math.Min(244, fillWidth), 6),
                BackColor = Color.FromArgb(16, 185, 129)
            };
            barBg.Controls.Add(barFill);
            _pnlTopKeysContainer.Controls.Add(barBg);

            y += 28;
        }
    }

    private void ClearAllLogs()
    {
        _session.Clear();
        _lstEvents.Items.Clear();
        _rtbTerminalPreview.Clear();
        _txtSandbox.Clear();
        _lblLastKeyVal.Text = "-";
        UpdateStatisticsPanel();
        _lblFooterBanner.Text = "Durum: Tüm ekran kayıtları ve istatistikler temizlendi.";
    }

    private void ExportLogsWithDialog()
    {
        if (_session.Entries.Count == 0)
        {
            MessageBox.Show(
                "Kaydedilecek herhangi bir klavye olayı bulunamadı. Lütfen önce birkaç tuşa basın.",
                "Kayıt Bulunamadı",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using SaveFileDialog sfd = new()
        {
            Title = "Klavye Olay Kayıtlarını .TXT Dosyasına Kaydet",
            Filter = "Metin Dosyası (*.txt)|*.txt",
            FileName = $"KeyEventLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
        };

        if (sfd.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            _exporter.ExportToTextFile(
                sfd.FileName,
                _session.Entries,
                _session.GetTopKeys(10),
                _session.TotalKeyDownCount);

            _lblFooterBanner.Text = $"Başarılı: {_session.Entries.Count} olay '{Path.GetFileName(sfd.FileName)}' dosyasına kaydedildi.";
            MessageBox.Show(
                $"Klavye olay kayıtları başarıyla kaydedildi:\n{sfd.FileName}",
                "Dışa Aktarım Tamamlandı",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Dosya kaydedilirken bir hata oluştu:\n{ex.Message}",
                "Dosya Yazma Hatası",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    public void CaptureScreenshots(string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        Show();
        Application.DoEvents();

        // Ekran Görüntüsü 1: Kayıt Aktif, örnek klavye olayları ve istatistikler dolu
        SetRecordingState(true);
        _txtSandbox.Text = "Merhaba YMH 219 Nesne Tabanlı Programlama!";
        DateTime baseTime = DateTime.Today.AddHours(14).AddMinutes(22).AddSeconds(10);

        (Keys key, char? ch, Keys mod)[] sampleKeys =
        [
            (Keys.LShiftKey, null, Keys.Shift),
            (Keys.M, 'M', Keys.Shift),
            (Keys.E, 'e', Keys.None),
            (Keys.R, 'r', Keys.None),
            (Keys.H, 'h', Keys.None),
            (Keys.A, 'a', Keys.None),
            (Keys.B, 'b', Keys.None),
            (Keys.A, 'a', Keys.None),
            (Keys.Space, ' ', Keys.None),
            (Keys.Y, 'Y', Keys.Shift),
            (Keys.M, 'M', Keys.Shift),
            (Keys.H, 'H', Keys.Shift),
            (Keys.Space, ' ', Keys.None),
            (Keys.D2, '2', Keys.None),
            (Keys.D1, '1', Keys.None),
            (Keys.D9, '9', Keys.None),
            (Keys.Space, ' ', Keys.None),
            (Keys.Return, null, Keys.None)
        ];

        int msOffset = 0;
        foreach (var (key, ch, mod) in sampleKeys)
        {
            DateTime t = baseTime.AddMilliseconds(msOffset);
            _session.RecordKeyDown(key, (int)key, mod, t);
            if (ch.HasValue)
            {
                _session.RecordKeyPress(ch.Value, t.AddMilliseconds(12));
            }
            _session.RecordKeyUp(key, (int)key, Keys.None, t.AddMilliseconds(65));
            msOffset += 140;
        }

        _lblLastKeyVal.Text = "Enter (Kod: 13)";
        RefreshDisplayedLogs();
        Refresh();
        Application.DoEvents();

        using (Bitmap bmp1 = new(Width, Height))
        {
            DrawToBitmap(bmp1, new Rectangle(0, 0, Width, Height));
            bmp1.Save(Path.Combine(outputDir, "active_logging.png"), System.Drawing.Imaging.ImageFormat.Png);
        }

        // Ekran Görüntüsü 2: Kayıt Durduruldu + Filtreleme Uygulandı + .TXT Kayıt Bildirimi
        SetRecordingState(false);
        _txtKeySearch.Text = "Space";
        RefreshDisplayedLogs();
        _lblFooterBanner.Text = "Başarılı: 50 klavye olayı ve istatistik raporu 'KeyEventLog_20261117.txt' dosyasına kaydedildi.";
        Refresh();
        Application.DoEvents();

        using (Bitmap bmp2 = new(Width, Height))
        {
            DrawToBitmap(bmp2, new Rectangle(0, 0, Width, Height));
            bmp2.Save(Path.Combine(outputDir, "filtered_export.png"), System.Drawing.Imaging.ImageFormat.Png);
        }

        Close();
    }
}
