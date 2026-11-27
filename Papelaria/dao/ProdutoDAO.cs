
using System;
using System.Collections.Generic;
using MySqlConnector;
using Papelaria.Model;

namespace Papelaria.DAO
{
    public class ProdutoDAO
    {
        private readonly string _connectionString;

        public ProdutoDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        // INSERIR PRODUTO COM ESTOQUE INICIAL
        public void Inserir(Produto produto)
        {
            string sqlProduto = @"
                INSERT INTO Produtos
                (id_categoria, nome, preco_venda, status)
                VALUES
                (@categoria, @nome, @preco, TRUE)";

            string sqlEstoque = @"
                INSERT INTO Estoque
                (id_produto, quantidade_atual, quantidade_minima)
                VALUES
                (@produto, @quantidade, @minimo)";

            using (MySqlConnection conexao =
                new MySqlConnection(_connectionString))
            {
                MySqlTransaction? transacao = null;

                try
                {
                    conexao.Open();
                    transacao = conexao.BeginTransaction();

                    using (MySqlCommand comando = new MySqlCommand(
                        sqlProduto, conexao, transacao))
                    {
                        comando.Parameters.Add("@categoria", MySqlDbType.Int32)
                            .Value = produto.CategoriaId;

                        comando.Parameters.Add("@nome", MySqlDbType.VarChar)
                            .Value = produto.Nome;

                        comando.Parameters.Add("@preco", MySqlDbType.Decimal)
                            .Value = produto.Preco;

                        comando.ExecuteNonQuery();

                        int produtoId = Convert.ToInt32(
                            comando.LastInsertedId);

                        using (MySqlCommand estoque = new MySqlCommand(
                            sqlEstoque, conexao, transacao))
                        {
                            estoque.Parameters.Add("@produto", MySqlDbType.Int32)
                                .Value = produtoId;

                            estoque.Parameters.Add("@quantidade", MySqlDbType.Int32)
                                .Value = produto.Estoque;

                            estoque.Parameters.Add("@minimo", MySqlDbType.Int32)
                                .Value = produto.EstoqueMinimo;

                            estoque.ExecuteNonQuery();
                        }

                        produto.Id = produtoId;
                    }

                    transacao.Commit();
                }
                catch (Exception ex)
                {
                    try
                    {
                        transacao?.Rollback();
                    }
                    catch (MySqlException)
                    {
                        // Preserva o erro original.
                    }

                    throw new Exception(
                        "Erro ao inserir produto: " + ex.Message, ex);
                }
                finally
                {
                    transacao?.Dispose();
                }
            }
        }

        // ATUALIZAR PRODUTO
        public void Atualizar(Produto produto)
        {
            string sqlProduto = @"
                UPDATE Produtos SET
                    id_categoria = @categoria,
                    nome = @nome,
                    preco_venda = @preco
                WHERE id_produto = @id";

            string sqlEstoque = @"
                UPDATE Estoque SET
                    quantidade_minima = @minimo
                WHERE id_produto = @id";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                {
                    conexao.Open();

                    using (MySqlCommand comando = new MySqlCommand(
                        sqlProduto, conexao))
                    {
                        comando.Parameters.Add("@categoria", MySqlDbType.Int32)
                            .Value = produto.CategoriaId;

                        comando.Parameters.Add("@nome", MySqlDbType.VarChar)
                            .Value = produto.Nome;

                        comando.Parameters.Add("@preco", MySqlDbType.Decimal)
                            .Value = produto.Preco;

                        comando.Parameters.Add("@id", MySqlDbType.Int32)
                            .Value = produto.Id;

                        comando.ExecuteNonQuery();
                    }

                    using (MySqlCommand estoque = new MySqlCommand(
                        sqlEstoque, conexao))
                    {
                        estoque.Parameters.Add("@minimo", MySqlDbType.Int32)
                            .Value = produto.EstoqueMinimo;

                        estoque.Parameters.Add("@id", MySqlDbType.Int32)
                            .Value = produto.Id;

                        int linhasAfetadas = estoque.ExecuteNonQuery();

                        if (linhasAfetadas == 0)
                        {
                            using (MySqlCommand verificar = new MySqlCommand(
                                "SELECT COUNT(*) FROM Produtos WHERE id_produto = @id",
                                conexao))
                            {
                                verificar.Parameters.Add("@id", MySqlDbType.Int32)
                                    .Value = produto.Id;

                                if (Convert.ToInt32(verificar.ExecuteScalar()) == 0)
                                    throw new Exception("Produto não encontrado.");
                            }

                            throw new Exception("Registro de estoque não encontrado.");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(
                    "Erro ao atualizar produto: " + ex.Message, ex);
            }
        }

        // EXCLUIR PRODUTO
        public void Excluir(int id)
        {
            string sqlEstoque = @"
                DELETE FROM Estoque
                WHERE id_produto = @id";

            string sqlProduto = @"
                DELETE FROM Produtos
                WHERE id_produto = @id";

            using (MySqlConnection conexao =
                new MySqlConnection(_connectionString))
            {
                MySqlTransaction? transacao = null;

                try
                {
                    conexao.Open();
                    transacao = conexao.BeginTransaction();

                    using (MySqlCommand estoque = new MySqlCommand(
                        sqlEstoque, conexao, transacao))
                    {
                        estoque.Parameters.Add("@id", MySqlDbType.Int32)
                            .Value = id;

                        estoque.ExecuteNonQuery();
                    }

                    using (MySqlCommand produto = new MySqlCommand(
                        sqlProduto, conexao, transacao))
                    {
                        produto.Parameters.Add("@id", MySqlDbType.Int32)
                            .Value = id;

                        int linhasAfetadas = produto.ExecuteNonQuery();

                        if (linhasAfetadas == 0)
                            throw new Exception("Produto não encontrado.");
                    }

                    transacao.Commit();
                }
                catch (Exception ex)
                {
                    try
                    {
                        transacao?.Rollback();
                    }
                    catch (MySqlException)
                    {
                        // Preserva o erro original.
                    }

                    throw new Exception(
                        "Erro ao excluir produto. Verifique se existem vendas ou pedidos vinculados: "
                        + ex.Message, ex);
                }
                finally
                {
                    transacao?.Dispose();
                }
            }
        }

        // LISTAR TODOS OS PRODUTOS
        public List<Produto> ListarTodos()
        {
            return Listar("", 0);
        }

        // LISTAR PRODUTOS COM FILTRO DE NOME E CATEGORIA
        public List<Produto> Listar(string filtro, int categoriaId)
        {
            List<Produto> produtos = new List<Produto>();

            string sql = @"
                SELECT
                    p.id_produto,
                    p.nome,
                    p.id_categoria,
                    c.nome AS categoria_nome,
                    p.preco_venda,
                    p.status,
                    e.quantidade_atual,
                    e.quantidade_minima
                FROM Produtos p
                INNER JOIN Categorias c
                    ON p.id_categoria = c.id_categoria
                INNER JOIN Estoque e
                    ON p.id_produto = e.id_produto
                WHERE p.nome LIKE @filtro
                AND (@categoria = 0 OR p.id_categoria = @categoria)
                ORDER BY p.nome";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@filtro", MySqlDbType.VarChar)
                        .Value = "%" + filtro + "%";

                    comando.Parameters.Add("@categoria", MySqlDbType.Int32)
                        .Value = categoriaId;

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
                    "Erro ao listar produtos: " + ex.Message, ex);
            }
        }

        // LISTAR SOMENTE PRODUTOS ATIVOS
        public List<Produto> ListarAtivos()
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
                WHERE p.status = TRUE
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
                    "Erro ao listar produtos ativos: " + ex.Message, ex);
            }
        }

        // OBTER PRODUTO PELO ID
        public Produto? ObterPorId(int id)
        {
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
                WHERE p.id_produto = @id";

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
                            return new Produto
                            {
                                Id = reader.GetInt32("id_produto"),
                                Nome = reader.GetString("nome"),
                                CategoriaId = reader.GetInt32("id_categoria"),
                                CategoriaNome = reader.GetString("categoria_nome"),
                                Preco = reader.GetDecimal("preco_venda"),
                                Estoque = reader.GetInt32("quantidade_atual"),
                                EstoqueMinimo = reader.GetInt32("quantidade_minima")
                            };
                        }
                    }
                }

                return null;
            }
            catch (MySqlException ex)
            {
                throw new Exception(
                    "Erro ao buscar produto: " + ex.Message, ex);
            }
        }
    }
}
