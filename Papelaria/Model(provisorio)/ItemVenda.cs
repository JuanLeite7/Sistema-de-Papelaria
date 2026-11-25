namespace Papelaria.Model;

public class ItemVenda
{
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = "";
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; } // preço no momento da venda

    // Calculado: não é gravado, aparece no grid
    public decimal Subtotal => Quantidade * PrecoUnitario;
}