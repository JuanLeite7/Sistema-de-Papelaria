namespace Papelaria.Model;

public class MovimentacaoEstoque
{
    public int Id { get; set; }
    public DateTime Data { get; set; } = DateTime.Now;
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = "";   // só pra exibir no grid
    public string Tipo { get; set; } = "Entrada";   // "Entrada" ou "Saída"
    public int Quantidade { get; set; }
    public int EstoqueApos { get; set; }            // saldo depois da movimentação
    public string Motivo { get; set; } = "";
}