using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Papelaria.Util;

namespace Papelaria.Forms;

public class FrmPrincipal : Form
{
    private enum Icone { Venda, Estoque, Cadastro }

    private readonly string usuarioLogado;
    private readonly Panel conteudo = new() { Dock = DockStyle.Fill, BackColor = Tema.Fundo };
    private readonly List<Button> abas = new();

    public FrmPrincipal(string usuario)
    {
        usuarioLogado = usuario;

        Text = "Papelaria - Sistema de Gerenciamento";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1000, 650);
        Font = Tema.Fonte;
        BackColor = Tema.Fundo;

        Controls.Add(conteudo);   // Fill primeiro
        MontarMenuTopo();         // Top depois

        MostrarDashboard();       // tela inicial (clique no nome da loja volta pra cá)
    }

    // ---------- MENU NO TOPO ----------
    private void MontarMenuTopo()
    {
        var topo = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Tema.Sidebar };

        var menu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Tema.Sidebar,
            Padding = new Padding(10, 0, 0, 0)
        };

        // Ordem: Venda, Estoque, Cadastro
        menu.Controls.Add(Aba("Venda", Icone.Venda, MostrarVenda));
        menu.Controls.Add(Aba("Estoque", Icone.Estoque, MostrarEstoque));
        menu.Controls.Add(Aba("Cadastro", Icone.Cadastro, MostrarCadastro));

        var logo = new Label
        {
            Text = "Papelaria Casa Bella",
            Dock = DockStyle.Left,
            Width = 290,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(20, 0, 0, 0),
            Cursor = Cursors.Hand
        };
        logo.Click += (_, _) => MostrarDashboard();

        var btnSair = new Button
        {
            Text = "Sair",
            Dock = DockStyle.Right,
            Width = 100,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Tema.Sidebar,
            Font = Tema.Fonte,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        btnSair.FlatAppearance.BorderSize = 0;
        btnSair.FlatAppearance.MouseOverBackColor = Tema.SidebarHover;
        btnSair.Click += (_, _) => Close();

        // Ordem importa no Dock: Fill primeiro, depois Left e Right
        topo.Controls.Add(menu);
        topo.Controls.Add(logo);
        topo.Controls.Add(btnSair);
        Controls.Add(topo);
    }

    private Button Aba(string texto, Icone icone, Action abrir)
    {
        var b = new Button
        {
            Text = texto,
            Image = CriarIcone(icone, Color.White, 26),
            TextImageRelation = TextImageRelation.ImageBeforeText,
            ImageAlign = ContentAlignment.MiddleCenter,
            TextAlign = ContentAlignment.MiddleCenter,
            Width = 160,
            Height = 64,
            Margin = new Padding(0),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Tema.Sidebar,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = Tema.SidebarHover;
        b.Click += (_, _) =>
        {
            MarcarAba(b);
            abrir();
        };
        abas.Add(b);
        return b;
    }

    private void MarcarAba(Button? ativa)
    {
        foreach (var a in abas)
            a.BackColor = a == ativa ? Tema.SidebarHover : Tema.Sidebar;
    }

    // ---------- PÁGINAS ----------
    private void Navegar(Panel pagina)
    {
        conteudo.SuspendLayout();
        foreach (Control c in conteudo.Controls) c.Dispose();
        conteudo.Controls.Clear();
        pagina.Dock = DockStyle.Fill;
        conteudo.Controls.Add(pagina);
        conteudo.ResumeLayout();
    }

    private void MostrarVenda() => Navegar(MontarPagina(
        Icone.Venda, "Venda", "Registre vendas e acompanhe o histórico.",
        ("Nova venda", Tema.Sucesso, () => new FrmNovaVenda().ShowDialog()),
        ("Histórico", Tema.Primaria, () => new FrmHistoricoVendas().ShowDialog()),
        ("Relatório", Tema.Primaria, () => EmConstrucao("Relatório de Vendas"))));

    private void MostrarEstoque() => Navegar(MontarPagina(
        Icone.Estoque, "Estoque", "Controle entradas, saídas e produtos acabando.",
        ("Movimentação", Tema.Primaria, () => new FrmMovimentacaoEstoque().ShowDialog()),
        ("Estoque baixo", Tema.Perigo, () => new FrmEstoqueBaixo().ShowDialog())));

    private void MostrarCadastro() => Navegar(MontarPagina(
        Icone.Cadastro, "Cadastro", "Gerencie clientes, produtos e demais registros.",
        ("Clientes", Tema.Primaria, () => new FrmClientes().ShowDialog()),
        ("Produtos", Tema.Primaria, () => new FrmProdutos().ShowDialog()),
        ("Fornecedores", Tema.Primaria, () => new FrmFornecedores().ShowDialog()),
        ("Categorias", Tema.Primaria, () => new FrmCategorias().ShowDialog()),
        ("Usuários", Tema.Primaria, () => new FrmUsuarios().ShowDialog())));

    private static Panel MontarPagina(Icone icone, string titulo, string subtitulo,
        params (string Texto, Color Cor, Action Acao)[] itens)
    {
        var pagina = new Panel { BackColor = Tema.Fundo };

        // Título com ícone de contorno preto
        pagina.Controls.Add(new PictureBox
        {
            Image = CriarIcone(icone, Color.Black, 44),
            SizeMode = PictureBoxSizeMode.AutoSize,
            Location = new Point(30, 22),
            BackColor = Color.Transparent
        });
        pagina.Controls.Add(new Label
        {
            Text = titulo,
            Font = Tema.FonteTitulo,
            ForeColor = Tema.Texto,
            Location = new Point(86, 20),
            AutoSize = true
        });
        pagina.Controls.Add(new Label
        {
            Text = subtitulo,
            ForeColor = Tema.TextoSuave,
            Location = new Point(88, 62),
            AutoSize = true
        });

        var cards = new FlowLayoutPanel
        {
            Location = new Point(30, 110),
            Size = new Size(1040, 400),
            BackColor = Color.Transparent
        };
        foreach (var (texto, cor, acao) in itens)
        {
            var b = Tema.Botao(texto, cor, 0, 0, 220, 90);
            b.Margin = new Padding(0, 0, 20, 20);
            b.Click += (_, _) => acao();
            cards.Controls.Add(b);
        }
        pagina.Controls.Add(cards);
        return pagina;
    }

    // ---------- DASHBOARD (tela inicial) ----------
    private void MostrarDashboard()
    {
        MarcarAba(null);

        var pagina = new Panel { BackColor = Tema.Fundo };

        pagina.Controls.Add(new Label
        {
            Text = $"Olá, {usuarioLogado}!",
            Font = Tema.FonteTitulo,
            ForeColor = Tema.Texto,
            Location = new Point(30, 25),
            AutoSize = true
        });
        pagina.Controls.Add(new Label
        {
            Text = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new CultureInfo("pt-BR")),
            ForeColor = Tema.TextoSuave,
            Location = new Point(32, 65),
            AutoSize = true
        });

        // TODO: trocar valores fixos por consultas do DAO
        var cards = new FlowLayoutPanel { Location = new Point(30, 110), Size = new Size(1040, 120) };
        cards.Controls.Add(Tema.CartaoIndicador("Produtos cadastrados", "0", Tema.Primaria));
        cards.Controls.Add(Tema.CartaoIndicador("Estoque baixo", "0", Tema.Perigo));
        cards.Controls.Add(Tema.CartaoIndicador("Vendas hoje", "0", Tema.Sucesso));
        cards.Controls.Add(Tema.CartaoIndicador("Faturamento hoje", "R$ 0,00", Tema.Alerta));
        pagina.Controls.Add(cards);

        pagina.Controls.Add(new Label
        {
            Text = "Atalhos rápidos",
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Tema.Texto,
            Location = new Point(30, 245),
            AutoSize = true
        });

        var atalhos = new FlowLayoutPanel { Location = new Point(30, 285), Size = new Size(1040, 60) };

        var btnVenda = Tema.Botao("Nova venda", Tema.Sucesso, 0, 0, 180, 50);
        btnVenda.Margin = new Padding(0, 0, 15, 0);
        btnVenda.Click += (_, _) => new FrmNovaVenda().ShowDialog();

        var btnProduto = Tema.Botao("Produtos", Tema.Primaria, 0, 0, 180, 50);
        btnProduto.Margin = new Padding(0, 0, 15, 0);
        btnProduto.Click += (_, _) => new FrmProdutos().ShowDialog();

        var btnCliente = Tema.Botao("Clientes", Tema.Primaria, 0, 0, 180, 50);
        btnCliente.Margin = new Padding(0, 0, 15, 0);
        btnCliente.Click += (_, _) => new FrmClientes().ShowDialog();

        atalhos.Controls.AddRange(new Control[] { btnVenda, btnProduto, btnCliente });
        pagina.Controls.Add(atalhos);

        Navegar(pagina);
    }

    // ---------- ÍCONES DE CONTORNO (desenhados em código, sem arquivos externos) ----------
    private static Bitmap CriarIcone(Icone tipo, Color cor, int tamanho)
    {
        var bmp = new Bitmap(tamanho, tamanho);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);
        g.ScaleTransform(tamanho / 24f, tamanho / 24f); // desenha numa grade de 24x24

        using var pen = new Pen(cor, 1.6f)
        {
            LineJoin = LineJoin.Round,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        static PointF[] P(params float[] v)
        {
            var pts = new PointF[v.Length / 2];
            for (int i = 0; i < pts.Length; i++) pts[i] = new PointF(v[i * 2], v[i * 2 + 1]);
            return pts;
        }

        switch (tipo)
        {
            case Icone.Cadastro: // folha de papel com canto dobrado e linhas de texto
                g.DrawPolygon(pen, P(6, 3, 14, 3, 19, 8, 19, 21, 6, 21));
                g.DrawLines(pen, P(14, 3, 14, 8, 19, 8));
                g.DrawLine(pen, 9, 13, 16, 13);
                g.DrawLine(pen, 9, 17, 16, 17);
                break;

            case Icone.Estoque: // caixa de papelão
                g.DrawPolygon(pen, P(12, 3, 21, 7.5f, 21, 16.5f, 12, 21, 3, 16.5f, 3, 7.5f));
                g.DrawLines(pen, P(3, 7.5f, 12, 12, 21, 7.5f));
                g.DrawLine(pen, 12, 12, 12, 21);
                g.DrawLine(pen, 7.5f, 5.25f, 16.5f, 9.75f); // fita da caixa
                break;

            case Icone.Venda: // caixa registradora
                g.DrawRectangle(pen, 8, 3, 8, 5);                  // visor
                g.DrawLine(pen, 12, 8, 12, 10);                    // haste
                g.DrawPolygon(pen, P(6, 10, 18, 10, 21, 17, 3, 17)); // teclado
                g.DrawRectangle(pen, 3, 17, 18, 4);                // gaveta
                g.DrawEllipse(pen, 7.6f, 12.8f, 0.8f, 0.8f);       // teclas
                g.DrawEllipse(pen, 11.6f, 12.8f, 0.8f, 0.8f);
                g.DrawEllipse(pen, 15.6f, 12.8f, 0.8f, 0.8f);
                break;
        }
        return bmp;
    }

    // Provisório: enquanto a tela não existe. Depois trocar por: new FrmXxx().ShowDialog();
    private static void EmConstrucao(string tela)
        => MessageBox.Show($"Tela '{tela}' ainda em construção.", "Aviso");
}