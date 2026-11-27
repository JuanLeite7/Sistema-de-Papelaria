
using System;
using System.Collections.Generic;
using MySqlConnector;
using Papelaria.Model;

namespace Papelaria.DAO
{
    public class ClienteDAO
    {
        private readonly string _connectionString;

        public ClienteDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        // INSERIR CLIENTE
        public void Inserir(Cliente cliente)
        {
            string sql = @"
                INSERT INTO Clientes
                (nome, cpf, telefone, email, endereco)
                VALUES
                (@nome, @cpf, @telefone, @email, @endereco)";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@nome", MySqlDbType.VarChar)
                        .Value = cliente.Nome;

                    comando.Parameters.Add("@cpf", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(cliente.Cpf)
                            ? DBNull.Value : cliente.Cpf;

                    comando.Parameters.Add("@telefone", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(cliente.Telefone)
                            ? DBNull.Value : cliente.Telefone;

                    comando.Parameters.Add("@email", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(cliente.Email)
                            ? DBNull.Value : cliente.Email;

                    comando.Parameters.Add("@endereco", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(cliente.Endereco)
                            ? DBNull.Value : cliente.Endereco;

                    conexao.Open();
                    comando.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Erro ao inserir cliente: " + ex.Message, ex);
            }
        }

        // ATUALIZAR CLIENTE
        public void Atualizar(Cliente cliente)
        {
            string sql = @"
                UPDATE Clientes SET
                    nome = @nome,
                    cpf = @cpf,
                    telefone = @telefone,
                    email = @email,
                    endereco = @endereco
                WHERE id_cliente = @id";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@nome", MySqlDbType.VarChar)
                        .Value = cliente.Nome;

                    comando.Parameters.Add("@cpf", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(cliente.Cpf)
                            ? DBNull.Value : cliente.Cpf;

                    comando.Parameters.Add("@telefone", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(cliente.Telefone)
                            ? DBNull.Value : cliente.Telefone;

                    comando.Parameters.Add("@email", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(cliente.Email)
                            ? DBNull.Value : cliente.Email;

                    comando.Parameters.Add("@endereco", MySqlDbType.VarChar)
                        .Value = string.IsNullOrWhiteSpace(cliente.Endereco)
                            ? DBNull.Value : cliente.Endereco;

                    comando.Parameters.Add("@id", MySqlDbType.Int32)
                        .Value = cliente.Id;

                    conexao.Open();

                    int linhasAfetadas = comando.ExecuteNonQuery();

                    if (linhasAfetadas == 0)
                    {
                        using (MySqlCommand verificar = new MySqlCommand(
                            "SELECT COUNT(*) FROM Clientes WHERE id_cliente = @id",
                            conexao))
                        {
                            verificar.Parameters.Add("@id", MySqlDbType.Int32)
                                .Value = cliente.Id;

                            if (Convert.ToInt32(verificar.ExecuteScalar()) == 0)
                                throw new Exception("Cliente não encontrado.");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Erro ao atualizar cliente: " + ex.Message, ex);
            }
        }

        // EXCLUIR CLIENTE
        public void Excluir(int id)
        {
            string sql = "DELETE FROM Clientes WHERE id_cliente = @id";

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
                        throw new Exception("Cliente não encontrado.");
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Erro ao excluir cliente: " + ex.Message, ex);
            }
        }

        // LISTAR TODOS OS CLIENTES
        public List<Cliente> ListarTodos()
        {
            List<Cliente> clientes = new List<Cliente>();

            string sql = @"
                SELECT id_cliente, nome, cpf, telefone, email, endereco
                FROM Clientes
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
                            clientes.Add(new Cliente
                            {
                                Id = reader.GetInt32("id_cliente"),
                                Nome = reader.GetString("nome"),
                                Cpf = reader.IsDBNull(reader.GetOrdinal("cpf"))
                                    ? "" : reader.GetString("cpf"),
                                Telefone = reader.IsDBNull(reader.GetOrdinal("telefone"))
                                    ? "" : reader.GetString("telefone"),
                                Email = reader.IsDBNull(reader.GetOrdinal("email"))
                                    ? "" : reader.GetString("email"),
                                Endereco = reader.IsDBNull(reader.GetOrdinal("endereco"))
                                    ? "" : reader.GetString("endereco")
                            });
                        }
                    }
                }

                return clientes;
            }
            catch (MySqlException ex)
            {
                throw new Exception("Erro ao listar clientes: " + ex.Message, ex);
            }
        }

        // OBTER CLIENTE PELO ID
        public Cliente? ObterPorId(int id)
        {
            string sql = @"
                SELECT id_cliente, nome, cpf, telefone, email, endereco
                FROM Clientes
                WHERE id_cliente = @id";

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
                            return new Cliente
                            {
                                Id = reader.GetInt32("id_cliente"),
                                Nome = reader.GetString("nome"),
                                Cpf = reader.IsDBNull(reader.GetOrdinal("cpf"))
                                    ? "" : reader.GetString("cpf"),
                                Telefone = reader.IsDBNull(reader.GetOrdinal("telefone"))
                                    ? "" : reader.GetString("telefone"),
                                Email = reader.IsDBNull(reader.GetOrdinal("email"))
                                    ? "" : reader.GetString("email"),
                                Endereco = reader.IsDBNull(reader.GetOrdinal("endereco"))
                                    ? "" : reader.GetString("endereco")
                            };
                        }
                    }
                }

                return null;
            }
            catch (MySqlException ex)
            {
                throw new Exception("Erro ao buscar cliente: " + ex.Message, ex);
            }
        }
    }
}
