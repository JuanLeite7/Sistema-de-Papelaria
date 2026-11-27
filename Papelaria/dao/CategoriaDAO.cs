
using System;
using System.Collections.Generic;
using MySqlConnector;
using Papelaria.Model;

namespace Papelaria.DAO
{
    public class CategoriaDAO
    {
        private readonly string _connectionString;

        public CategoriaDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        // INSERIR CATEGORIA
        public void Inserir(Categoria categoria)
        {
            string sql = @"
                INSERT INTO Categorias
                (nome, descricao)
                VALUES
                (@nome, @descricao)";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@nome", MySqlDbType.VarChar)
                        .Value = categoria.Nome;

                    comando.Parameters.Add("@descricao", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(categoria.Descricao)
                            ? DBNull.Value : categoria.Descricao;

                    conexao.Open();
                    comando.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Erro ao inserir categoria: " + ex.Message, ex);
            }
        }

        // ATUALIZAR CATEGORIA
        public void Atualizar(Categoria categoria)
        {
            string sql = @"
                UPDATE Categorias SET
                    nome = @nome,
                    descricao = @descricao
                WHERE id_categoria = @id";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@nome", MySqlDbType.VarChar)
                        .Value = categoria.Nome;

                    comando.Parameters.Add("@descricao", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(categoria.Descricao)
                            ? DBNull.Value : categoria.Descricao;

                    comando.Parameters.Add("@id", MySqlDbType.Int32)
                        .Value = categoria.Id;

                    conexao.Open();

                    int linhasAfetadas = comando.ExecuteNonQuery();

                    if (linhasAfetadas == 0)
                    {
                        using (MySqlCommand verificar = new MySqlCommand(
                            "SELECT COUNT(*) FROM Categorias WHERE id_categoria = @id",
                            conexao))
                        {
                            verificar.Parameters.Add("@id", MySqlDbType.Int32)
                                .Value = categoria.Id;

                            if (Convert.ToInt32(verificar.ExecuteScalar()) == 0)
                                throw new Exception("Categoria não encontrada.");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Erro ao atualizar categoria: " + ex.Message, ex);
            }
        }

        // EXCLUIR CATEGORIA
        public void Excluir(int id)
        {
            string sql = "DELETE FROM Categorias WHERE id_categoria = @id";

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
                        throw new Exception("Categoria não encontrada.");
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(
                    "Erro ao excluir categoria. Verifique se existem produtos vinculados a ela: "
                    + ex.Message, ex);
            }
        }

        // LISTAR TODAS AS CATEGORIAS
        public List<Categoria> ListarTodos()
        {
            List<Categoria> categorias = new List<Categoria>();

            string sql = @"
                SELECT id_categoria, nome, descricao
                FROM Categorias
                ORDER BY nome";

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
                            categorias.Add(new Categoria
                            {
                                Id = reader.GetInt32("id_categoria"),
                                Nome = reader.GetString("nome"),
                                Descricao = reader.IsDBNull(
                                    reader.GetOrdinal("descricao"))
                                    ? "" : reader.GetString("descricao")
                            });
                        }
                    }
                }

                return categorias;
            }
            catch (MySqlException ex)
            {
                throw new Exception("Erro ao listar categorias: " + ex.Message, ex);
            }
        }

        // OBTER CATEGORIA PELO ID
        public Categoria? ObterPorId(int id)
        {
            string sql = @"
                SELECT id_categoria, nome, descricao
                FROM Categorias
                WHERE id_categoria = @id";

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
                            return new Categoria
                            {
                                Id = reader.GetInt32("id_categoria"),
                                Nome = reader.GetString("nome"),
                                Descricao = reader.IsDBNull(
                                    reader.GetOrdinal("descricao"))
                                    ? "" : reader.GetString("descricao")
                            };
                        }
                    }
                }

                return null;
            }
            catch (MySqlException ex)
            {
                throw new Exception("Erro ao buscar categoria: " + ex.Message, ex);
            }
        }
    }
}
