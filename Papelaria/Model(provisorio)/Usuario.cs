namespace Papelaria.Model;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Login { get; set; } = "";
    public string Senha { get; set; } = "";   // o DAO deve gravar com hash, nunca em texto puro
    public string Perfil { get; set; } = "Vendedor"; // "Administrador" ou "Vendedor"
    public bool Ativo { get; set; } = true;
}