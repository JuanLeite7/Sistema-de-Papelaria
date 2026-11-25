using System.Drawing;
using Papelaria.Model;
using Papelaria.Util;

namespace Papelaria.Forms;

public class FrmClientes : Form
{
    // ---------- CONTROLES ----------
    private readonly TextBox txtPesquisa = new() { PlaceholderText = "Pesquisar por nome ou CPF..." };
    private readonly DataGridView grid = new();

    private readonly TextBox txtNome = new() { MaxLength = 100 };
    private readonly MaskedTextBox txtCpf = new() { Mask = "000,000,000-00" };
    private readonly MaskedTextBox txtTelefone = new() { Mask = "(00) 00000-0000" };
    private readonly TextBox txtEmail = new() { MaxLength = 100 };
    private readonly TextBox txtEndereco = new() { MaxLength = 150 };

    private readonly Button btnNovo = Tema.Botao("Novo", Tema.Primaria, 15, 165);
    private readonly Button btnSalvar = Tema.Botao("Salvar", Tema.Sucesso, 135, 165);
    private readonly Button btnExcluir = Tema.Botao("Excluir", Tema.Perigo, 255, 165);
    private readonly Button btnFechar = Tema.Botao("Fechar", Tema.Neutro, 735, 165);

    private Cliente? clienteAtual; // null = cadastrando um novo

    public FrmClientes()
    {
        Text = "Cadastro de Clientes";
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
        Controls.Add(Tema.Cabecalho("Clientes", "Cadastro e consulta de clientes"));

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

        cardForm.Controls.Add(Tema.Rotulo("Nome *", 15, 15));
        cardForm.Controls.Add(Tema.Rotulo("CPF *", 410, 15));
        cardForm.Controls.Add(Tema.Rotulo("Telefone", 595, 15));
        cardForm.Controls.Add(Tema.Rotulo("E-mail", 15, 78));
        cardForm.Controls.Add(Tema.Rotulo("Endereço", 410, 78));
        cardForm.Controls.Add(Tema.Rotulo("* campos obrigatórios", 15, 140));

        Tema.Campo(txtNome, 15, 38, 380);
        Tema.Campo(txtCpf, 410, 38, 170);
        Tema.Campo(txtTelefone, 595, 38, 250);
        Tema.Campo(txtEmail, 15, 101, 380);
        Tema.Campo(txtEndereco, 410, 101, 435);

        btnFechar.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        cardForm.Controls.AddRange(new Control[]
        {
            txtNome, txtCpf, txtTelefone, txtEmail, txtEndereco,
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
            grid.DataSource = ListarClientes(txtPesquisa.Text.Trim());

            // Ajustes só se a coluna existir
            if (grid.Columns["Id"] is { } colId)
            {
                colId.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // desliga o auto primeiro
                colId.Width = 50;                                          // depois define a largura
            }
            if (grid.Columns["Cpf"] is { } colCpf) colCpf.HeaderText = "CPF";
            if (grid.Columns["Endereco"] is { } colEnd) colEnd.HeaderText = "Endereço";
            if (grid.Columns["Email"] is { } colEmail) colEmail.HeaderText = "E-mail";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao carregar clientes: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (grid.CurrentRow?.DataBoundItem is not Cliente c) return;

        clienteAtual = c;
        txtNome.Text = c.Nome;
        txtCpf.Text = c.Cpf;
        txtTelefone.Text = c.Telefone;
        txtEmail.Text = c.Email;
        txtEndereco.Text = c.Endereco;
        btnExcluir.Enabled = true;
    }

    // ---------- AÇÕES ----------
    private void LimparCampos()
    {
        clienteAtual = null;
        txtNome.Clear();
        txtCpf.Clear();
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
            Aviso("Informe o nome do cliente.", txtNome);
            return false;
        }
        if (!txtCpf.MaskCompleted)
        {
            Aviso("Informe o CPF completo.", txtCpf);
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
            var c = clienteAtual ?? new Cliente();
            c.Nome = txtNome.Text.Trim();
            c.Cpf = txtCpf.Text;
            c.Telefone = txtTelefone.Text;
            c.Email = txtEmail.Text.Trim();
            c.Endereco = txtEndereco.Text.Trim();

            SalvarCliente(c);

            MessageBox.Show("Cliente salvo com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        if (clienteAtual == null) return;

        var resp = MessageBox.Show($"Excluir o cliente '{clienteAtual.Nome}'?", "Confirmação",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (resp != DialogResult.Yes) return;

        try
        {
            ExcluirCliente(clienteAtual.Id);
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
    // Quando o DAO ficar pronto, só estes 3 métodos precisam ser trocados:
    //   ListarClientes(filtro)  -> new ClienteDAO().Listar(filtro)
    //   SalvarCliente(c)        -> new ClienteDAO().Salvar(c)  (insert ou update)
    //   ExcluirCliente(id)      -> new ClienteDAO().Excluir(id)
    // =====================================================================
    private static readonly List<Cliente> bancoFake = new();
    private static int proximoId = 1;

    private static List<Cliente> ListarClientes(string filtro)
        => bancoFake
            .Where(c => string.IsNullOrEmpty(filtro)
                     || c.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase)
                     || c.Cpf.Contains(filtro))
            .ToList();

    private static void SalvarCliente(Cliente c)
    {
        if (c.Id == 0)
        {
            c.Id = proximoId++;
            bancoFake.Add(c);
        }
        // se já tem Id, o objeto da lista já foi alterado (mesma referência)
    }

    private static void ExcluirCliente(int id) => bancoFake.RemoveAll(c => c.Id == id);
}