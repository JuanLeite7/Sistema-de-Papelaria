using Papelaria.DAO;
using Papelaria.Model;
using Papelaria.Util;

namespace Papelaria.Forms;

public class FrmLogin : Form
{
    private readonly TextBox txtUsuario = new() { PlaceholderText = "Digite seu usuário" };
    private readonly TextBox txtSenha = new() { PlaceholderText = "Digite sua senha", UseSystemPasswordChar = true };
    private readonly Button btnEntrar = Tema.Botao("Entrar", Tema.Primaria, 50, 275, 360, 44);

    public FrmLogin()
    {
        Text = "Papelaria - Login";
        ClientSize = new Size(780, 450);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.White;
        Font = Tema.Fonte;
        AcceptButton = btnEntrar; // Enter aciona o botão

        // ----- Lado direito: formulário (adicionado primeiro = Fill) -----
        var direita = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        direita.Controls.Add(new Label
        {
            Text = "Bem-vindo de volta",
            Font = Tema.FonteTitulo,
            ForeColor = Tema.Texto,
            Location = new Point(50, 50),
            AutoSize = true
        });
        direita.Controls.Add(new Label
        {
            Text = "Entre com suas credenciais para continuar",
            ForeColor = Tema.TextoSuave,
            Location = new Point(52, 90),
            AutoSize = true
        });
        direita.Controls.Add(Tema.Rotulo("Usuário", 50, 128));
        direita.Controls.Add(Tema.Rotulo("Senha", 50, 193));

        Tema.Campo(txtUsuario, 50, 150, 360).Font = new Font("Segoe UI", 12F);
        Tema.Campo(txtSenha, 50, 215, 360).Font = new Font("Segoe UI", 12F);
        direita.Controls.AddRange(new Control[] { txtUsuario, txtSenha, btnEntrar });

        // ----- Lado esquerdo: identidade visual -----
        var esquerda = new Panel { Dock = DockStyle.Left, Width = 320, BackColor = Tema.Primaria };
        esquerda.Controls.Add(new Label
        {
            Text = "Papelaria",
            Font = new Font("Segoe UI", 28F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(35, 150),
            AutoSize = true
        });
        esquerda.Controls.Add(new Label
        {
            Text = "Sistema de Gerenciamento",
            Font = new Font("Segoe UI", 11F),
            ForeColor = Color.FromArgb(199, 210, 254),
            Location = new Point(40, 210),
            AutoSize = true
        });

        Controls.Add(direita);   // Fill primeiro
        Controls.Add(esquerda);  // Left depois

        btnEntrar.Click += BtnEntrar_Click;
    }

  private void BtnEntrar_Click(object? sender, EventArgs e)
{
    if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtSenha.Text))
    {
        MessageBox.Show("Informe usuário e senha.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    Usuario? usuario;

    try
    {
        var dao = new UsuarioDAO(Conexao.ConnectionString);
        usuario = dao.Autenticar(txtUsuario.Text.Trim(), txtSenha.Text);
    }
    catch (Exception ex)
    {
        MessageBox.Show("Erro ao conectar com o banco:\n" + ex.Message,
            "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
    }

    if (usuario == null)
    {
        MessageBox.Show("Usuário ou senha inválidos.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
    }

    Hide();
    var principal = new FrmPrincipal(usuario.Nome); // ou passe o objeto usuario inteiro
    principal.FormClosed += (_, _) => Close();
    principal.Show();
}
}
