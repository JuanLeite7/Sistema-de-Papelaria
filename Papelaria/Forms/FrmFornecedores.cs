using Papelaria.Model;
using Papelaria.Util;

namespace Papelaria.Forms;

public class FrmFornecedores : Form
{
    // ---------- CONTROLES ----------
    private readonly TextBox txtPesquisa = new() { PlaceholderText = "Pesquisar por nome ou CNPJ..." };
    private readonly DataGridView grid = new();

    private readonly TextBox txtNome = new() { MaxLength = 100 };
    private readonly MaskedTextBox txtCnpj = new() { Mask = "00,000,000/0000-00" };
    private readonly MaskedTextBox txtTelefone = new() { Mask = "(00) 00000-0000" };
    private readonly TextBox txtEmail = new() { MaxLength = 100 };
    private readonly TextBox txtEndereco = new() { MaxLength = 150 };

    private readonly Button btnNovo = Tema.Botao("Novo", Tema.Primaria, 15, 165);
    private readonly Button btnSalvar = Tema.Botao("Salvar", Tema.Sucesso, 135, 165);
    private readonly Button btnExcluir = Tema.Botao("Excluir", Tema.Perigo, 255, 165);
    private readonly Button btnFechar = Tema.Botao("Fechar", Tema.Neutro, 735, 165);

    private Fornecedor? fornecedorAtual; // null = cadastrando um novo

    public FrmFornecedores()
    {
        Text = "Cadastro de Fornecedores";
        Font = Tema.Fonte;
        BackColor = Tema.Fundo;
        ClientSize = new Size(900, 660);
        MinimumSize = new Size(916, 700);
        StartPosition = FormStartPosition.CenterParent;

        MontarTela();
        CarregarGrid();
        LimparCampos();
    }

    // ---------- MONTAGEM DA TELA ----------
    private void MontarTela()
    {
        Controls.Add(Tema.Cabecalho("Fornecedores", "Cadastro e consulta de fornecedores"));

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
        var cardForm = Tema.Cartao(20, 425, 860, 220);
        cardForm.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        cardForm.Controls.Add(Tema.Rotulo("Nome / Razão social *", 15, 15));
        cardForm.Controls.Add(Tema.Rotulo("CNPJ *", 410, 15));
        cardForm.Controls.Add(Tema.Rotulo("Telefone", 595, 15));
        cardForm.Controls.Add(Tema.Rotulo("E-mail", 15, 78));
        cardForm.Controls.Add(Tema.Rotulo("Endereço", 410, 78));
        cardForm.Controls.Add(Tema.Rotulo("* campos obrigatórios", 15, 140));

        Tema.Campo(txtNome, 15, 38, 380);
        Tema.Campo(txtCnpj, 410, 38, 170);
        Tema.Campo(txtTelefone, 595, 38, 250);
        Tema.Campo(txtEmail, 15, 101, 380);
        Tema.Campo(txtEndereco, 410, 101, 435);

        btnFechar.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        cardForm.Controls.AddRange(new Control[]
        {
            txtNome, txtCnpj, txtTelefone, txtEmail, txtEndereco,
            btnNovo, btnSalvar, btnExcluir, btnFechar
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
            grid.DataSource = ListarFornecedores(txtPesquisa.Text.Trim());

            // Ajustes só se a coluna existir
            if (grid.Columns["Id"] is { } colId)
            {
                colId.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colId.Width = 50;
            }
            if (grid.Columns["Cnpj"] is { } colCnpj) colCnpj.HeaderText = "CNPJ";
            if (grid.Columns["Endereco"] is { } colEnd) colEnd.HeaderText = "Endereço";
            if (grid.Columns["Email"] is { } colEmail) colEmail.HeaderText = "E-mail";

            grid.ClearSelection(); // não preenche o formulário sozinho
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao carregar fornecedores: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (grid.SelectedRows.Count == 0 || grid.SelectedRows[0].DataBoundItem is not Fornecedor f) return;

        fornecedorAtual = f;
        txtNome.Text = f.Nome;
        txtCnpj.Text = f.Cnpj;
        txtTelefone.Text = f.Telefone;
        txtEmail.Text = f.Email;
        txtEndereco.Text = f.Endereco;
        btnExcluir.Enabled = true;
    }

    // ---------- AÇÕES ----------
    private void LimparCampos()
    {
        fornecedorAtual = null;
        txtNome.Clear();
        txtCnpj.Clear();
        txtTelefone.Clear();
        txtEmail.Clear();
        txtEndereco.Clear();
        grid.ClearSelection();
        btnExcluir.Enabled = false;
        txtNome.Focus();
    }

    private bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(txtNome.Text))
        {
            Aviso("Informe o nome do fornecedor.", txtNome);
            return false;
        }
        if (!txtCnpj.MaskCompleted)
        {
            Aviso("Informe o CNPJ completo.", txtCnpj);
            return false;
        }
        if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !txtEmail.Text.Contains('@'))
        {
            Aviso("E-mail inválido.", txtEmail);
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
            var f = fornecedorAtual ?? new Fornecedor();
            f.Nome = txtNome.Text.Trim();
            f.Cnpj = txtCnpj.Text;
            f.Telefone = txtTelefone.Text;
            f.Email = txtEmail.Text.Trim();
            f.Endereco = txtEndereco.Text.Trim();

            SalvarFornecedor(f);

            MessageBox.Show("Fornecedor salvo com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        if (fornecedorAtual == null) return;

        var resp = MessageBox.Show($"Excluir o fornecedor '{fornecedorAtual.Nome}'?", "Confirmação",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (resp != DialogResult.Yes) return;

        try
        {
            ExcluirFornecedor(fornecedorAtual.Id);
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
    // Quando o DAO ficar pronto, trocar estes 3 métodos:
    //   ListarFornecedores(filtro) -> new FornecedorDAO().Listar(filtro)
    //   SalvarFornecedor(f)        -> new FornecedorDAO().Salvar(f)  (insert ou update)
    //   ExcluirFornecedor(id)      -> new FornecedorDAO().Excluir(id)
    // =====================================================================
    private static readonly List<Fornecedor> bancoFake = new();
    private static int proximoId = 1;

    private static List<Fornecedor> ListarFornecedores(string filtro)
        => bancoFake
            .Where(f => string.IsNullOrEmpty(filtro)
                     || f.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase)
                     || f.Cnpj.Contains(filtro))
            .ToList();

    private static void SalvarFornecedor(Fornecedor f)
    {
        if (f.Id == 0)
        {
            f.Id = proximoId++;
            bancoFake.Add(f);
        }
        // se já tem Id, o objeto da lista já foi alterado (mesma referência)
    }

    private static void ExcluirFornecedor(int id) => bancoFake.RemoveAll(f => f.Id == id);
}