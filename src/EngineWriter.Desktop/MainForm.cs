using System.Drawing.Drawing2D;
using System.Text;

namespace EngineWriter;

public sealed class MainForm : Form
{
    readonly RichTextBox editor = new();
    readonly Label status = new();
    readonly Label mission = new();
    readonly ListBox radio = new();
    readonly MapPanel map = new();
    readonly ComboBox profile = new();
    int lastWords;
    int signal;

    public MainForm()
    {
        Text = "ENGINE WRITER // FIELD DESK 1944";
        MinimumSize = new Size(1180, 720);
        Size = new Size(1380, 840);
        BackColor = Color.FromArgb(38, 34, 27);
        ForeColor = Color.FromArgb(231, 219, 183);
        Font = new Font("Segoe UI", 10f);

        var top = new Panel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(14, 10, 14, 8), BackColor = Color.FromArgb(53, 47, 36) };
        var title = new Label { Text = "ENGINE WRITER  •  RADIO DESK 1944", AutoSize = true, Font = new Font("Georgia", 16, FontStyle.Bold), Location = new Point(12, 14) };
        profile.Items.AddRange(new object[] { "EN Webnovel", "CN Webnovel", "KR Webnovel" });
        profile.SelectedIndex = 0; profile.DropDownStyle = ComboBoxStyle.DropDownList; profile.Width = 140; profile.Anchor = AnchorStyles.Top | AnchorStyles.Right; profile.Location = new Point(1010, 14);
        var view = MakeButton("2D / RELIEF 3D", (_, _) => { map.Relief = !map.Relief; map.Invalidate(); }); view.Anchor = AnchorStyles.Top | AnchorStyles.Right; view.Location = new Point(1160, 12); view.Width = 175;
        top.Controls.AddRange(new Control[] { title, profile, view });

        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 760, BackColor = Color.FromArgb(26, 24, 20) };
        split.Panel1.Padding = new Padding(14); split.Panel2.Padding = new Padding(10);

        editor.Dock = DockStyle.Fill; editor.BorderStyle = BorderStyle.None; editor.BackColor = Color.FromArgb(246, 237, 210); editor.ForeColor = Color.FromArgb(37, 33, 27); editor.Font = new Font("Georgia", 13.5f); editor.AcceptsTab = true;
        editor.Text = "Capítulo 1 — O sinal na madrugada\n\nÀs 02:13, o rádio que deveria estar morto começou a transmitir.\n\n";
        editor.TextChanged += OnTextChanged;

        var leftBottom = new Panel { Dock = DockStyle.Bottom, Height = 92, Padding = new Padding(10), BackColor = Color.FromArgb(53, 47, 36) };
        mission.Dock = DockStyle.Top; mission.Height = 42; mission.Text = "MISSÃO: escreva 120 palavras que deixem claro o objetivo imediato da cena."; mission.Font = new Font("Segoe UI Semibold", 10f);
        status.Dock = DockStyle.Bottom; status.Height = 28;
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 310, FlowDirection = FlowDirection.LeftToRight };
        buttons.Controls.Add(MakeButton("CRITIC → BUILDER", (_, _) => RunAgents()));
        buttons.Controls.Add(MakeButton("SALVAR", (_, _) => SaveDraft()));
        leftBottom.Controls.Add(mission); leftBottom.Controls.Add(status); leftBottom.Controls.Add(buttons);
        split.Panel1.Controls.Add(editor); split.Panel1.Controls.Add(leftBottom);

        var mapGroup = new GroupBox { Text = "TEATRO DE OPERAÇÕES", Dock = DockStyle.Top, Height = 390, ForeColor = ForeColor, Padding = new Padding(8) };
        map.Dock = DockStyle.Fill; mapGroup.Controls.Add(map);
        var radioGroup = new GroupBox { Text = "RÁDIO / INTERCEPTAÇÕES", Dock = DockStyle.Fill, ForeColor = ForeColor, Padding = new Padding(8) };
        radio.Dock = DockStyle.Fill; radio.BackColor = Color.FromArgb(28, 29, 24); radio.ForeColor = Color.FromArgb(196, 224, 173); radio.BorderStyle = BorderStyle.None; radio.Font = new Font("Consolas", 9.5f);
        radio.Items.Add("[02:13] CHANNEL OPEN — manuscript signal acquired.");
        radio.Items.Add("[02:14] FIELD ORDER — hold the hook, advance the scene.");
        radioGroup.Controls.Add(radio);
        split.Panel2.Controls.Add(radioGroup); split.Panel2.Controls.Add(mapGroup);

        Controls.Add(split); Controls.Add(top);
        KeyPreview = true;
        KeyDown += (_, e) => { if (e.Control && e.KeyCode == Keys.S) { SaveDraft(); e.SuppressKeyPress = true; } if (e.KeyCode == Keys.F6) RunAgents(); if (e.KeyCode == Keys.F7) { map.Relief = !map.Relief; map.Invalidate(); } };
        UpdateStatus();
    }

    Button MakeButton(string text, EventHandler click)
    {
        var b = new Button { Text = text, AutoSize = true, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(96, 82, 51), ForeColor = Color.FromArgb(244, 232, 194) };
        b.FlatAppearance.BorderColor = Color.FromArgb(143, 121, 71); b.Click += click; return b;
    }

    void OnTextChanged(object? sender, EventArgs e)
    {
        int words = CountWords(editor.Text); int gained = Math.Max(0, words - lastWords);
        if (gained > 0) { signal += gained; while (signal >= 80) { signal -= 80; map.Advance(); radio.Items.Insert(0, $"[{DateTime.Now:HH:mm}] +80 WORD SIGNAL — front {map.LastFront + 1} advanced."); } }
        lastWords = words; UpdateStatus(); map.Invalidate();
    }

    void UpdateStatus() => status.Text = $"WORDS {CountWords(editor.Text):N0}   •   SIGNAL {signal}/80   •   PROFILE {profile.Text}   •   AUTOSAVE local";

    void RunAgents()
    {
        var text = editor.Text.Trim(); var notes = new List<string>();
        if (text.Length < 500) notes.Add("Amplie a cena antes de explicar o mundo; procure ação, desejo ou risco concreto.");
        var paragraphs = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (paragraphs.Any(p => CountWords(p) > 110)) notes.Add("Há um parágrafo muito longo; quebrá-lo pode melhorar o ritmo para leitura mobile.");
        if (!text.Contains('“') && !text.Contains('"')) notes.Add("Considere uma fala curta para introduzir voz, conflito ou informação sob pressão.");
        if (!text.EndsWith("?") && !text.EndsWith("!") && text.Length > 250) notes.Add("O encerramento pode abrir uma pergunta ou risco mais forte para sustentar o próximo clique.");
        if (notes.Count == 0) notes.Add("A cena está legível. Preserve o impulso e avance antes de polir em excesso.");
        var top = notes.Take(3).ToArray();
        foreach (var n in top.Reverse()) radio.Items.Insert(0, $"[CRITIC] {n}");
        mission.Text = "MISSÃO: " + BuildMission(top[0]);
        radio.Items.Insert(0, "[BUILDER] Nova micro-missão preparada; crítica condensada para não interromper o fluxo.");
    }

    static string BuildMission(string note) => note.Contains("fala", StringComparison.OrdinalIgnoreCase) ? "adicione 1–3 falas que revelem conflito sem explicar todo o backstory." : note.Contains("encerramento", StringComparison.OrdinalIgnoreCase) ? "reescreva as duas últimas frases para criar uma pergunta, ameaça ou promessa." : note.Contains("parágrafo", StringComparison.OrdinalIgnoreCase) ? "quebre o maior bloco em unidades mais rápidas e visuais." : "escreva mais 120 palavras focando objetivo, obstáculo e consequência imediata.";

    void SaveDraft()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Engine Writer"); Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "chapter-current.txt"); File.WriteAllText(path, editor.Text, Encoding.UTF8); radio.Items.Insert(0, $"[{DateTime.Now:HH:mm}] SAVED — {path}");
    }

    static int CountWords(string text) => text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
}

public sealed class MapPanel : Panel
{
    readonly float[] fronts = { .42f, .58f, .36f, .49f };
    readonly Random rng = new();
    public bool Relief { get; set; }
    public int LastFront { get; private set; }
    public MapPanel() { DoubleBuffered = true; BackColor = Color.FromArgb(69, 67, 52); }
    public void Advance() { LastFront = rng.Next(fronts.Length); fronts[LastFront] = Math.Min(.95f, fronts[LastFront] + .06f + (float)rng.NextDouble() * .04f); }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var r = ClientRectangle; if (r.Width < 20 || r.Height < 20) return;
        using var paper = new SolidBrush(Color.FromArgb(191, 178, 132)); e.Graphics.FillRectangle(paper, r);
        if (Relief) DrawRelief(e.Graphics, r); else Draw2D(e.Graphics, r);
        using var pen = new Pen(Color.FromArgb(76, 67, 47), 2); e.Graphics.DrawRectangle(pen, 5, 5, r.Width - 11, r.Height - 11);
    }

    void Draw2D(Graphics g, Rectangle r)
    {
        using var grid = new Pen(Color.FromArgb(55, 92, 82, 58), 1);
        for (int x = 20; x < r.Width; x += 32) g.DrawLine(grid, x, 0, x, r.Height);
        for (int y = 20; y < r.Height; y += 32) g.DrawLine(grid, 0, y, r.Width, y);
        for (int i = 0; i < 4; i++) { int y = 55 + i * 72; int end = 55 + (int)((r.Width - 110) * fronts[i]); using var p = new Pen(Color.FromArgb(92, 61, 42), 12) { StartCap = LineCap.Round, EndCap = LineCap.Round }; g.DrawLine(p, 55, y, end, y); using var b = new SolidBrush(Color.FromArgb(35, 48, 40)); g.FillEllipse(b, end - 9, y - 9, 18, 18); g.DrawString($"FRONT {i + 1}  {fronts[i] * 100:0}%", Font, Brushes.Black, 58, y + 13); }
    }

    void DrawRelief(Graphics g, Rectangle r)
    {
        var cx = r.Width / 2f; var baseY = r.Height * .75f;
        for (int layer = 0; layer < 7; layer++)
        {
            float t = layer / 6f; var pts = new List<PointF>();
            for (int x = 25; x <= r.Width - 25; x += 14) { double wave = Math.Sin(x * .035 + layer * .8) + .45 * Math.Sin(x * .082 - layer); float y = baseY - layer * 28 - (float)wave * (18 + layer * 3); pts.Add(new PointF(x, y)); }
            pts.Add(new PointF(r.Width - 25, baseY + 30)); pts.Add(new PointF(25, baseY + 30));
            using var b = new SolidBrush(Color.FromArgb(95 + layer * 13, 96 + layer * 10, 67 + layer * 6)); g.FillPolygon(b, pts.ToArray());
            using var p = new Pen(Color.FromArgb(82, 65, 44), 1.5f); g.DrawLines(p, pts.Take(pts.Count - 2).ToArray());
        }
        for (int i = 0; i < 4; i++) { float x = 70 + (r.Width - 140) * fronts[i]; float y = 70 + i * 48; using var b = new SolidBrush(Color.FromArgb(47, 56, 44)); g.FillEllipse(b, x - 8, y - 8, 16, 16); g.DrawString($"F{i + 1}", Font, Brushes.Black, x + 10, y - 10); }
        g.DrawString("RELIEF MODE // SIGNAL TOPOGRAPHY", new Font(Font, FontStyle.Bold), Brushes.Black, 18, 18);
    }
}
