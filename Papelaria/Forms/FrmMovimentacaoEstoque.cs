using Papelaria.Model;
using Papelaria.Util;

namespace Papelaria.Forms;

public class FrmMovimentacaoEstoque : Form
{
    // ---------- CONTROLES ----------
    private readonly ComboBox cmbProduto = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cmbTipo = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown nudQtd = new() { Minimum = 1, Maximum = 999999, Value = 1 };
    private readonly TextBox txtMotivo = new() { MaxLength = 150, PlaceholderText = "Ex: compra do fornecedor, ajuste, perda..." };
    private readonly Label lblEstoque = new()
    {
        Text = "-",
        AutoSize = true,
        ForeColor = Tema.Texto,
        Font = new Font("Segoe UI", 14F, FontStyle.Bold)
    };

    private readonly ComboBox cmbFiltro = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView grid = new();

    private readonly Button btnRegistrar = Tema.Botao("Registrar", Tema.Sucesso, 15, 155, 130);
    private readonly Button btnLimpar = Tema.Botao("Limpar", Tema.Primaria, 155, 155);
    private readonly Button btnFechar = Tema.Botao("Fechar", Tema.Neutro, 735, 155);

    private List<Produto> produtos = new();

    public FrmMovimentacaoEstoque()
    {
        Text = "Movimentação de Estoque";
        Font = Tema.Fonte;
        BackColor = Tema.Fundo;
        ClientSize = new Size(900, 660);
        MinimumSize = new Size(916, 700);
        StartPosition = FormStartPosition.CenterParent;

        CarregarProdutos(); // antes de ligar os eventos
        MontarTela();
        CarregarGrid();
        LimparCampos();
    }

    // ---------- PRODUTOS (combos) ----------
    private void CarregarProdutos()
    {
        produtos = ListarProdutos();

        cmbProduto.Items.AddRange(produtos.ToArray());

        cmbFiltro.Items.Add(new Produto { Id = 0, Nome = "Todos os produtos" });
        cmbFiltro.Items.AddRange(produtos.ToArray());
        cmbFiltro.SelectedIndex = 0;
    }

    // ---------- MONTAGEM DA TELA ----------
    private void MontarTela()
    {
        Controls.Add(Tema.Cabecalho("Movimentação de Estoque", "Registre entradas e saídas de produtos"));

        // Cartão do formulário (nova movimentação)
        var cardForm = Tema.Cartao(20, 92, 860, 205);

        cmbTipo.Items.AddRange(new object[] { "Entrada", "Saída" });

        cardForm.Controls.Add(Tema.Rotulo("Produto *", 15, 15));
        cardForm.Controls.Add(Tema.Rotulo("Tipo *", 410, 15));
        cardForm.Controls.Add(Tema.Rotulo("Quantidade *", 595, 15));
        cardForm.Controls.Add(Tema.Rotulo("Motivo", 15, 78));
        cardForm.Controls.Add(Tema.Rotulo("Estoque atual do produto", 410, 78));
        cardForm.Controls.Add(Tema.Rotulo("* campos obrigatórios", 15, 132));

        Tema.Posicionar(cmbProduto, 15, 38, 380);
        Tema.Posicionar(cmbTipo, 410, 38, 170);
        Tema.Posicionar(nudQtd, 595, 38, 250);
        Tema.Campo(txtMotivo, 15, 101, 380);
        lblEstoque.Location = new Point(410, 98);

        btnFechar.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        cardForm.Controls.AddRange(new Control[]
        {
            cmbProduto, cmbTipo, nudQtd, txtMotivo, lblEstoque,
            btnRegistrar, btnLimpar, btnFechar
        });
        Controls.Add(cardForm);

        // Cartão do histórico
        var cardHist = Tema.Cartao(20, 310, 860, 330);
        cardHist.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        var lblHist = new Label
        {
            Text = "Histórico de movimentações",
            Font = Tema.FonteNegrito,
            ForeColor = Tema.Texto,
            Location = new Point(15, 18),
            AutoSize = true
        };
        Tema.Posicionar(cmbFiltro, 590, 14, 255);
        cmbFiltro.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        Tema.EstilizarGrid(grid);
        grid.Location = new Point(15, 55);
        grid.Size = new Size(830, 260);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        cardHist.Controls.AddRange(new Control[] { lblHist, cmbFiltro, grid });
        Controls.Add(cardHist);

        // Eventos
        cmbProduto.SelectedIndexChanged += (_, _) => AtualizarEstoqueAtual();
        cmbFiltro.SelectedIndexChanged += (_, _) => CarregarGrid();
        grid.CellFormatting += Grid_CellFormatting;
        btnRegistrar.Click += BtnRegistrar_Click;
        btnLimpar.Click += (_, _) => LimparCampos();
        btnFechar.Click += (_, _) => Close();
    }

    // ---------- GRID ----------
    private void CarregarGrid()
    {
        try
        {
            int produtoId = cmbFiltro.SelectedItem is Produto filtro ? filtro.Id : 0;

            grid.AutoGenerateColumns = true;
            grid.DataSource = null;
            grid.DataSource = ListarMovimentacoes(produtoId);

            // Ajustes só se a coluna existir
            if (grid.Columns["Id"] is { } colId)
            {
                colId.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colId.Width = 50;
            }
            if (grid.Columns["ProdutoId"] is { } colProdId) colProdId.Visible = false;
            if (grid.Columns["ProdutoNome"] is { } colProd) colProd.HeaderText = "Produto";
            if (grid.Columns["EstoqueApos"] is { } colApos) colApos.HeaderText = "Estoque após";
            if (grid.Columns["Data"] is { } colData) colData.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

            grid.ClearSelection();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao carregar movimentações: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Entrada em verde, Saída em vermelho
    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Tipo") return;

        e.CellStyle.ForeColor = e.Value as string == "Entrada" ? Tema.Sucesso : Tema.Perigo;
        e.CellStyle.Font = Tema.FonteNegrito;
    }

    // ---------- AÇÕES ----------
    private void AtualizarEstoqueAtual()
    {
        if (cmbProduto.SelectedItem is Produto p)
        {
            lblEstoque.Text = $"{p.Estoque} un.";
            lblEstoque.ForeColor = p.Estoque <= p.EstoqueMinimo ? Tema.Perigo : Tema.Texto;
        }
        else
        {
            lblEstoque.Text = "-";
            lblEstoque.ForeColor = Tema.Texto;
        }
    }

    private void LimparCampos()
    {
        cmbProduto.SelectedIndex = -1;
        cmbTipo.SelectedIndex = -1;
        nudQtd.Value = 1;
        txtMotivo.Clear();
        cmbProduto.Focus();
    }

    private bool ValidarCampos()
    {
        if (cmbProduto.SelectedItem is not Produto produto)
        {
            Aviso("Selecione o produto.", cmbProduto);
            return false;
        }
        if (cmbTipo.SelectedItem is not string tipo)
        {
            Aviso("Selecione o tipo da movimentação (Entrada ou Saída).", cmbTipo);
            return false;
        }
        if (nudQtd.Value <= 0)
        {
            Aviso("Informe uma quantidade maior que zero.", nudQtd);
            return false;
        }
        if (tipo == "Saída" && nudQtd.Value > produto.Estoque)
        {
            Aviso($"Estoque insuficiente. Disponível: {produto.Estoque} un.", nudQtd);
            return false;
        }
        return true;
    }

    private static void Aviso(string msg, Control campo)
    {
        MessageBox.Show(msg, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        campo.Focus();
    }

    private void BtnRegistrar_Click(object? sender, EventArgs e)
    {
        if (!ValidarCampos()) return;

        try
        {
            var produto = (Produto)cmbProduto.SelectedItem!;

            var mov = new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                ProdutoNome = produto.Nome,
                Tipo = (string)cmbTipo.SelectedItem!,
                Quantidade = (int)nudQtd.Value,
                Motivo = txtMotivo.Text.Trim(),
                Data = DateTime.Now
            };

            RegistrarMovimentacao(mov, produto);

            MessageBox.Show("Movimentação registrada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            CarregarGrid();
            LimparCampos();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao registrar: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // =====================================================================
    // ACESSO A DADOS PROVISÓRIO (em memória)
    // Quando o DAO ficar pronto, trocar estes métodos:
    //   ListarProdutos()                  -> new ProdutoDAO().Listar("", 0)
    //   ListarMovimentacoes(produtoId)    -> new MovimentacaoDAO().Listar(produtoId)
    //   RegistrarMovimentacao(mov, prod)  -> new MovimentacaoDAO().Registrar(mov)
    //     IMPORTANTE: no banco, o Registrar deve gravar a movimentação E
    //     atualizar o estoque do produto na MESMA TRANSAÇÃO (ou nada é gravado).
    // =====================================================================
    private static readonly List<MovimentacaoEstoque> bancoFake = new();
    private static int proximoId = 1;

    private static List<Produto> ListarProdutos() => FrmProdutos.ListarProdutos("", 0);

    private static List<MovimentacaoEstoque> ListarMovimentacoes(int produtoId)
        => bancoFake
            .Where(m => produtoId == 0 || m.ProdutoId == produtoId)
            .OrderByDescending(m => m.Data)
            .ToList();

    private static void RegistrarMovimentacao(MovimentacaoEstoque mov, Produto produto)
    {
        // Atualiza o saldo do produto (no banco isso vira um UPDATE dentro da transação)
        produto.Estoque += mov.Tipo == "Entrada" ? mov.Quantidade : -mov.Quantidade;
        mov.EstoqueApos = produto.Estoque;

        mov.Id = proximoId++;
        bancoFake.Add(mov);
    }
}