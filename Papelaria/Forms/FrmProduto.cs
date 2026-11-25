using System.Drawing;
using Papelaria.Model;
using Papelaria.Util;

namespace Papelaria.Forms;

public class FrmProdutos : Form
{
    // ---------- CONTROLES ----------
    private readonly TextBox txtPesquisa = new() { PlaceholderText = "Pesquisar por nome..." };
    private readonly ComboBox cmbFiltroCategoria = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView grid = new();

    private readonly TextBox txtNome = new() { MaxLength = 100 };
    private readonly ComboBox cmbCategoria = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown nudPreco = new() { DecimalPlaces = 2, Maximum = 999999.99m, ThousandsSeparator = true };
    private readonly NumericUpDown nudEstoque = new() { Maximum = 999999 };
    private readonly NumericUpDown nudEstoqueMin = new() { Maximum = 999999 };

    private readonly Button btnNovo = Tema.Botao("Novo", Tema.Primaria, 15, 175);
    private readonly Button btnSalvar = Tema.Botao("Salvar", Tema.Sucesso, 135, 175);
    private readonly Button btnExcluir = Tema.Botao("Excluir", Tema.Perigo, 255, 175);
    private readonly Button btnFechar = Tema.Botao("Fechar", Tema.Neutro, 735, 175);

    private Produto? produtoAtual; // null = cadastrando um novo
    private List<Categoria> categorias = new();

    public FrmProdutos()
    {
        Text = "Cadastro de Produtos";
        Font = Tema.Fonte;
        BackColor = Tema.Fundo;
        ClientSize = new Size(900, 670);
        MinimumSize = new Size(916, 710);
        StartPosition = FormStartPosition.CenterParent;

        CarregarCategorias(); // antes de ligar os eventos
        MontarTela();
        CarregarGrid();
        LimparCampos();
    }

    // ---------- CATEGORIAS (combos) ----------
    private void CarregarCategorias()
    {
        categorias = ListarCategorias();

        cmbCategoria.Items.AddRange(categorias.ToArray());

        cmbFiltroCategoria.Items.Add(new Categoria { Id = 0, Nome = "Todas as categorias" });
        cmbFiltroCategoria.Items.AddRange(categorias.ToArray());
        cmbFiltroCategoria.SelectedIndex = 0;
    }

    // ---------- MONTAGEM DA TELA ----------
    private void MontarTela()
    {
        Controls.Add(Tema.Cabecalho("Produtos", "Cadastro, consulta e controle de produtos"));

        // Cartão da lista (pesquisa + filtro + grid)
        var cardLista = Tema.Cartao(20, 92, 860, 320);
        cardLista.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        Tema.Campo(txtPesquisa, 15, 15, 340);
        Tema.Posicionar(cmbFiltroCategoria, 370, 15, 240);
        Tema.EstilizarGrid(grid);
        grid.Location = new Point(15, 55);
        grid.Size = new Size(830, 250);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        cardLista.Controls.Add(Tema.Rotulo("Em vermelho: estoque abaixo do mínimo", 625, 19));
        cardLista.Controls.AddRange(new Control[] { txtPesquisa, cmbFiltroCategoria, grid });
        Controls.Add(cardLista);

        // Cartão do formulário
        var cardForm = Tema.Cartao(20, 425, 860, 230);
        cardForm.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        cardForm.Controls.Add(Tema.Rotulo("Nome *", 15, 15));
        cardForm.Controls.Add(Tema.Rotulo("Categoria *", 410, 15));
        cardForm.Controls.Add(Tema.Rotulo("Preço (R$) *", 15, 78));
        cardForm.Controls.Add(Tema.Rotulo("Estoque inicial", 205, 78));
        cardForm.Controls.Add(Tema.Rotulo("Estoque mínimo", 395, 78));
        cardForm.Controls.Add(Tema.Rotulo("* campos obrigatórios. Depois de cadastrado, o estoque só muda pela Movimentação.", 15, 145));

        Tema.Campo(txtNome, 15, 38, 380);
        Tema.Posicionar(cmbCategoria, 410, 38, 435);
        Tema.Posicionar(nudPreco, 15, 101, 170);
        Tema.Posicionar(nudEstoque, 205, 101, 170);
        Tema.Posicionar(nudEstoqueMin, 395, 101, 170);

        btnFechar.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        cardForm.Controls.AddRange(new Control[]
        {
            txtNome, cmbCategoria, nudPreco, nudEstoque, nudEstoqueMin,
            btnNovo, btnSalvar, btnExcluir, btnFechar
        });
        Controls.Add(cardForm);

        // Eventos
        txtPesquisa.TextChanged += (_, _) => CarregarGrid();
        cmbFiltroCategoria.SelectedIndexChanged += (_, _) => CarregarGrid();
        grid.SelectionChanged += Grid_SelectionChanged;
        grid.CellFormatting += Grid_CellFormatting;
        btnNovo.Click += (_, _) => LimparCampos();
        btnSalvar.Click += BtnSalvar_Click;
        btnExcluir.Click += BtnExcluir_Click;
        btnFechar.Click += (_, _) => Close();
    }

    // ---------- GRID ----------
    private void CarregarGrid()
    {
        try
        {
            int categoriaId = cmbFiltroCategoria.SelectedItem is Categoria cat ? cat.Id : 0;

            grid.AutoGenerateColumns = true;
            grid.DataSource = null;
            grid.DataSource = ListarProdutos(txtPesquisa.Text.Trim(), categoriaId);

            // Ajustes só se a coluna existir
            if (grid.Columns["Id"] is { } colId)
            {
                colId.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colId.Width = 50;
            }
            if (grid.Columns["CategoriaId"] is { } colCatId) colCatId.Visible = false;
            if (grid.Columns["CategoriaNome"] is { } colCat) colCat.HeaderText = "Categoria";
            if (grid.Columns["EstoqueMinimo"] is { } colMin) colMin.HeaderText = "Estoque mín.";
            if (grid.Columns["Preco"] is { } colPreco)
            {
                colPreco.HeaderText = "Preço";
                colPreco.DefaultCellStyle.Format = "C2"; // formata como moeda
            }

            grid.ClearSelection(); // não preenche o formulário sozinho
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao carregar produtos: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Linha em vermelho quando o estoque está abaixo (ou igual) ao mínimo
    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (grid.Rows[e.RowIndex].DataBoundItem is Produto p && p.Estoque <= p.EstoqueMinimo)
            e.CellStyle.ForeColor = Tema.Perigo;
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (grid.SelectedRows.Count == 0 || grid.SelectedRows[0].DataBoundItem is not Produto p) return;

        produtoAtual = p;
        txtNome.Text = p.Nome;
        cmbCategoria.SelectedItem = categorias.FirstOrDefault(c => c.Id == p.CategoriaId);
        nudPreco.Value = p.Preco;
        nudEstoque.Value = p.Estoque;
        nudEstoqueMin.Value = p.EstoqueMinimo;
        nudEstoque.Enabled = false; // estoque só muda por movimentação
        btnExcluir.Enabled = true;
    }

    // ---------- AÇÕES ----------
    private void LimparCampos()
    {
        produtoAtual = null;
        txtNome.Clear();
        cmbCategoria.SelectedIndex = -1;
        nudPreco.Value = 0;
        nudEstoque.Value = 0;
        nudEstoqueMin.Value = 0;
        nudEstoque.Enabled = true;
        grid.ClearSelection();
        btnExcluir.Enabled = false;
        txtNome.Focus();
    }

    private bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(txtNome.Text))
        {
            Aviso("Informe o nome do produto.", txtNome);
            return false;
        }
        if (cmbCategoria.SelectedItem is not Categoria)
        {
            Aviso("Selecione a categoria.", cmbCategoria);
            return false;
        }
        if (nudPreco.Value <= 0)
        {
            Aviso("Informe um preço maior que zero.", nudPreco);
            return false;
        }
        return true;
    }

    private static void Aviso(string msg, Control campo)
    {
        MessageBox.Show(msg, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        campo.Focus();
    }

    private void BtnSalvar_Click(object? sender, EventArgs e)
    {
        if (!ValidarCampos()) return;

        try
        {
            var cat = (Categoria)cmbCategoria.SelectedItem!;
            var p = produtoAtual ?? new Produto();

            p.Nome = txtNome.Text.Trim();
            p.CategoriaId = cat.Id;
            p.CategoriaNome = cat.Nome;
            p.Preco = nudPreco.Value;
            p.EstoqueMinimo = (int)nudEstoqueMin.Value;
            if (produtoAtual == null) p.Estoque = (int)nudEstoque.Value; // só no cadastro

            SalvarProduto(p);

            MessageBox.Show("Produto salvo com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            CarregarGrid();
            LimparCampos();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao salvar: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnExcluir_Click(object? sender, EventArgs e)
    {
        if (produtoAtual == null) return;

        var resp = MessageBox.Show($"Excluir o produto '{produtoAtual.Nome}'?", "Confirmação",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (resp != DialogResult.Yes) return;

        try
        {
            ExcluirProduto(produtoAtual.Id);
            CarregarGrid();
            LimparCampos();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao excluir: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // =====================================================================
    // ACESSO A DADOS PROVISÓRIO (em memória)
    // Quando o DAO ficar pronto, trocar estes métodos:
    //   ListarCategorias()                -> new CategoriaDAO().Listar()
    //   ListarProdutos(filtro, categoria) -> new ProdutoDAO().Listar(filtro, categoriaId)
    //   SalvarProduto(p)                  -> new ProdutoDAO().Salvar(p)  (insert ou update)
    //   ExcluirProduto(id)                -> new ProdutoDAO().Excluir(id)
    // =====================================================================
    private static List<Categoria> ListarCategorias() => new();

    private static readonly List<Produto> bancoFake = new();
    private static int proximoId = 1;

    private static List<Produto> ListarProdutos(string filtro, int categoriaId)
        => bancoFake
            .Where(p => (string.IsNullOrEmpty(filtro) || p.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                     && (categoriaId == 0 || p.CategoriaId == categoriaId))
            .ToList();

    private static void SalvarProduto(Produto p)
    {
        if (p.Id == 0)
        {
            p.Id = proximoId++;
            bancoFake.Add(p);
        }
        // se já tem Id, o objeto da lista já foi alterado (mesma referência)
    }

    private static void ExcluirProduto(int id) => bancoFake.RemoveAll(p => p.Id == id);
}