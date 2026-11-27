
namespace Papelaria.Model
{
    public class PedidoCompra
    {
        public int Id { get; set; }

        public int FornecedorId { get; set; }

        public int UsuarioId { get; set; }

        public DateTime DataPedido { get; set; } = DateTime.Now;

        public string Status { get; set; } = "PENDENTE";

        public decimal ValorTotal { get; set; }

        public List<ItemPedidoCompra> Itens { get; set; } = new List<ItemPedidoCompra>();
    }
}
