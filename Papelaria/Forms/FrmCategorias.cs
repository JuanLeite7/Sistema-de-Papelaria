
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Papelaria.Model;
using Papelaria.Util;
using Papelaria.DAO;
using Papelaria.Conexao;

namespace Papelaria.Forms
{
    public class FrmCategorias : Form
    {
        // CONTROLES
        private readonly TextBox txtPesquisa =
            new() { PlaceholderText = "Pesquisar por nome..." };

        private readonly DataGridView grid = new();

        private readonly TextBox txtNome = new() { MaxLength = 100 };
        private readonly TextBox txtDescricao = new() { MaxLength = 200 };

        private readonly Button btnNovo =
            Tema.Botao("Novo", Tema.Primaria, 15, 100);

        private readonly Button btnSalvar =
            Tema.Botao("Salvar", Tema.Sucesso, 135, 100);

        private readonly Button btnExcluir =
            Tema.Botao("Excluir", Tema.Perigo, 255, 100);

        private readonly Button btnFechar =
            Tema.Botao("Fechar", Tema.Neutro, 735, 100);

        private Categoria? categoriaAtual;

        // CONSTRUTOR
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

        // CRIAR DAO
        private static CategoriaDAO CriarDAO()
        {
            using (var conexao = ConexaoMySQL.ObterConexao())
            {
                string connectionString = conexao.ConnectionString;

                return new CategoriaDAO(connectionString);
            }
        }

        // MONTAGEM DA TELA
        private void MontarTela()
        {
            Controls.Add(
                Tema.Cabecalho(
                    "Categorias",
                    "Organize os produtos por categoria"
                )
            );

            var cardLista = Tema.Cartao(20, 92, 860, 320);

            cardLista.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right |
                AnchorStyles.Bottom;

            Tema.Campo(txtPesquisa, 15, 15, 340);

            Tema.EstilizarGrid(grid);

            grid.Location = new Point(15, 55);
            grid.Size = new Size(830, 250);

            grid.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right |
                AnchorStyles.Bottom;

            cardLista.Controls.Add(txtPesquisa);
            cardLista.Controls.Add(grid);

            Controls.Add(cardLista);

            var cardForm = Tema.Cartao(20, 425, 860, 150);

            cardForm.Anchor =
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            cardForm.Controls.Add(Tema.Rotulo("Nome *", 15, 15));
            cardForm.Controls.Add(Tema.Rotulo("Descrição", 410, 15));
            cardForm.Controls.Add(
                Tema.Rotulo("* campos obrigatórios", 15, 78)
            );

            Tema.Campo(txtNome, 15, 38, 380);
            Tema.Campo(txtDescricao, 410, 38, 435);

            btnFechar.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            cardForm.Controls.AddRange(new Control[]
            {
                txtNome,
                txtDescricao,
                btnNovo,
                btnSalvar,
                btnExcluir,
                btnFechar
            });

            Controls.Add(cardForm);

            // EVENTOS
            txtPesquisa.TextChanged += (_, _) => CarregarGrid();

            grid.SelectionChanged += Grid_SelectionChanged;

            btnNovo.Click += (_, _) => LimparCampos();

            btnSalvar.Click += BtnSalvar_Click;

            btnExcluir.Click += BtnExcluir_Click;

            btnFechar.Click += (_, _) => Close();
        }

        // CARREGAR GRID
        private void CarregarGrid()
        {
            try
            {
                grid.AutoGenerateColumns = true;
                grid.DataSource = null;

                grid.DataSource = ListarCategorias(
                    txtPesquisa.Text.Trim()
                );

                if (grid.Columns["Id"] is { } colId)
                {
                    colId.AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.None;

                    colId.Width = 50;
                }

                if (grid.Columns["Descricao"] is { } colDesc)
                {
                    colDesc.HeaderText = "Descrição";
                }

                grid.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao carregar categorias: " + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // SELECIONAR CATEGORIA
        private void Grid_SelectionChanged(object? sender, EventArgs e)
        {
            if (
                grid.SelectedRows.Count == 0 ||
                grid.SelectedRows[0].DataBoundItem is not Categoria c
            )
            {
                return;
            }

            categoriaAtual = c;

            txtNome.Text = c.Nome;
            txtDescricao.Text = c.Descricao;

            btnExcluir.Enabled = true;
        }

        // LIMPAR CAMPOS
        private void LimparCampos()
        {
            categoriaAtual = null;

            txtNome.Clear();
            txtDescricao.Clear();

            grid.ClearSelection();

            btnExcluir.Enabled = false;

            txtNome.Focus();
        }

        // VALIDAR CAMPOS
        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show(
                    "Informe o nome da categoria.",
                    "Atenção",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtNome.Focus();

                return false;
            }

            return true;
        }

        // SALVAR OU ATUALIZAR
        private void BtnSalvar_Click(object? sender, EventArgs e)
        {
            if (!ValidarCampos())
                return;

            try
            {
                var categoria = categoriaAtual ?? new Categoria();

                categoria.Nome = txtNome.Text.Trim();

                categoria.Descricao = txtDescricao.Text.Trim();

                CategoriaDAO dao = CriarDAO();

                if (categoriaAtual == null)
                {
                    dao.Inserir(categoria);
                }
                else
                {
                    dao.Atualizar(categoria);
                }

                MessageBox.Show(
                    "Categoria salva com sucesso!",
                    "Sucesso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                CarregarGrid();

                LimparCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao salvar categoria: " + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // EXCLUIR CATEGORIA
        private void BtnExcluir_Click(object? sender, EventArgs e)
        {
            if (categoriaAtual == null)
                return;

            var resposta = MessageBox.Show(
                $"Deseja excluir a categoria '{categoriaAtual.Nome}'?",
                "Confirmação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (resposta != DialogResult.Yes)
                return;

            try
            {
                CategoriaDAO dao = CriarDAO();

                dao.Excluir(categoriaAtual.Id);

                MessageBox.Show(
                    "Categoria excluída com sucesso!",
                    "Sucesso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                CarregarGrid();

                LimparCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao excluir categoria: " + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // LISTAR CATEGORIAS NO MYSQL
        // UTILIZADO TAMBÉM PELOS FORMULÁRIOS DE PRODUTOS E ESTOQUE
        internal static List<Categoria> ListarCategorias(string filtro)
        {
            try
            {
                CategoriaDAO dao = CriarDAO();

                List<Categoria> categorias = dao.ListarTodos();

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    categorias = categorias
                        .Where(c => c.Nome.Contains(
                            filtro,
                            StringComparison.OrdinalIgnoreCase
                        ))
                        .ToList();
                }

                return categorias;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Erro ao listar categorias: " + ex.Message,
                    ex
                );
            }
        }

        // EXCLUIR CATEGORIA PELO ID
        internal static void ExcluirCategoria(int id)
        {
            CategoriaDAO dao = CriarDAO();

            dao.Excluir(id);
        }
    }
}
