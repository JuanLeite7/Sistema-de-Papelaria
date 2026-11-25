using System.Drawing;
using System.Globalization;
using Papelaria.Util;

namespace Papelaria.Forms;

public class FrmPrincipal : Form
{
    private readonly string usuarioLogado;
    private readonly Panel conteudo = new() { Dock = DockStyle.Fill, BackColor = Tema.Fundo };

    public FrmPrincipal(string usuario)
    {
        usuarioLogado = usuario;

        Text = "Papelaria - Sistema de Gerenciamento";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1000, 650);
        Font = Tema.Fonte;
        BackColor = Tema.Fundo;

        MontarDashboard();   // Fill primeiro
        MontarSidebar();     // Left depois
    }

    // ---------- MENU LATERAL ----------
    private void MontarSidebar()
    {
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 240, BackColor = Tema.Sidebar };

        var menu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Tema.Sidebar
        };

        var inicio = ItemMenu("Dashboard", () => { });
        inicio.BackColor = Tema.SidebarHover; // marca como tela ativa
        menu.Controls.Add(inicio);

        menu.Controls.Add(Secao("CADASTROS"));
        menu.Controls.Add(ItemMenu("Clientes", () => new FrmClientes().ShowDialog()));
        menu.Controls.Add(ItemMenu("Produtos", () => new FrmProdutos().ShowDialog()));
        menu.Controls.Add(ItemMenu("Fornecedores", () => new FrmFornecedores().ShowDialog()));
        menu.Controls.Add(ItemMenu("Categorias", () => new FrmCategorias().ShowDialog()));
        menu.Controls.Add(ItemMenu("Usuários", () => new FrmUsuarios().ShowDialog()));

        menu.Controls.Add(Secao("ESTOQUE"));
        menu.Controls.Add(ItemMenu("Movimentação", () => new FrmMovimentacaoEstoque().ShowDialog()));
        menu.Controls.Add(ItemMenu("Estoque baixo", () => new FrmEstoqueBaixo().ShowDialog()));

        menu.Controls.Add(Secao("VENDAS"));
        menu.Controls.Add(ItemMenu("Nova venda", () => new FrmNovaVenda().ShowDialog()));
        menu.Controls.Add(ItemMenu("Histórico", () => EmConstrucao("Histórico de Vendas")));
        menu.Controls.Add(ItemMenu("Relatório", () => EmConstrucao("Relatório de Vendas")));

        var logo = new Label
        {
            Text = "Papelaria",
            Dock = DockStyle.Top,
            Height = 70,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(20, 0, 0, 0)
        };

        var btnSair = ItemMenu("Sair", () => Close());
        btnSair.Dock = DockStyle.Bottom;

        // Ordem importa no Dock: Fill primeiro, depois Top e Bottom
        sidebar.Controls.Add(menu);
        sidebar.Controls.Add(logo);
        sidebar.Controls.Add(btnSair);
        Controls.Add(sidebar);
    }

    private static Button ItemMenu(string texto, Action acao)
    {
        var b = new Button
        {
            Text = "   " + texto,
            Width = 240,
            Height = 38,
            Margin = new Padding(0),
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.White,
            BackColor = Tema.Sidebar,
            Font = Tema.Fonte,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = Tema.SidebarHover;
        b.Click += (_, _) => acao();
        return b;
    }

    private static Label Secao(string texto) => new()
    {
        Text = texto,
        Width = 240,
        Height = 30,
        Margin = new Padding(0),
        TextAlign = ContentAlignment.BottomLeft,
        Padding = new Padding(20, 0, 0, 4),
        ForeColor = Color.FromArgb(165, 180, 252),
        Font = new Font("Segoe UI", 8F, FontStyle.Bold)
    };

    // ---------- DASHBOARD ----------
    private void MontarDashboard()
    {
        conteudo.Controls.Add(new Label
        {
            Text = $"Olá, {usuarioLogado}!",
            Font = Tema.FonteTitulo,
            ForeColor = Tema.Texto,
            Location = new Point(30, 25),
            AutoSize = true
        });
        conteudo.Controls.Add(new Label
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
        conteudo.Controls.Add(cards);

        conteudo.Controls.Add(new Label
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
        conteudo.Controls.Add(atalhos);

        Controls.Add(conteudo);
    }

    // Provisório: enquanto a tela não existe. Depois trocar por: new FrmXxx().ShowDialog();
    private static void EmConstrucao(string tela)
        => MessageBox.Show($"Tela '{tela}' ainda em construção.", "Aviso");
}