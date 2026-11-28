
namespace Papelaria.Model
{
    public class Promocao
    {
        public int Id { get; set; }

        public string Nome { get; set; } = "";

        public string Descricao { get; set; } = "";

        public decimal PercentualDesconto { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime DataFim { get; set; }

        public bool Status { get; set; } = true;
    }
}
