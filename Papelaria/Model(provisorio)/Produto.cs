namespace Papelaria.Model;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public int CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = ""; // só pra exibir no grid
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
    public int EstoqueMinimo { get; set; }
}