using System.Drawing;
using Papelaria.Model;
using Papelaria.Util;

namespace Papelaria.Forms;

public class FrmUsuarios : Form
{
    // ---------- CONTROLES ----------
    private readonly TextBox txtPesquisa = new() { PlaceholderText = "Pesquisar por nome ou login..." };
    private readonly DataGridView grid = new();

    private readonly TextBox txtNome = new() { MaxLength = 100 };
    private readonly TextBox txtLogin = new() { MaxLength = 50 };
    private readonly ComboBox cmbPerfil = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox txtSenha = new() { MaxLength = 50, UseSystemPasswordChar = true };
    private readonly TextBox txtConfirmar = new() { MaxLength = 50, UseSystemPasswordChar = true };
    private readonly CheckBox chkAtivo = new() { Text = "Usuário ativo", Checked = true, AutoSize = true };

    private readonly Button btnNovo = Tema.Botao("Novo", Tema.Primaria, 15, 165);
    private readonly Button btnSalvar = Tema.Botao("Salvar", Tema.Sucesso, 135, 165);
    private readonly Button btnExcluir = Tema.Botao("Excluir", Tema.Perigo, 255, 165);
    private readonly Button btnFechar = Tema.Botao("Fechar", Tema.Neutro, 735, 165);

    private Usuario? usuarioAtual; // null = cadastrando um novo

    public FrmUsuarios()
    {
        Text = "Cadastro de Usuários";
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
        Controls.Add(Tema.Cabecalho("Usuários", "Controle de acesso ao sistema"));

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

        cmbPerfil.Items.AddRange(new object[] { "Administrador", "Vendedor" });

        cardForm.Controls.Add(Tema.Rotulo("Nome *", 15, 15));
        cardForm.Controls.Add(Tema.Rotulo("Login *", 410, 15));
        cardForm.Controls.Add(Tema.Rotulo("Perfil *", 625, 15));
        cardForm.Controls.Add(Tema.Rotulo("Senha *", 15, 78));
        cardForm.Controls.Add(Tema.Rotulo("Confirmar senha *", 215, 78));
        cardForm.Controls.Add(Tema.Rotulo("* obrigatórios. Ao editar, deixe a senha em branco para mantê-la.", 15, 140));

        Tema.Campo(txtNome, 15, 38, 380);
        Tema.Campo(txtLogin, 410, 38, 200);
        Tema.Posicionar(cmbPerfil, 625, 38, 220);
        Tema.Campo(txtSenha, 15, 101, 185);
        Tema.Campo(txtConfirmar, 215, 101, 180);
        chkAtivo.Font = Tema.Fonte;
        chkAtivo.Location = new Point(410, 103);

        btnFechar.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        cardForm.Controls.AddRange(new Control[]
        {
            txtNome, txtLogin, cmbPerfil, txtSenha, txtConfirmar, chkAtivo,
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
            grid.DataSource = ListarUsuarios(txtPesquisa.Text.Trim());

            // Ajustes só se a coluna existir
            if (grid.Columns["Id"] is { } colId)
            {
                colId.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colId.Width = 50;
            }
            if (grid.Columns["Senha"] is { } colSenha) colSenha.Visible = false; // nunca mostrar senha
            if (grid.Columns["Ativo"] is { } colAtivo)
            {
                colAtivo.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colAtivo.Width = 70;
            }

            grid.ClearSelection(); // não preenche o formulário sozinho
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao carregar usuários: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (grid.SelectedRows.Count == 0 || grid.SelectedRows[0].DataBoundItem is not Usuario u) return;

        usuarioAtual = u;
        txtNome.Text = u.Nome;
        txtLogin.Text = u.Login;
        cmbPerfil.SelectedItem = u.Perfil;
        chkAtivo.Checked = u.Ativo;
        txtSenha.Clear();      // senha nunca é carregada de volta na tela
        txtConfirmar.Clear();
        btnExcluir.Enabled = true;
    }

    // ---------- AÇÕES ----------
    private void LimparCampos()
    {
        usuarioAtual = null;
        txtNome.Clear();
        txtLogin.Clear();
        cmbPerfil.SelectedIndex = -1;
        txtSenha.Clear();
        txtConfirmar.Clear();
        chkAtivo.Checked = true;
        grid.ClearSelection();
        btnExcluir.Enabled = false;
        txtNome.Focus();
    }

    private bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(txtNome.Text))
        {
            Aviso("Informe o nome do usuário.", txtNome);
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtLogin.Text))
        {
            Aviso("Informe o login.", txtLogin);
            return false;
        }
        if (cmbPerfil.SelectedItem is null)
        {
            Aviso("Selecione o perfil.", cmbPerfil);
            return false;
        }

        // Senha obrigatória só no cadastro; na edição, em branco = manter a atual
        bool senhaObrigatoria = usuarioAtual == null;
        if (senhaObrigatoria && string.IsNullOrEmpty(txtSenha.Text))
        {
            Aviso("Informe a senha.", txtSenha);
            return false;
        }
        if (!string.IsNullOrEmpty(txtSenha.Text))
        {
            if (txtSenha.Text.Length < 4)
            {
                Aviso("A senha deve ter pelo menos 4 caracteres.", txtSenha);
                return false;
            }
            if (txtSenha.Text != txtConfirmar.Text)
            {
                Aviso("A confirmação não confere com a senha.", txtConfirmar);
                return false;
            }
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
            var u = usuarioAtual ?? new Usuario();
            u.Nome = txtNome.Text.Trim();
            u.Login = txtLogin.Text.Trim();
            u.Perfil = (string)cmbPerfil.SelectedItem!;
            u.Ativo = chkAtivo.Checked;
            if (!string.IsNullOrEmpty(txtSenha.Text)) u.Senha = txtSenha.Text; // só troca se digitou

            SalvarUsuario(u);

            MessageBox.Show("Usuário salvo com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        if (usuarioAtual == null) return;

        var resp = MessageBox.Show($"Excluir o usuário '{usuarioAtual.Nome}'?", "Confirmação",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (resp != DialogResult.Yes) return;

        try
        {
            ExcluirUsuario(usuarioAtual.Id);
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
    //   ListarUsuarios(filtro) -> new UsuarioDAO().Listar(filtro)
    //   SalvarUsuario(u)       -> new UsuarioDAO().Salvar(u)  (insert ou update, senha com hash)
    //   ExcluirUsuario(id)     -> new UsuarioDAO().Excluir(id)
    // Login repetido: no banco, o ideal é um UNIQUE na coluna login.
    // =====================================================================
    private static readonly List<Usuario> bancoFake = new();
    private static int proximoId = 1;

    private static List<Usuario> ListarUsuarios(string filtro)
        => bancoFake
            .Where(u => string.IsNullOrEmpty(filtro)
                     || u.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase)
                     || u.Login.Contains(filtro, StringComparison.OrdinalIgnoreCase))
            .ToList();

    private static void SalvarUsuario(Usuario u)
    {
        if (bancoFake.Any(x => x.Id != u.Id && x.Login.Equals(u.Login, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Já existe um usuário com este login.");

        if (u.Id == 0)
        {
            u.Id = proximoId++;
            bancoFake.Add(u);
        }
        // se já tem Id, o objeto da lista já foi alterado (mesma referência)
    }

    private static void ExcluirUsuario(int id) => bancoFake.RemoveAll(u => u.Id == id);
}