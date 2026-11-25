using System.Drawing;
using Papelaria.Model;
using Papelaria.Util;

namespace Papelaria.Forms;

public class FrmEstoqueBaixo : Form
{
    // ---------- CONTROLES ----------
    private readonly TextBox txtPesquisa = new() { PlaceholderText = "Pesquisar por nome..." };
    private readonly ComboBox cmbCategoria = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label lblResumo = new() { AutoSize = true, Font = Tema.FonteNegrito };
    private readonly DataGridView grid = new();

    private readonly Button btnEntrada = Tema.Botao("Registrar entrada", Tema.Sucesso, 20, 560, 170);
    private readonly Button btnFechar = Tema.Botao("Fechar", Tema.Neutro, 770, 560);

    public FrmEstoqueBaixo()
    {
        Text = "Produtos com Estoque Baixo";
        Font = Tema.Fonte;
        BackColor = Tema.Fundo;
        ClientSize = new Size(900, 620);
        MinimumSize = new Size(916, 660);
        StartPosition = FormStartPosition.CenterParent;

        CarregarCategorias(); // antes de ligar os eventos
        MontarTela();
        CarregarGrid();
    }

    // ---------- CATEGORIAS (filtro) ----------
    private void CarregarCategorias()
    {
        cmbCategoria.Items.Add(new Categoria { Id = 0, Nome = "Todas as categorias" });
        cmbCategoria.Items.AddRange(ListarCategorias().ToArray());
        cmbCategoria.SelectedIndex = 0;
    }

    // ---------- MONTAGEM DA TELA ----------
    private void MontarTela()
    {
        Controls.Add(Tema.Cabecalho("Estoque Baixo", "Produtos que precisam de reposição"));

        var cardLista = Tema.Cartao(20, 92, 860, 450);
        cardLista.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        Tema.Campo(txtPesquisa, 15, 15, 300);
        Tema.Posicionar(cmbCategoria, 330, 15, 240);
        lblResumo.Location = new Point(590, 19);
        lblResumo.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        Tema.EstilizarGrid(grid);
        grid.Location = new Point(15, 55);
        grid.Size = new Size(830, 380);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        cardLista.Controls.AddRange(new Control[] { txtPesquisa, cmbCategoria, lblResumo, grid });
        Controls.Add(cardLista);

        btnEntrada.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnFechar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        Controls.Add(btnEntrada);
        Controls.Add(btnFechar);

        // Eventos
        txtPesquisa.TextChanged += (_, _) => CarregarGrid();
        cmbCategoria.SelectedIndexChanged += (_, _) => CarregarGrid();
        grid.CellFormatting += Grid_CellFormatting;
        btnEntrada.Click += BtnEntrada_Click;
        btnFechar.Click += (_, _) => Close();
    }

    // ---------- GRID ----------
    private void CarregarGrid()
    {
        try
        {
            int categoriaId = cmbCategoria.SelectedItem is Categoria cat ? cat.Id : 0;
            var lista = ListarEstoqueBaixo(txtPesquisa.Text.Trim(), categoriaId);

            grid.AutoGenerateColumns = true;
            grid.DataSource = null;
            grid.DataSource = lista;

            // Ajustes só se a coluna existir
            if (grid.Columns["ProdutoId"] is { } colId) colId.Visible = false;
            if (grid.Columns["Minimo"] is { } colMin) colMin.HeaderText = "Estoque mín.";
            if (grid.Columns["Situacao"] is { } colSit) colSit.HeaderText = "Situação";

            grid.ClearSelection();

            if (lista.Count == 0)
            {
                lblResumo.Text = "Nenhum produto com estoque baixo";
                lblResumo.ForeColor = Tema.Sucesso;
            }
            else
            {
                lblResumo.Text = $"{lista.Count} produto(s) precisam de reposição";
                lblResumo.ForeColor = Tema.Perigo;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao carregar a consulta: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // "Sem estoque" em vermelho, "Baixo" em laranja
    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Situacao") return;

        e.CellStyle.ForeColor = e.Value as string == "Sem estoque" ? Tema.Perigo : Tema.Alerta;
        e.CellStyle.Font = Tema.FonteNegrito;
    }

    // ---------- AÇÕES ----------
    private void BtnEntrada_Click(object? sender, EventArgs e)
    {
        using var tela = new FrmMovimentacaoEstoque();
        tela.ShowDialog(this);
        CarregarGrid(); // o estoque pode ter mudado
    }

    // =====================================================================
    // ACESSO A DADOS PROVISÓRIO (em memória)
    // Quando o DAO ficar pronto, trocar estes métodos:
    //   ListarCategorias()                       -> new CategoriaDAO().Listar("")
    //   ListarEstoqueBaixo(filtro, categoriaId)  -> new ProdutoDAO().ListarEstoqueBaixo(filtro, categoriaId)
    //     (no banco: WHERE estoque <= estoque_minimo)
    // =====================================================================
    private static List<Categoria> ListarCategorias() => FrmCategorias.ListarCategorias("");

    private static List<ItemEstoqueBaixo> ListarEstoqueBaixo(string filtro, int categoriaId)
        => FrmProdutos.ListarProdutos(filtro, categoriaId)
            .Where(p => p.Estoque <= p.EstoqueMinimo)
            .OrderBy(p => p.Estoque)
            .Select(p => new ItemEstoqueBaixo
            {
                ProdutoId = p.Id,
                Produto = p.Nome,
                Categoria = p.CategoriaNome,
                Estoque = p.Estoque,
                Minimo = p.EstoqueMinimo,
                Situacao = p.Estoque == 0 ? "Sem estoque" : "Baixo"
            })
            .ToList();
}

/// <summary>Linha da consulta (só pra exibir no grid).</summary>
public sealed class ItemEstoqueBaixo
{
    public int ProdutoId { get; set; }
    public string Produto { get; set; } = "";
    public string Categoria { get; set; } = "";
    public int Estoque { get; set; }
    public int Minimo { get; set; }
    public string Situacao { get; set; } = "";
}