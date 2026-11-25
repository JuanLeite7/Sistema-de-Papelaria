namespace Papelaria.Model;

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Descricao { get; set; } = "";

    // Faz o ComboBox mostrar o nome da categoria
    public override string ToString() => Nome;
}