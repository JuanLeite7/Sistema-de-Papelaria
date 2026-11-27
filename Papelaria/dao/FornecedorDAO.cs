using MySqlConnector;
using Papelaria.Model;

namespace Papelaria.DAO
{
    public class FornecedorDAO
    {
        private readonly string _connectionString;

        public FornecedorDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        // INSERIR FORNECEDOR
        public void Inserir(Fornecedor fornecedor)
        {
            string sql = @"
                INSERT INTO Fornecedores
                (razao_social, cnpj, telefone, email, endereco)
                VALUES
                (@razao_social, @cnpj, @telefone, @email, @endereco)";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@razao_social", MySqlDbType.VarChar)
                        .Value = fornecedor.Nome;

                    comando.Parameters.Add("@cnpj", MySqlDbType.VarChar)
                        .Value = fornecedor.Cnpj;

                    comando.Parameters.Add("@telefone", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(fornecedor.Telefone)
                            ? DBNull.Value : fornecedor.Telefone;

                    comando.Parameters.Add("@email", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(fornecedor.Email)
                            ? DBNull.Value : fornecedor.Email;

                    comando.Parameters.Add("@endereco", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(fornecedor.Endereco)
                            ? DBNull.Value : fornecedor.Endereco;

                    conexao.Open();
                    comando.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(
                    "Erro ao inserir fornecedor: " + ex.Message, ex);
            }
        }

        // ATUALIZAR FORNECEDOR
        public void Atualizar(Fornecedor fornecedor)
        {
            string sql = @"
                UPDATE Fornecedores SET
                    razao_social = @razao_social,
                    cnpj = @cnpj,
                    telefone = @telefone,
                    email = @email,
                    endereco = @endereco
                WHERE id_fornecedor = @id";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@razao_social", MySqlDbType.VarChar)
                        .Value = fornecedor.Nome;

                    comando.Parameters.Add("@cnpj", MySqlDbType.VarChar)
                        .Value = fornecedor.Cnpj;

                    comando.Parameters.Add("@telefone", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(fornecedor.Telefone)
                            ? DBNull.Value : fornecedor.Telefone;

                    comando.Parameters.Add("@email", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(fornecedor.Email)
                            ? DBNull.Value : fornecedor.Email;

                    comando.Parameters.Add("@endereco", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(fornecedor.Endereco)
                            ? DBNull.Value : fornecedor.Endereco;

                    comando.Parameters.Add("@id", MySqlDbType.Int32)
                        .Value = fornecedor.Id;

                    conexao.Open();

                    int linhasAfetadas = comando.ExecuteNonQuery();

                    if (linhasAfetadas == 0)
                    {
                        using (MySqlCommand verificar = new MySqlCommand(
                            "SELECT COUNT(*) FROM Fornecedores WHERE id_fornecedor = @id",
                            conexao))
                        {
                            verificar.Parameters.Add("@id", MySqlDbType.Int32)
                                .Value = fornecedor.Id;

                            if (Convert.ToInt32(verificar.ExecuteScalar()) == 0)
                                throw new Exception("Fornecedor não encontrado.");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(
                    "Erro ao atualizar fornecedor: " + ex.Message, ex);
            }
        }

        // EXCLUIR FORNECEDOR
        public void Excluir(int id)
        {
            string sql = @"
                DELETE FROM Fornecedores
                WHERE id_fornecedor = @id";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@id", MySqlDbType.Int32)
                        .Value = id;

                    conexao.Open();

                    int linhasAfetadas = comando.ExecuteNonQuery();

                    if (linhasAfetadas == 0)
                        throw new Exception("Fornecedor não encontrado.");
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(
                    "Erro ao excluir fornecedor. Verifique se existem pedidos de compra vinculados: "
                    + ex.Message, ex);
            }
        }

        // LISTAR TODOS OS FORNECEDORES
        public List<Fornecedor> ListarTodos()
        {
            List<Fornecedor> fornecedores = new List<Fornecedor>();

            string sql = @"
                SELECT id_fornecedor, razao_social,
                       cnpj, telefone, email, endereco
                FROM Fornecedores
                ORDER BY razao_social";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    conexao.Open();

                    using (MySqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fornecedores.Add(new Fornecedor
                            {
                                Id = reader.GetInt32("id_fornecedor"),
                                Nome = reader.GetString("razao_social"),
                                Cnpj = reader.GetString("cnpj"),

                                Telefone = reader.IsDBNull(
                                    reader.GetOrdinal("telefone"))
                                    ? "" : reader.GetString("telefone"),

                                Email = reader.IsDBNull(
                                    reader.GetOrdinal("email"))
                                    ? "" : reader.GetString("email"),

                                Endereco = reader.IsDBNull(
                                    reader.GetOrdinal("endereco"))
                                    ? "" : reader.GetString("endereco")
                            });
                        }
                    }
                }

                return fornecedores;
            }
            catch (MySqlException ex)
            {
                throw new Exception(
                    "Erro ao listar fornecedores: " + ex.Message, ex);
            }
        }

        // OBTER FORNECEDOR PELO ID
        public Fornecedor? ObterPorId(int id)
        {
            string sql = @"
                SELECT id_fornecedor, razao_social,
                       cnpj, telefone, email, endereco
                FROM Fornecedores
                WHERE id_fornecedor = @id";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@id", MySqlDbType.Int32)
                        .Value = id;

                    conexao.Open();

                    using (MySqlDataReader reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Fornecedor
                            {
                                Id = reader.GetInt32("id_fornecedor"),
                                Nome = reader.GetString("razao_social"),
                                Cnpj = reader.GetString("cnpj"),

                                Telefone = reader.IsDBNull(
                                    reader.GetOrdinal("telefone"))
                                    ? "" : reader.GetString("telefone"),

                                Email = reader.IsDBNull(
                                    reader.GetOrdinal("email"))
                                    ? "" : reader.GetString("email"),

                                Endereco = reader.IsDBNull(
                                    reader.GetOrdinal("endereco"))
                                    ? "" : reader.GetString("endereco")
                            };
                        }
                    }
                }

                return null;
            }
            catch (MySqlException ex)
            {
                throw new Exception(
                    "Erro ao buscar fornecedor: " + ex.Message, ex);
            }
        }
    }
}
