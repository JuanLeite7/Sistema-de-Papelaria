
using MySqlConnector;
using Papelaria.Model;

namespace Papelaria.DAO
{
    public class PromocaoDAO
    {
        private readonly string _connectionString;

        public PromocaoDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Cadastra uma nova promoção
        public void Inserir(Promocao promocao)
        {
            ValidarPromocao(promocao);

            using (MySqlConnection conexao = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO Promocoes
                    (nome, descricao, percentual_desconto,
                     data_inicio, data_fim, status)
                    VALUES
                    (@nome, @descricao, @desconto,
                     @inicio, @fim, @status);";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.AddWithValue("@nome", promocao.Nome);
                    comando.Parameters.AddWithValue("@descricao", promocao.Descricao);
                    comando.Parameters.AddWithValue("@desconto", promocao.PercentualDesconto);
                    comando.Parameters.AddWithValue("@inicio", promocao.DataInicio);
                    comando.Parameters.AddWithValue("@fim", promocao.DataFim);
                    comando.Parameters.AddWithValue("@status", promocao.Status);

                    conexao.Open();
                    comando.ExecuteNonQuery();
                }
            }
        }

        // Atualiza uma promoção existente
        public void Atualizar(Promocao promocao)
        {
            ValidarPromocao(promocao);

            using (MySqlConnection conexao = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE Promocoes
                    SET nome = @nome,
                        descricao = @descricao,
                        percentual_desconto = @desconto,
                        data_inicio = @inicio,
                        data_fim = @fim,
                        status = @status
                    WHERE id_promocao = @id;";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.AddWithValue("@id", promocao.Id);
                    comando.Parameters.AddWithValue("@nome", promocao.Nome);
                    comando.Parameters.AddWithValue("@descricao", promocao.Descricao);
                    comando.Parameters.AddWithValue("@desconto", promocao.PercentualDesconto);
                    comando.Parameters.AddWithValue("@inicio", promocao.DataInicio);
                    comando.Parameters.AddWithValue("@fim", promocao.DataFim);
                    comando.Parameters.AddWithValue("@status", promocao.Status);

                    conexao.Open();

                    int linhasAfetadas = comando.ExecuteNonQuery();

                    if (linhasAfetadas == 0)
                    {
                        using (MySqlCommand verificar = new MySqlCommand(
                            "SELECT COUNT(*) FROM Promocoes WHERE id_promocao = @id;",
                            conexao))
                        {
                            verificar.Parameters.AddWithValue("@id", promocao.Id);

                            if (Convert.ToInt32(verificar.ExecuteScalar()) == 0)
                                throw new Exception("Promoção não encontrada.");
                        }
                    }
                }
            }
        }

        // Desativa uma promoção
        public void Excluir(int id)
        {
            using (MySqlConnection conexao = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE Promocoes
                    SET status = 0
                    WHERE id_promocao = @id;";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.AddWithValue("@id", id);

                    conexao.Open();

                    int linhasAfetadas = comando.ExecuteNonQuery();

                    if (linhasAfetadas == 0)
                    {
                        using (MySqlCommand verificar = new MySqlCommand(
                            "SELECT COUNT(*) FROM Promocoes WHERE id_promocao = @id;",
                            conexao))
                        {
                            verificar.Parameters.AddWithValue("@id", id);

                            if (Convert.ToInt32(verificar.ExecuteScalar()) == 0)
                                throw new Exception("Promoção não encontrada.");
                        }
                    }
                }
            }
        }

        // Lista todas as promoções
        public List<Promocao> ListarTodos()
        {
            List<Promocao> promocoes = new List<Promocao>();

            using (MySqlConnection conexao = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT id_promocao, nome, descricao,
                           percentual_desconto, data_inicio,
                           data_fim, status
                    FROM Promocoes
                    ORDER BY data_inicio DESC;";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    conexao.Open();

                    using (MySqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            promocoes.Add(new Promocao
                            {
                                Id = Convert.ToInt32(reader["id_promocao"]),
                                Nome = reader["nome"].ToString() ?? "",
                                Descricao = reader["descricao"].ToString() ?? "",
                                PercentualDesconto = Convert.ToDecimal(reader["percentual_desconto"]),
                                DataInicio = Convert.ToDateTime(reader["data_inicio"]),
                                DataFim = Convert.ToDateTime(reader["data_fim"]),
                                Status = Convert.ToBoolean(reader["status"])
                            });
                        }
                    }
                }
            }

            return promocoes;
        }

        // Busca uma promoção pelo código
        public Promocao? ObterPorId(int id)
        {
            using (MySqlConnection conexao = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT id_promocao, nome, descricao,
                           percentual_desconto, data_inicio,
                           data_fim, status
                    FROM Promocoes
                    WHERE id_promocao = @id;";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.AddWithValue("@id", id);

                    conexao.Open();

                    using (MySqlDataReader reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Promocao
                            {
                                Id = Convert.ToInt32(reader["id_promocao"]),
                                Nome = reader["nome"].ToString() ?? "",
                                Descricao = reader["descricao"].ToString() ?? "",
                                PercentualDesconto = Convert.ToDecimal(reader["percentual_desconto"]),
                                DataInicio = Convert.ToDateTime(reader["data_inicio"]),
                                DataFim = Convert.ToDateTime(reader["data_fim"]),
                                Status = Convert.ToBoolean(reader["status"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        // Lista somente as promoções ativas
        public List<Promocao> ListarAtivas()
        {
            return ListarTodos()
                .Where(p => p.Status)
                .ToList();
        }

        // Valida os dados da promoção
        private void ValidarPromocao(Promocao promocao)
        {
            if (string.IsNullOrWhiteSpace(promocao.Nome))
                throw new Exception("Informe o nome da promoção.");

            if (promocao.PercentualDesconto <= 0 ||
                promocao.PercentualDesconto > 100)
            {
                throw new Exception("O desconto deve ser maior que 0 e menor ou igual a 100.");
            }

            if (promocao.DataFim < promocao.DataInicio)
                throw new Exception("A data final não pode ser anterior à data inicial.");
        }
    }
}
