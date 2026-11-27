namespace Papelaria.Util;

/// <summary>Paleta, fontes e estilos centralizados. Mudou aqui, muda no sistema inteiro.</summary>
public static class Tema
{
    // ---------- CORES ----------
    public static readonly Color Primaria = Color.FromArgb(79, 70, 229);
    public static readonly Color Sucesso = Color.FromArgb(16, 185, 129);
    public static readonly Color Perigo = Color.FromArgb(239, 68, 68);
    public static readonly Color Alerta = Color.FromArgb(245, 158, 11);
    public static readonly Color Neutro = Color.FromArgb(107, 114, 128);
    public static readonly Color Fundo = Color.FromArgb(243, 244, 246);
    public static readonly Color Borda = Color.FromArgb(229, 231, 235);
    public static readonly Color Texto = Color.FromArgb(31, 41, 55);
    public static readonly Color TextoSuave = Color.FromArgb(107, 114, 128);
    public static readonly Color Sidebar = Color.FromArgb(30, 27, 75);
    public static readonly Color SidebarHover = Color.FromArgb(55, 48, 163);

    // ---------- FONTES ----------
    public static readonly Font Fonte = new("Segoe UI", 10F);
    public static readonly Font FonteNegrito = new("Segoe UI", 10F, FontStyle.Bold);
    public static readonly Font FontePequena = new("Segoe UI", 9F);
    public static readonly Font FonteTitulo = new("Segoe UI", 18F, FontStyle.Bold);

    private static Color Escurecer(Color c, double f)
        => Color.FromArgb((int)(c.R * (1 - f)), (int)(c.G * (1 - f)), (int)(c.B * (1 - f)));

    // ---------- COMPONENTES ----------

    /// <summary>Faixa colorida no topo da tela com título e subtítulo.</summary>
    public static Panel Cabecalho(string titulo, string subtitulo)
    {
        var p = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Primaria };
        p.Controls.Add(new Label
        {
            Text = titulo,
            Font = FonteTitulo,
            ForeColor = Color.White,
            Location = new Point(24, 10),
            AutoSize = true
        });
        p.Controls.Add(new Label
        {
            Text = subtitulo,
            Font = Fonte,
            ForeColor = Color.FromArgb(199, 210, 254),
            Location = new Point(26, 44),
            AutoSize = true
        });
        return p;
    }

    /// <summary>Posiciona e aplica a fonte em ComboBox, NumericUpDown, etc.</summary>
    public static T Posicionar<T>(T controle, int x, int y, int largura) where T : Control
    {
        controle.Location = new Point(x, y);
        controle.Width = largura;
        controle.Font = Fonte;
        return controle;
    }

    /// <summary>Painel branco com borda fina (visual de "cartão").</summary>
    public static Panel Cartao(int x, int y, int largura, int altura)
    {
        var p = new Panel { Location = new Point(x, y), Size = new Size(largura, altura), BackColor = Color.White };
        p.Paint += (_, e) =>
        {
            using var pen = new Pen(Borda);
            e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        };
        p.Resize += (_, _) => p.Invalidate();
        return p;
    }

    /// <summary>Cartão pequeno de indicador do dashboard (faixa colorida + título + valor).</summary>
    public static Panel CartaoIndicador(string titulo, string valor, Color cor)
    {
        var p = Cartao(0, 0, 230, 100);
        p.Margin = new Padding(0, 0, 20, 20);
        p.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 6, BackColor = cor });
        p.Controls.Add(new Label
        {
            Text = titulo,
            ForeColor = TextoSuave,
            Font = Fonte,
            Location = new Point(20, 14),
            AutoSize = true
        });
        p.Controls.Add(new Label
        {
            Name = "valor",
            Text = valor,
            ForeColor = Texto,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            Location = new Point(18, 42),
            AutoSize = true
        });
        return p;
    }

    /// <summary>Botão chapado com cor, hover e estilo desabilitado.</summary>
    public static Button Botao(string texto, Color cor, int x, int y, int largura = 110, int altura = 38)
    {
        var b = new Button
        {
            Text = texto,
            Location = new Point(x, y),
            Size = new Size(largura, altura),
            FlatStyle = FlatStyle.Flat,
            BackColor = cor,
            ForeColor = Color.White,
            Font = FonteNegrito,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = Escurecer(cor, 0.12);
        b.FlatAppearance.MouseDownBackColor = Escurecer(cor, 0.25);
        b.EnabledChanged += (_, _) => b.BackColor = b.Enabled ? cor : Color.FromArgb(209, 213, 219);
        return b;
    }

    /// <summary>Rótulo pequeno e cinza para ficar em cima dos campos.</summary>
    public static Label Rotulo(string texto, int x, int y) => new()
    {
        Text = texto,
        Location = new Point(x, y),
        AutoSize = true,
        ForeColor = TextoSuave,
        Font = FontePequena
    };

    /// <summary>Posiciona e estiliza um TextBox ou MaskedTextBox.</summary>
    public static T Campo<T>(T campo, int x, int y, int largura) where T : TextBoxBase
    {
        campo.Location = new Point(x, y);
        campo.Width = largura;
        campo.Font = Fonte;
        campo.BorderStyle = BorderStyle.FixedSingle;
        return campo;
    }

    /// <summary>Visual moderno para qualquer DataGridView.</summary>
    public static void EstilizarGrid(DataGridView g)
    {
        g.EnableHeadersVisualStyles = false;
        g.BorderStyle = BorderStyle.None;
        g.BackgroundColor = Color.White;
        g.GridColor = Borda;
        g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        g.ColumnHeadersHeight = 38;
        g.ColumnHeadersDefaultCellStyle.BackColor = Primaria;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Primaria;
        g.ColumnHeadersDefaultCellStyle.Font = FonteNegrito;
        g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

        g.RowTemplate.Height = 34;
        g.DefaultCellStyle.Font = Fonte;
        g.DefaultCellStyle.ForeColor = Texto;
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 231, 255);
        g.DefaultCellStyle.SelectionForeColor = Texto;
        g.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);

        g.RowHeadersVisible = false;
        g.ReadOnly = true;
        g.AllowUserToAddRows = false;
        g.AllowUserToDeleteRows = false;
        g.AllowUserToResizeRows = false;
        g.MultiSelect = false;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
    }
}