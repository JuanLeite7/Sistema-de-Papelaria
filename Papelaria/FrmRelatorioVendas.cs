using System.Drawing;
using Papelaria.Model;
using Papelaria.Util;

namespace Papelaria.Forms;

public class FrmRelatorioVendas : Form
{
    // ---------- CONTROLES ----------
    private readonly CheckBox chkTodoPeriodo = new() { Text = "Todo o período", Checked = true, AutoSize = true };
    private readonly DateTimePicker dtInicio = new() { Format = DateTimePickerFormat.Short, Enabled = false };
    private readonly DateTimePicker dtFim = new() { Format = DateTimePickerFormat.Short, Enabled = false };
    private readonly ComboBox cmbPagamento = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    // Cartões de indicadores (o Label "valor" de cada um é atualizado em CarregarRelatorio)
    private readonly Panel cardQtd = Tema.CartaoIndicador("Vendas realizadas", "0", Tema.Primaria);
    private readonly Panel cardFaturamento = Tema.CartaoIndicador("Faturamento", "R$ 0,00", Tema.Sucesso);
    private readonly Panel cardTicket = Tema.CartaoIndicador("Ticket médio", "R$ 0,00", Tema.Alerta);
    private readonly Panel cardDescontos = Tema.CartaoIndicador("Descontos concedidos", "R$ 0,00", Tema.Perigo);

    private readonly DataGridView gridPagamento = new();
    private readonly DataGridView gridProdutos = new();

    private readonly Button btnFechar = Tema.Botao("Fechar", Tema.Neutro, 860, 668, 120, 42);

    public FrmRelatorioVendas()
    {
        Text = "Relatório de Vendas";
        Font = Tema.Fonte;
        BackColor = Tema.Fundo;
        ClientSize = new Size(1000, 720);
        MinimumSize = new Size(1016, 760);
        StartPosition = FormStartPosition.CenterParent;

        MontarTela();
        CarregarRelatorio();
    }

    // ---------- MONTAGEM DA TELA ----------
    private void MontarTela()
    {
        Controls.Add(Tema.Cabecalho("Relatório de Vendas", "Resumo do faturamento por período"));

        // Cartão dos filtros
        var cardFiltros = Tema.Cartao(20, 86, 960, 72);
        cardFiltros.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        var hoje = DateTime.Today;
        dtInicio.Value = new DateTime(hoje.Year, hoje.Month, 1); // início do mês
        dtFim.Value = hoje;

        cmbPagamento.Items.Add("Todas as formas");
        cmbPagamento.Items.AddRange(new object[] { "Dinheiro", "Cartão de débito", "Cartão de crédito", "Pix" });
        cmbPagamento.SelectedIndex = 0;

        cardFiltros.Controls.Add(Tema.Rotulo("Período", 15, 8));
        cardFiltros.Controls.Add(Tema.Rotulo("até", 307, 40));
        cardFiltros.Controls.Add(Tema.Rotulo("Forma de pagamento", 660, 8));

        chkTodoPeriodo.Font = Tema.Fonte;
        chkTodoPeriodo.Location = new Point(15, 34);
        Tema.Posicionar(dtInicio, 170, 32, 120);
        Tema.Posicionar(dtFim, 340, 32, 120);
        Tema.Posicionar(cmbPagamento, 660, 32, 285);

        cardFiltros.Controls.AddRange(new Control[] { chkTodoPeriodo, dtInicio, dtFim, cmbPagamento });
        Controls.Add(cardFiltros);

        // Cartões de indicadores
        Control[] indicadores = { cardQtd, cardFaturamento, cardTicket, cardDescontos };
        for (int i = 0; i < indicadores.Length; i++)
        {
            indicadores[i].Size = new Size(225, 100);
            indicadores[i].Location = new Point(20 + i * 245, 170);
            indicadores[i].Anchor = AnchorStyles.Top | AnchorStyles.Left;
            Controls.Add(indicadores[i]);
        }

        // Cartão: vendas por forma de pagamento
        var cardPag = Tema.Cartao(20, 282, 400, 370);
        cardPag.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;

        var lblPag = new Label
        {
            Text = "Vendas por forma de pagamento",
            Font = Tema.FonteNegrito,
            ForeColor = Tema.Texto,
            Location = new Point(15, 15),
            AutoSize = true
        };
        Tema.EstilizarGrid(gridPagamento);
        gridPagamento.Location = new Point(15, 50);
        gridPagamento.Size = new Size(370, 305);
        gridPagamento.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        cardPag.Controls.Add(lblPag);
        cardPag.Controls.Add(gridPagamento);
        Controls.Add(cardPag);

        // Cartão: produtos mais vendidos
        var cardProd = Tema.Cartao(435, 282, 545, 370);
        cardProd.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        var lblProd = new Label
        {
            Text = "Produtos mais vendidos",
            Font = Tema.FonteNegrito,
            ForeColor = Tema.Texto,
            Location = new Point(15, 15),
            AutoSize = true
        };
        Tema.EstilizarGrid(gridProdutos);
        gridProdutos.Location = new Point(15, 50);
        gridProdutos.Size = new Size(515, 305);
        gridProdutos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        cardProd.Controls.Add(lblProd);
        cardProd.Controls.Add(gridProdutos);
        Controls.Add(cardProd);

        btnFechar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        Controls.Add(btnFechar);

        // Eventos
        chkTodoPeriodo.CheckedChanged += (_, _) =>
        {
            dtInicio.Enabled = dtFim.Enabled = !chkTodoPeriodo.Checked;
            CarregarRelatorio();
        };
        dtInicio.ValueChanged += (_, _) => CarregarRelatorio();
        dtFim.ValueChanged += (_, _) => CarregarRelatorio();
        cmbPagamento.SelectedIndexChanged += (_, _) => CarregarRelatorio();
        btnFechar.Click += (_, _) => Close();
    }

    // ---------- RELATÓRIO ----------
    private void CarregarRelatorio()
    {
        try
        {
            DateTime? inicio = chkTodoPeriodo.Checked ? null : dtInicio.Value.Date;
            DateTime? fim = chkTodoPeriodo.Checked ? null : dtFim.Value.Date;
            string? pagamento = cmbPagamento.SelectedIndex <= 0 ? null : cmbPagamento.SelectedItem as string;

            var vendas = ConsultarVendas(inicio, fim, pagamento);
            var porPagamento = ResumoPorPagamento(vendas);
            var produtos = ProdutosMaisVendidos(vendas);

            // Indicadores
            decimal faturamento = vendas.Sum(v => v.Total);
            decimal descontos = vendas.Sum(v => v.Desconto);
            decimal ticket = vendas.Count == 0 ? 0 : faturamento / vendas.Count;

            DefinirValor(cardQtd, vendas.Count.ToString());
            DefinirValor(cardFaturamento, faturamento.ToString("C2"));
            DefinirValor(cardTicket, ticket.ToString("C2"));
            DefinirValor(cardDescontos, descontos.ToString("C2"));

            // Grid por forma de pagamento
            gridPagamento.AutoGenerateColumns = true;
            gridPagamento.DataSource = null;
            gridPagamento.DataSource = porPagamento;

            // Ajustes só se a coluna existir
            if (gridPagamento.Columns["FormaPagamento"] is { } colForma) colForma.HeaderText = "Pagamento";
            if (gridPagamento.Columns["Vendas"] is { } colVendas)
            {
                colVendas.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colVendas.Width = 70;
            }
            if (gridPagamento.Columns["Total"] is { } colTotal)
            {
                colTotal.DefaultCellStyle.Format = "C2";
                colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (gridPagamento.Columns["Participacao"] is { } colPart)
            {
                colPart.HeaderText = "%";
                colPart.DefaultCellStyle.Format = "P1";
                colPart.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                colPart.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colPart.Width = 70;
            }
            gridPagamento.ClearSelection();

            // Grid de produtos mais vendidos
            gridProdutos.AutoGenerateColumns = true;
            gridProdutos.DataSource = null;
            gridProdutos.DataSource = produtos;

            if (gridProdutos.Columns["Produto"] is { } colProd) colProd.HeaderText = "Produto";
            if (gridProdutos.Columns["QuantidadeVendida"] is { } colQtd)
            {
                colQtd.HeaderText = "Qtd vendida";
                colQtd.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colQtd.Width = 110;
            }
            if (gridProdutos.Columns["Faturamento"] is { } colFat)
            {
                colFat.DefaultCellStyle.Format = "C2";
                colFat.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            gridProdutos.ClearSelection();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao gerar o relatório: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Atualiza o Label "valor" de um cartão indicador
    private static void DefinirValor(Panel card, string texto)
    {
        if (card.Controls["valor"] is Label lbl) lbl.Text = texto;
    }

    // =====================================================================
    // ACESSO A DADOS PROVISÓRIO (em memória)
    // Quando o DAO ficar pronto, trocar estes métodos:
    //   ConsultarVendas(inicio, fim, pagamento) -> new VendaDAO().Listar(inicio, fim, pagamento)
    //   ResumoPorPagamento(vendas)              -> new VendaDAO().ResumoPorPagamento(inicio, fim, pagamento)
    //     (no banco: SELECT forma_pagamento, COUNT(*), SUM(total) ... GROUP BY forma_pagamento)
    //   ProdutosMaisVendidos(vendas)            -> new VendaDAO().ProdutosMaisVendidos(inicio, fim, pagamento)
    //     (no banco: JOIN ItensVenda + Produtos ... GROUP BY produto ORDER BY SUM(quantidade) DESC)
    // =====================================================================
    private static List<Venda> ConsultarVendas(DateTime? inicio, DateTime? fim, string? pagamento)
        => FrmNovaVenda.ListarVendas()
            .Where(v => (inicio == null || v.Data.Date >= inicio.Value)
                     && (fim == null || v.Data.Date <= fim.Value)
                     && (pagamento == null || v.FormaPagamento == pagamento))
            .ToList();

    private static List<LinhaPagamento> ResumoPorPagamento(List<Venda> vendas)
    {
        decimal totalGeral = vendas.Sum(v => v.Total);

        return vendas
            .GroupBy(v => v.FormaPagamento)
            .Select(g => new LinhaPagamento
            {
                FormaPagamento = g.Key,
                Vendas = g.Count(),
                Total = g.Sum(v => v.Total),
                Participacao = totalGeral == 0 ? 0 : g.Sum(v => v.Total) / totalGeral
            })
            .OrderByDescending(l => l.Total)
            .ToList();
    }

    private static List<LinhaProduto> ProdutosMaisVendidos(List<Venda> vendas)
        => vendas
            .SelectMany(v => v.Itens)
            .GroupBy(i => i.ProdutoNome)
            .Select(g => new LinhaProduto
            {
                Produto = g.Key,
                QuantidadeVendida = g.Sum(i => i.Quantidade),
                Faturamento = g.Sum(i => i.Subtotal)
            })
            .OrderByDescending(l => l.QuantidadeVendida)
            .Take(10)
            .ToList();
}

/// <summary>Linha do resumo por forma de pagamento (só pra exibir no grid).</summary>
public sealed class LinhaPagamento
{
    public string FormaPagamento { get; set; } = "";
    public int Vendas { get; set; }
    public decimal Total { get; set; }
    public decimal Participacao { get; set; } // fração: 0,25 = 25%
}

/// <summary>Linha do ranking de produtos (só pra exibir no grid).</summary>
public sealed class LinhaProduto
{
    public string Produto { get; set; } = "";
    public int QuantidadeVendida { get; set; }
    public decimal Faturamento { get; set; }
}