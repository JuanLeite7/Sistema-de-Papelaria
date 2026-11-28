
using System;
using MySqlConnector;

namespace Papelaria.Conexao
{
    public class ConexaoMySQL
    {
        private static string connectionString =
       "Server=localhost;Port=3306;Database=papelaria_db;User ID=root;Password= SUA SENHA;";

        public static MySqlConnection ObterConexao()
        {
            return new MySqlConnection(connectionString);
        }

        public static string TestarConexao()
        {
            try
            {
                using (MySqlConnection conexao = ObterConexao())
                {
                    conexao.Open();
                    return "Conexão com o banco realizada com sucesso!";
                }
            }
            catch (Exception ex)
            {
                return "Erro ao conectar: " + ex.Message;
            }
        }
    }
}
