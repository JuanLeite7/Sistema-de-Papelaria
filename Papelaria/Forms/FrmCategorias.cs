using System.Drawing;
using Papelaria.Model;
using Papelaria.Util;

namespace Papelaria.Forms;

public class FrmCategorias : Form
{
    // ---------- CONTROLES ----------
    private readonly TextBox txtPesquisa = new() { PlaceholderText = "Pesquisar por nome..." };
    private readonly DataGridView grid = new();

    private readonly TextBox txtNome = new() { MaxLength = 100 };
    private readonly TextBox txtDescricao = new() { MaxLength = 200 };

    private readonly Button btnNovo = Tema.Botao("Novo", Tema.Primaria, 15, 100);
    private readonly Button btnSalvar = Tema.Botao("Salvar", Tema.Sucesso, 135, 100);
    private readonly Button btnExcluir = Tema.Botao("Excluir", Tema.Perigo, 255, 100);
    private readonly Button btnFechar = Tema.Botao("Fechar", Tema.Neutro, 735, 100);

    private Categoria? categoriaAtual; // null = cadastrando uma nova

    public FrmCategorias()
    {
        Text = "Cadastro de Categorias";
        Font = Tema.Fonte;
        BackColor = Tema.Fundo;
        ClientSize = new Size(900, 600);
        MinimumSize = new Size(916, 640);
        StartPosition = FormStartPosition.CenterParent;

        MontarTela();
        CarregarGrid();
        LimparCampos();
    }

    // ---------- MONTAGEM DA TELA ----------
    private void MontarTela()
    {
        Controls.Add(Tema.Cabecalho("Categorias", "Organize os produtos por categoria"));

        // Cartão da lista (pesquisa + grid)
        var cardLista = Tema.Cartao(20, 92, 860, 320);
        cardLista.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        Tema.Campo(txtPesquisa, 15, 15, 340);
        Tema.EstilizarGrid(grid);
        grid.Location = new Point(15, 55);
        grid.Size = new Size(830, 250);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        cardLista.Controls.Add(txtPesquisa);
        cardLista.Controls.Add(grid);
        Controls.Add(cardLista);

        // Cartão do formulário
        var cardForm = Tema.Cartao(20, 425, 860, 150);
        cardForm.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        cardForm.Controls.Add(Tema.Rotulo("Nome *", 15, 15));
        cardForm.Controls.Add(Tema.Rotulo("Descrição", 410, 15));
        cardForm.Controls.Add(Tema.Rotulo("* campos obrigatórios", 15, 78));

        Tema.Campo(txtNome, 15, 38, 380);
        Tema.Campo(txtDescricao, 410, 38, 435);

        btnFechar.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        cardForm.Controls.AddRange(new Control[]
        {
            txtNome, txtDescricao, btnNovo, btnSalvar, btnExcluir, btnFechar
        });
        Controls.Add(cardForm);

        // Eventos
        txtPesquisa.TextChanged += (_, _) => CarregarGrid();
        grid.SelectionChanged += Grid_SelectionChanged;
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
            grid.AutoGenerateColumns = true;
            grid.DataSource = null;
            grid.DataSource = ListarCategorias(txtPesquisa.Text.Trim());

            // Ajustes só se a coluna existir
            if (grid.Columns["Id"] is { } colId)
            {
                colId.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colId.Width = 50;
            }
            if (grid.Columns["Descricao"] is { } colDesc) colDesc.HeaderText = "Descrição";

            grid.ClearSelection(); // não preenche o formulário sozinho
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao carregar categorias: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (grid.SelectedRows.Count == 0 || grid.SelectedRows[0].DataBoundItem is not Categoria c) return;

        categoriaAtual = c;
        txtNome.Text = c.Nome;
        txtDescricao.Text = c.Descricao;
        btnExcluir.Enabled = true;
    }

    // ---------- AÇÕES ----------
    private void LimparCampos()
    {
        categoriaAtual = null;
        txtNome.Clear();
        txtDescricao.Clear();
        grid.ClearSelection();
        btnExcluir.Enabled = false;
        txtNome.Focus();
    }

    private bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(txtNome.Text))
        {
            MessageBox.Show("Informe o nome da categoria.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtNome.Focus();
            return false;
        }
        return true;
    }

    private void BtnSalvar_Click(object? sender, EventArgs e)
    {
        if (!ValidarCampos()) return;

        try
        {
            var c = categoriaAtual ?? new Categoria();
            c.Nome = txtNome.Text.Trim();
            c.Descricao = txtDescricao.Text.Trim();

            SalvarCategoria(c);

            MessageBox.Show("Categoria salva com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        if (categoriaAtual == null) return;

        var resp = MessageBox.Show($"Excluir a categoria '{categoriaAtual.Nome}'?", "Confirmação",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (resp != DialogResult.Yes) return;

        try
        {
            ExcluirCategoria(categoriaAtual.Id);
            CarregarGrid();
            LimparCampos();
        }
        catch (Exception ex)
        {
            // Com o banco, aqui vai cair o erro de chave estrangeira se a categoria tiver produtos
            MessageBox.Show("Erro ao excluir: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // =====================================================================
    // ACESSO A DADOS PROVISÓRIO (em memória)
    // Quando o DAO ficar pronto, trocar estes 3 métodos:
    //   ListarCategorias(filtro) -> new CategoriaDAO().Listar(filtro)
    //   SalvarCategoria(c)       -> new CategoriaDAO().Salvar(c)  (insert ou update)
    //   ExcluirCategoria(id)     -> new CategoriaDAO().Excluir(id)
    // =====================================================================
    private static readonly List<Categoria> bancoFake = new();
    private static int proximoId = 1;

    internal static List<Categoria> ListarCategorias(string filtro)
        => bancoFake
            .Where(c => string.IsNullOrEmpty(filtro)
                     || c.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase))
            .ToList();

    private static void SalvarCategoria(Categoria c)
    {
        if (c.Id == 0)
        {
            c.Id = proximoId++;
            bancoFake.Add(c);
        }
        // se já tem Id, o objeto da lista já foi alterado (mesma referência)
    }

    private static void ExcluirCategoria(int id) => bancoFake.RemoveAll(c => c.Id == id);
}