using Papelaria.Model;
using Papelaria.Util;

namespace Papelaria.Forms;

public class FrmHistoricoVendas : Form
{
    // ---------- CONTROLES ----------
    private readonly TextBox txtPesquisa = new() { PlaceholderText = "Pesquisar por cliente..." };
    private readonly CheckBox chkTodoPeriodo = new() { Text = "Todo o período", Checked = true, AutoSize = true };
    private readonly DateTimePicker dtInicio = new() { Format = DateTimePickerFormat.Short, Enabled = false };
    private readonly DateTimePicker dtFim = new() { Format = DateTimePickerFormat.Short, Enabled = false };
    private readonly Label lblResumo = new()
    {
        AutoSize = false,
        Size = new Size(290, 28),
        TextAlign = ContentAlignment.MiddleRight,
        Font = Tema.FonteNegrito,
        ForeColor = Tema.Primaria
    };
    private readonly DataGridView grid = new();

    private readonly Label lblItens = new() { AutoSize = true, Font = Tema.FonteNegrito, ForeColor = Tema.Texto };
    private readonly DataGridView gridItens = new();

    private readonly Button btnFechar = Tema.Botao("Fechar", Tema.Neutro, 860, 668, 120, 42);

    public FrmHistoricoVendas()
    {
        Text = "Histórico de Vendas";
        Font = Tema.Fonte;
        BackColor = Tema.Fundo;
        ClientSize = new Size(1000, 720);
        MinimumSize = new Size(1016, 760);
        StartPosition = FormStartPosition.CenterParent;

        MontarTela();
        CarregarGrid();
    }

    // ---------- MONTAGEM DA TELA ----------
    private void MontarTela()
    {
        Controls.Add(Tema.Cabecalho("Histórico de Vendas", "Consulte as vendas já realizadas"));

        // Cartão das vendas (filtros + grid)
        var cardVendas = Tema.Cartao(20, 92, 960, 300);
        cardVendas.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        var hoje = DateTime.Today;
        dtInicio.Value = new DateTime(hoje.Year, hoje.Month, 1); // início do mês
        dtFim.Value = hoje;

        Tema.Campo(txtPesquisa, 15, 15, 210);
        chkTodoPeriodo.Font = Tema.Fonte;
        chkTodoPeriodo.Location = new Point(240, 17);
        Tema.Posicionar(dtInicio, 385, 15, 110);
        Tema.Posicionar(dtFim, 537, 15, 110);
        lblResumo.Location = new Point(655, 14);
        lblResumo.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        Tema.EstilizarGrid(grid);
        grid.Location = new Point(15, 55);
        grid.Size = new Size(930, 230);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        cardVendas.Controls.Add(Tema.Rotulo("até", 506, 19));
        cardVendas.Controls.AddRange(new Control[] { txtPesquisa, chkTodoPeriodo, dtInicio, dtFim, lblResumo, grid });
        Controls.Add(cardVendas);

        // Cartão dos itens da venda selecionada
        var cardItens = Tema.Cartao(20, 405, 960, 250);
        cardItens.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        lblItens.Location = new Point(15, 15);
        Tema.EstilizarGrid(gridItens);
        gridItens.Location = new Point(15, 45);
        gridItens.Size = new Size(930, 190);
        gridItens.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        cardItens.Controls.AddRange(new Control[] { lblItens, gridItens });
        Controls.Add(cardItens);

        btnFechar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        Controls.Add(btnFechar);

        // Eventos
        txtPesquisa.TextChanged += (_, _) => CarregarGrid();
        chkTodoPeriodo.CheckedChanged += (_, _) =>
        {
            dtInicio.Enabled = dtFim.Enabled = !chkTodoPeriodo.Checked;
            CarregarGrid();
        };
        dtInicio.ValueChanged += (_, _) => CarregarGrid();
        dtFim.ValueChanged += (_, _) => CarregarGrid();
        grid.SelectionChanged += Grid_SelectionChanged;
        btnFechar.Click += (_, _) => Close();
    }

    // ---------- GRID DE VENDAS ----------
    private void CarregarGrid()
    {
        try
        {
            DateTime? inicio = chkTodoPeriodo.Checked ? null : dtInicio.Value.Date;
            DateTime? fim = chkTodoPeriodo.Checked ? null : dtFim.Value.Date;
            var lista = ConsultarVendas(txtPesquisa.Text.Trim(), inicio, fim);

            grid.AutoGenerateColumns = true;
            grid.DataSource = null;
            grid.DataSource = lista;

            // Ajustes só se a coluna existir
            if (grid.Columns["Id"] is { } colId)
            {
                colId.HeaderText = "Nº";
                colId.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colId.Width = 60;
            }
            if (grid.Columns["Data"] is { } colData) colData.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            if (grid.Columns["ClienteId"] is { } colCliId) colCliId.Visible = false;
            if (grid.Columns["ClienteNome"] is { } colCli) colCli.HeaderText = "Cliente";
            if (grid.Columns["FormaPagamento"] is { } colPag) colPag.HeaderText = "Pagamento";
            if (grid.Columns["Itens"] is { } colItens) colItens.Visible = false; // os itens aparecem no grid de baixo

            foreach (var nome in new[] { "Subtotal", "Desconto", "Total" })
            {
                if (grid.Columns[nome] is { } col)
                {
                    col.DefaultCellStyle.Format = "C2";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            grid.ClearSelection(); // não abre os itens de uma venda sozinho

            lblResumo.Text = lista.Count == 0
                ? "Nenhuma venda encontrada"
                : $"{lista.Count} venda(s)  |  Total: {lista.Sum(v => v.Total):C2}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao carregar vendas: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        var venda = grid.SelectedRows.Count > 0 ? grid.SelectedRows[0].DataBoundItem as Venda : null;
        MostrarItens(venda);
    }

    // ---------- GRID DE ITENS ----------
    private void MostrarItens(Venda? venda)
    {
        gridItens.AutoGenerateColumns = true;
        gridItens.DataSource = null;

        if (venda == null)
        {
            lblItens.Text = "Selecione uma venda para ver os itens";
            return;
        }

        gridItens.DataSource = venda.Itens;
        lblItens.Text = $"Itens da venda nº {venda.Id} - {venda.ClienteNome}";

        // Ajustes só se a coluna existir
        if (gridItens.Columns["ProdutoId"] is { } colId) colId.Visible = false;
        if (gridItens.Columns["ProdutoNome"] is { } colProd) colProd.HeaderText = "Produto";
        if (gridItens.Columns["Quantidade"] is { } colQtd)
        {
            colQtd.HeaderText = "Qtd";
            colQtd.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colQtd.Width = 80;
        }
        if (gridItens.Columns["PrecoUnitario"] is { } colPreco)
        {
            colPreco.HeaderText = "Preço unit.";
            colPreco.DefaultCellStyle.Format = "C2";
            colPreco.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        if (gridItens.Columns["Subtotal"] is { } colSub)
        {
            colSub.DefaultCellStyle.Format = "C2";
            colSub.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        gridItens.ClearSelection();
    }

    // =====================================================================
    // ACESSO A DADOS PROVISÓRIO (em memória)
    // Quando o DAO ficar pronto, trocar este método:
    //   ConsultarVendas(cliente, inicio, fim) -> new VendaDAO().Listar(cliente, inicio, fim)
    //     (no banco: WHERE cliente LIKE ... AND data BETWEEN ... ORDER BY data DESC)
    //   Os itens de cada venda (venda.Itens) devem vir do JOIN com ItensVenda.
    // =====================================================================
    private static List<Venda> ConsultarVendas(string cliente, DateTime? inicio, DateTime? fim)
        => FrmNovaVenda.ListarVendas()
            .Where(v => (string.IsNullOrEmpty(cliente)
                            || v.ClienteNome.Contains(cliente, StringComparison.OrdinalIgnoreCase))
                     && (inicio == null || v.Data.Date >= inicio.Value)
                     && (fim == null || v.Data.Date <= fim.Value))
            .ToList();
}