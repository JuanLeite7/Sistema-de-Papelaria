namespace Papelaria.Model;

public class Venda
{
    public int Id { get; set; }
    public DateTime Data { get; set; } = DateTime.Now;
    public int? ClienteId { get; set; }                  // null = consumidor final
    public string ClienteNome { get; set; } = "Consumidor final";
    public string FormaPagamento { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal Desconto { get; set; }
    public decimal Total { get; set; }
    public List<ItemVenda> Itens { get; set; } = new();
}