using MySqlConnector;
using Papelaria.Model;

namespace Papelaria.DAO
{
    public class EstoqueDAO
    {
        private readonly string _connectionString;

        public EstoqueDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        // CONSULTAR QUANTIDADE EM ESTOQUE
        public int? ConsultarEstoque(int produtoId)
        {
            string sql = @"
                SELECT quantidade_atual
                FROM Estoque
                WHERE id_produto = @produtoId";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@produtoId", MySqlDbType.Int32)
                        .Value = produtoId;

                    conexao.Open();

                    object resultado = comando.ExecuteScalar();

                    if (resultado == null || resultado == DBNull.Value)
                        return null;

                    return Convert.ToInt32(resultado);
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(
                    "Erro ao consultar estoque: " + ex.Message, ex);
            }
        }

        // REGISTRAR ENTRADA NO ESTOQUE
        public void RegistrarEntrada(int produtoId, int quantidade)
        {
            if (quantidade <= 0)
                throw new Exception("A quantidade deve ser maior que zero.");

            string sql = @"
                UPDATE Estoque
                SET quantidade_atual = quantidade_atual + @quantidade
                WHERE id_produto = @produtoId";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@quantidade", MySqlDbType.Int32)
                        .Value = quantidade;

                    comando.Parameters.Add("@produtoId", MySqlDbType.Int32)
                        .Value = produtoId;

                    conexao.Open();

                    int linhasAfetadas = comando.ExecuteNonQuery();

                    if (linhasAfetadas == 0)
                        throw new Exception(
                            "Produto não encontrado no estoque.");
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(
                    "Erro ao registrar entrada no estoque: " + ex.Message, ex);
            }
        }

        // REGISTRAR SAÍDA NO ESTOQUE
        public void RegistrarSaida(int produtoId, int quantidade)
        {
            if (quantidade <= 0)
                throw new Exception("A quantidade deve ser maior que zero.");

            string sql = @"
                UPDATE Estoque
                SET quantidade_atual = quantidade_atual - @quantidade
                WHERE id_produto = @produtoId
                AND quantidade_atual >= @quantidade";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@quantidade", MySqlDbType.Int32)
                        .Value = quantidade;

                    comando.Parameters.Add("@produtoId", MySqlDbType.Int32)
                        .Value = produtoId;

                    conexao.Open();

                    int linhasAfetadas = comando.ExecuteNonQuery();

                    if (linhasAfetadas == 0)
                    {
                        using (MySqlCommand verificar = new MySqlCommand(
                            "SELECT COUNT(*) FROM Estoque WHERE id_produto = @id",
                            conexao))
                        {
                            verificar.Parameters.Add("@id", MySqlDbType.Int32)
                                .Value = produtoId;

                            if (Convert.ToInt32(verificar.ExecuteScalar()) == 0)
                                throw new Exception(
                                    "Produto não encontrado no estoque.");
                        }

                        throw new Exception(
                            "Estoque insuficiente para realizar a saída.");
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(
                    "Erro ao registrar saída no estoque: " + ex.Message, ex);
            }
        }

        // ATUALIZAR QUANTIDADE MÍNIMA
        public void AtualizarQuantidadeMinima(int produtoId, int quantidadeMinima)
        {
            if (quantidadeMinima < 0)
                throw new Exception(
                    "A quantidade mínima não pode ser negativa.");

            string sql = @"
                UPDATE Estoque
                SET quantidade_minima = @quantidadeMinima
                WHERE id_produto = @produtoId";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@quantidadeMinima", MySqlDbType.Int32)
                        .Value = quantidadeMinima;

                    comando.Parameters.Add("@produtoId", MySqlDbType.Int32)
                        .Value = produtoId;

                    conexao.Open();

                    int linhasAfetadas = comando.ExecuteNonQuery();

                    if (linhasAfetadas == 0)
                    {
                        using (MySqlCommand verificar = new MySqlCommand(
                            "SELECT COUNT(*) FROM Estoque WHERE id_produto = @id",
                            conexao))
                        {
                            verificar.Parameters.Add("@id", MySqlDbType.Int32)
                                .Value = produtoId;

                            if (Convert.ToInt32(verificar.ExecuteScalar()) == 0)
                                throw new Exception(
                                    "Produto não encontrado no estoque.");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(
                    "Erro ao atualizar quantidade mínima: " + ex.Message, ex);
            }
        }

        // LISTAR PRODUTOS COM ESTOQUE BAIXO
        public List<Produto> ListarEstoqueBaixo()
        {
            List<Produto> produtos = new List<Produto>();

            string sql = @"
                SELECT
                    p.id_produto,
                    p.nome,
                    p.id_categoria,
                    c.nome AS categoria_nome,
                    p.preco_venda,
                    e.quantidade_atual,
                    e.quantidade_minima
                FROM Produtos p
                INNER JOIN Categorias c
                    ON p.id_categoria = c.id_categoria
                INNER JOIN Estoque e
                    ON p.id_produto = e.id_produto
                WHERE e.quantidade_atual <= e.quantidade_minima
                AND p.status = TRUE
                ORDER BY e.quantidade_atual ASC";

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
                            produtos.Add(new Produto
                            {
                                Id = reader.GetInt32("id_produto"),
                                Nome = reader.GetString("nome"),
                                CategoriaId = reader.GetInt32("id_categoria"),
                                CategoriaNome = reader.GetString("categoria_nome"),
                                Preco = reader.GetDecimal("preco_venda"),
                                Estoque = reader.GetInt32("quantidade_atual"),
                                EstoqueMinimo = reader.GetInt32("quantidade_minima")
                            });
                        }
                    }
                }

                return produtos;
            }
            catch (MySqlException ex)
            {
                throw new Exception(
                    "Erro ao listar estoque baixo: " + ex.Message, ex);
            }
        }

        // LISTAR TODOS OS PRODUTOS E SEUS ESTOQUES
        public List<Produto> ListarTodos()
        {
            List<Produto> produtos = new List<Produto>();

            string sql = @"
                SELECT
                    p.id_produto,
                    p.nome,
                    p.id_categoria,
                    c.nome AS categoria_nome,
                    p.preco_venda,
                    e.quantidade_atual,
                    e.quantidade_minima
                FROM Produtos p
                INNER JOIN Categorias c
                    ON p.id_categoria = c.id_categoria
                INNER JOIN Estoque e
                    ON p.id_produto = e.id_produto
                ORDER BY p.nome";

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
                            produtos.Add(new Produto
                            {
                                Id = reader.GetInt32("id_produto"),
                                Nome = reader.GetString("nome"),
                                CategoriaId = reader.GetInt32("id_categoria"),
                                CategoriaNome = reader.GetString("categoria_nome"),
                                Preco = reader.GetDecimal("preco_venda"),
                                Estoque = reader.GetInt32("quantidade_atual"),
                                EstoqueMinimo = reader.GetInt32("quantidade_minima")
                            });
                        }
                    }
                }

                return produtos;
            }
            catch (MySqlException ex)
            {
                throw new Exception(
                    "Erro ao listar estoque: " + ex.Message, ex);
            }
        }
    }
}
