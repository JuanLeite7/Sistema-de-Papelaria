using System.ComponentModel;
using Papelaria.Model;
using Papelaria.Util;

namespace Papelaria.Forms;

public class FrmNovaVenda : Form
{
    // ---------- CONTROLES ----------
    private readonly ComboBox cmbCliente = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cmbProduto = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown nudQtd = new() { Minimum = 1, Maximum = 999999, Value = 1 };
    private readonly Label lblInfoProduto = new() { AutoSize = true, ForeColor = Tema.TextoSuave };
    private readonly Button btnAdicionar = Tema.Botao("Adicionar", Tema.Primaria, 790, 31, 155, 38);

    private readonly DataGridView grid = new();
    private readonly BindingList<ItemVenda> carrinho = new();

    private readonly ComboBox cmbPagamento = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown nudDesconto = new() { DecimalPlaces = 2, ThousandsSeparator = true, Maximum = 0 };
    private readonly Label lblSubtotal = new() { AutoSize = true, ForeColor = Tema.TextoSuave, Font = Tema.Fonte };
    private readonly Label lblTotal = new()
    {
        AutoSize = true,
        ForeColor = Tema.Sucesso,
        Font = new Font("Segoe UI", 22F, FontStyle.Bold)
    };

    private readonly Button btnFinalizar = Tema.Botao("Finalizar venda", Tema.Sucesso, 15, 95, 180, 42);
    private readonly Button btnRemover = Tema.Botao("Remover item", Tema.Perigo, 205, 95, 140, 42);
    private readonly Button btnLimpar = Tema.Botao("Limpar tudo", Tema.Alerta, 355, 95, 130, 42);
    private readonly Button btnFechar = Tema.Botao("Fechar", Tema.Neutro, 825, 95, 120, 42);

    public FrmNovaVenda()
    {
        Text = "Nova Venda";
        Font = Tema.Fonte;
        BackColor = Tema.Fundo;
        ClientSize = new Size(1000, 720);
        MinimumSize = new Size(1016, 760);
        StartPosition = FormStartPosition.CenterParent;

        CarregarClientes();  // antes de ligar os eventos
        CarregarProdutos();
        MontarTela();
        NovaVenda();
    }

    // ---------- COMBOS ----------
    private void CarregarClientes()
    {
        cmbCliente.Items.Add(new Cliente { Id = 0, Nome = "Consumidor final" });
        cmbCliente.Items.AddRange(ListarClientes().ToArray());
        cmbCliente.SelectedIndex = 0;
    }

    private void CarregarProdutos()
    {
        cmbProduto.Items.AddRange(ListarProdutos().ToArray());
    }

    // ---------- MONTAGEM DA TELA ----------
    private void MontarTela()
    {
        Controls.Add(Tema.Cabecalho("Nova Venda", "Monte o carrinho e finalize a venda"));

        // Cartão: adicionar produto
        var cardAdd = Tema.Cartao(20, 86, 960, 115);
        cardAdd.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        cardAdd.Controls.Add(Tema.Rotulo("Cliente", 15, 12));
        cardAdd.Controls.Add(Tema.Rotulo("Produto *", 310, 12));
        cardAdd.Controls.Add(Tema.Rotulo("Qtd *", 685, 12));

        Tema.Posicionar(cmbCliente, 15, 35, 280);
        Tema.Posicionar(cmbProduto, 310, 35, 360);
        Tema.Posicionar(nudQtd, 685, 35, 90);
        lblInfoProduto.Location = new Point(310, 80);

        btnAdicionar.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        cardAdd.Controls.AddRange(new Control[] { cmbCliente, cmbProduto, nudQtd, lblInfoProduto, btnAdicionar });
        Controls.Add(cardAdd);

        // Cartão: carrinho
        var cardCarrinho = Tema.Cartao(20, 213, 960, 330);
        cardCarrinho.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        var lblCarrinho = new Label
        {
            Text = "Carrinho",
            Font = Tema.FonteNegrito,
            ForeColor = Tema.Texto,
            Location = new Point(15, 15),
            AutoSize = true
        };

        Tema.EstilizarGrid(grid);
        grid.Location = new Point(15, 50);
        grid.Size = new Size(930, 268);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        ConfigurarGrid();

        cardCarrinho.Controls.Add(lblCarrinho);
        cardCarrinho.Controls.Add(grid);
        Controls.Add(cardCarrinho);

        // Cartão: pagamento, totais e botões
        var cardTotais = Tema.Cartao(20, 555, 960, 150);
        cardTotais.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        cmbPagamento.Items.AddRange(new object[] { "Dinheiro", "Cartão de débito", "Cartão de crédito", "Pix" });

        cardTotais.Controls.Add(Tema.Rotulo("Forma de pagamento *", 15, 15));
        cardTotais.Controls.Add(Tema.Rotulo("Desconto (R$)", 250, 15));
        Tema.Posicionar(cmbPagamento, 15, 38, 220);
        Tema.Posicionar(nudDesconto, 250, 38, 130);
        lblSubtotal.Location = new Point(560, 18);
        lblTotal.Location = new Point(558, 40);

        btnFechar.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        cardTotais.Controls.AddRange(new Control[]
        {
            cmbPagamento, nudDesconto, lblSubtotal, lblTotal,
            btnFinalizar, btnRemover, btnLimpar, btnFechar
        });
        Controls.Add(cardTotais);

        // Eventos
        cmbProduto.SelectedIndexChanged += (_, _) => AtualizarInfoProduto();
        nudDesconto.ValueChanged += (_, _) => RecalcularTotais();
        btnAdicionar.Click += BtnAdicionar_Click;
        btnRemover.Click += BtnRemover_Click;
        btnLimpar.Click += BtnLimpar_Click;
        btnFinalizar.Click += BtnFinalizar_Click;
        btnFechar.Click += (_, _) => Close();

        // Avisa se tentar sair com itens no carrinho (também vale pro X da janela)
        FormClosing += (_, e) =>
        {
            if (carrinho.Count > 0 &&
                MessageBox.Show("Há itens no carrinho. Sair mesmo assim?", "Atenção",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                e.Cancel = true;
        };
    }

    // ---------- GRID DO CARRINHO ----------
    private void ConfigurarGrid()
    {
        grid.AutoGenerateColumns = true;
        grid.DataSource = carrinho;

        // Ajustes só se a coluna existir
        if (grid.Columns["ProdutoId"] is { } colId) colId.Visible = false;
        if (grid.Columns["ProdutoNome"] is { } colProd) colProd.HeaderText = "Produto";
        if (grid.Columns["Quantidade"] is { } colQtd)
        {
            colQtd.HeaderText = "Qtd";
            colQtd.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colQtd.Width = 80;
        }
        if (grid.Columns["PrecoUnitario"] is { } colPreco)
        {
            colPreco.HeaderText = "Preço unit.";
            colPreco.DefaultCellStyle.Format = "C2";
            colPreco.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        if (grid.Columns["Subtotal"] is { } colSub)
        {
            colSub.DefaultCellStyle.Format = "C2";
            colSub.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
    }

    // ---------- LÓGICA DO CARRINHO ----------
    private int QuantidadeNoCarrinho(int produtoId)
        => carrinho.Where(i => i.ProdutoId == produtoId).Sum(i => i.Quantidade);

    private void AtualizarInfoProduto()
    {
        if (cmbProduto.SelectedItem is Produto p)
        {
            int disponivel = p.Estoque - QuantidadeNoCarrinho(p.Id);
            lblInfoProduto.Text = $"Preço: {p.Preco:C2}   |   Disponível: {disponivel} un.";
            lblInfoProduto.ForeColor = disponivel <= 0 ? Tema.Perigo : Tema.TextoSuave;
        }
        else
        {
            lblInfoProduto.Text = "Selecione um produto";
            lblInfoProduto.ForeColor = Tema.TextoSuave;
        }
    }

    private void RecalcularTotais()
    {
        decimal subtotal = carrinho.Sum(i => i.Subtotal);

        nudDesconto.Maximum = subtotal; // desconto nunca passa do subtotal
        decimal total = subtotal - nudDesconto.Value;

        lblSubtotal.Text = $"Subtotal: {subtotal:C2}";
        lblTotal.Text = $"Total: {total:C2}";
    }

    private void NovaVenda()
    {
        carrinho.Clear();
        cmbCliente.SelectedIndex = 0;
        cmbProduto.SelectedIndex = -1;
        cmbPagamento.SelectedIndex = -1;
        nudQtd.Value = 1;
        nudDesconto.Value = 0;
        RecalcularTotais();
        AtualizarInfoProduto();
    }

    // ---------- AÇÕES ----------
    private static void Aviso(string msg, Control campo)
    {
        MessageBox.Show(msg, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        campo.Focus();
    }

    private void BtnAdicionar_Click(object? sender, EventArgs e)
    {
        if (cmbProduto.SelectedItem is not Produto p)
        {
            Aviso("Selecione o produto.", cmbProduto);
            return;
        }

        int qtd = (int)nudQtd.Value;
        int noCarrinho = QuantidadeNoCarrinho(p.Id);

        if (qtd + noCarrinho > p.Estoque)
        {
            Aviso($"Estoque insuficiente. Disponível: {p.Estoque - noCarrinho} un.", nudQtd);
            return;
        }

        var existente = carrinho.FirstOrDefault(i => i.ProdutoId == p.Id);
        if (existente != null)
        {
            existente.Quantidade += qtd;
            carrinho.ResetItem(carrinho.IndexOf(existente)); // atualiza a linha no grid
        }
        else
        {
            carrinho.Add(new ItemVenda
            {
                ProdutoId = p.Id,
                ProdutoNome = p.Nome,
                Quantidade = qtd,
                PrecoUnitario = p.Preco
            });
        }

        nudQtd.Value = 1;
        cmbProduto.SelectedIndex = -1;
        RecalcularTotais();
        grid.ClearSelection();
        cmbProduto.Focus();
    }

    private void BtnRemover_Click(object? sender, EventArgs e)
    {
        if (grid.SelectedRows.Count == 0 || grid.SelectedRows[0].DataBoundItem is not ItemVenda item)
        {
            MessageBox.Show("Clique no item do carrinho que deseja remover.", "Atenção",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        carrinho.Remove(item);
        RecalcularTotais();
        AtualizarInfoProduto();
        grid.ClearSelection();
    }

    private void BtnLimpar_Click(object? sender, EventArgs e)
    {
        if (carrinho.Count > 0)
        {
            var resp = MessageBox.Show("Limpar o carrinho e começar de novo?", "Confirmação",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (resp != DialogResult.Yes) return;
        }
        NovaVenda();
    }

    private void BtnFinalizar_Click(object? sender, EventArgs e)
    {
        if (carrinho.Count == 0)
        {
            Aviso("Adicione pelo menos um produto ao carrinho.", cmbProduto);
            return;
        }
        if (cmbPagamento.SelectedItem is not string pagamento)
        {
            Aviso("Selecione a forma de pagamento.", cmbPagamento);
            return;
        }

        decimal subtotal = carrinho.Sum(i => i.Subtotal);
        decimal desconto = nudDesconto.Value;
        decimal total = subtotal - desconto;

        var resp = MessageBox.Show($"Finalizar a venda no valor de {total:C2}?", "Confirmação",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (resp != DialogResult.Yes) return;

        try
        {
            var cliente = (Cliente)cmbCliente.SelectedItem!;

            var venda = new Venda
            {
                Data = DateTime.Now,
                ClienteId = cliente.Id == 0 ? null : cliente.Id,
                ClienteNome = cliente.Nome,
                FormaPagamento = pagamento,
                Subtotal = subtotal,
                Desconto = desconto,
                Total = total,
                Itens = carrinho.ToList()
            };

            RegistrarVenda(venda);

            MessageBox.Show($"Venda #{venda.Id} registrada com sucesso!\nTotal: {venda.Total:C2}", "Sucesso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            NovaVenda();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao registrar a venda: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // =====================================================================
    // ACESSO A DADOS PROVISÓRIO (em memória)
    // Quando o DAO ficar pronto, trocar estes métodos:
    //   ListarClientes()        -> new ClienteDAO().Listar("")
    //   ListarProdutos()        -> new ProdutoDAO().Listar("", 0)
    //   ListarVendas()          -> new VendaDAO().Listar()   (usado pelo Histórico)
    //   RegistrarVenda(venda)   -> new VendaDAO().Registrar(venda)
    //     IMPORTANTE: no banco, o Registrar deve fazer TUDO numa TRANSAÇÃO:
    //       1) INSERT na Venda
    //       2) INSERT em ItensVenda (um por item)
    //       3) UPDATE do estoque de cada produto (baixa)
    //       4) INSERT da movimentação de saída (se houver tabela de movimentação)
    //     Se qualquer passo falhar: ROLLBACK e nada é gravado.
    // =====================================================================
    private static List<Cliente> ListarClientes() => FrmClientes.ListarClientes("");
    private static List<Produto> ListarProdutos() => FrmProdutos.ListarProdutos("", 0);

    private static readonly List<Venda> bancoFake = new();
    private static int proximoId = 1;

    internal static List<Venda> ListarVendas()
        => bancoFake.OrderByDescending(v => v.Data).ToList();

    private static void RegistrarVenda(Venda venda)
    {
        var produtos = FrmProdutos.ListarProdutos("", 0);

        // 1) Valida TUDO antes de alterar qualquer coisa (simula o "tudo ou nada" da transação)
        foreach (var item in venda.Itens)
        {
            var p = produtos.FirstOrDefault(x => x.Id == item.ProdutoId)
                ?? throw new InvalidOperationException($"Produto '{item.ProdutoNome}' não encontrado.");

            if (p.Estoque < item.Quantidade)
                throw new InvalidOperationException($"Estoque insuficiente para '{p.Nome}'.");
        }

        // 2) Baixa o estoque
        foreach (var item in venda.Itens)
            produtos.First(x => x.Id == item.ProdutoId).Estoque -= item.Quantidade;

        // 3) Grava a venda
        venda.Id = proximoId++;
        bancoFake.Add(venda);
    }
}