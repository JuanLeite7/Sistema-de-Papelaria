using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using MySqlConnector;
using Papelaria.Model;

namespace Papelaria.DAO
{
    public class UsuarioDAO
    {
        private readonly string _connectionString;

        public UsuarioDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        // GERAR HASH DA SENHA
        private string GerarHashSenha(string senha)
        {
            const int iteracoes = 210000;
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                senha,
                salt,
                iteracoes,
                HashAlgorithmName.SHA256,
                32
            );

            return $"PBKDF2${iteracoes}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        // VERIFICAR SENHA
        private bool VerificarSenha(string senha, string hashArmazenado)
        {
            try
            {
                string[] partes = hashArmazenado.Split('$');

                if (partes.Length != 4 || partes[0] != "PBKDF2")
                    return false;

                int iteracoes = int.Parse(partes[1]);
                byte[] salt = Convert.FromBase64String(partes[2]);
                byte[] hashOriginal = Convert.FromBase64String(partes[3]);

                byte[] hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
                    senha,
                    salt,
                    iteracoes,
                    HashAlgorithmName.SHA256,
                    hashOriginal.Length
                );

                return CryptographicOperations.FixedTimeEquals(
                    hashOriginal,
                    hashCalculado
                );
            }
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        // CONVERTER PERFIL PARA O PADRÃO DO BANCO
        private string ConverterPerfilParaBanco(string perfil)
        {
            return perfil.Trim().ToUpperInvariant() switch
            {
                "ADMIN" or "ADMINISTRADOR" => "ADMIN",
                "VENDEDOR" => "VENDEDOR",
                "ESTOQUISTA" => "ESTOQUISTA",
                _ => throw new Exception("Perfil de usuário inválido.")
            };
        }

        // CONVERTER PERFIL DO BANCO PARA O MODEL
        private string ConverterPerfilParaModel(string perfil)
        {
            return perfil switch
            {
                "ADMIN" => "Administrador",
                "VENDEDOR" => "Vendedor",
                "ESTOQUISTA" => "Estoquista",
                _ => perfil
            };
        }

        // INSERIR USUÁRIO
        public void Inserir(Usuario usuario)
        {
            string sql = @"
                INSERT INTO Usuarios
                (nome, email, senha_hash, tipo, status)
                VALUES
                (@nome, @email, @senha_hash, @tipo, @status)";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@nome", MySqlDbType.VarChar)
                        .Value = usuario.Nome;

                    comando.Parameters.Add("@email", MySqlDbType.VarChar)
                        .Value = usuario.Login;

                    comando.Parameters.Add("@senha_hash", MySqlDbType.VarChar)
                        .Value = GerarHashSenha(usuario.Senha);

                    comando.Parameters.Add("@tipo", MySqlDbType.VarChar)
                        .Value = ConverterPerfilParaBanco(usuario.Perfil);

                    comando.Parameters.Add("@status", MySqlDbType.Bool)
                        .Value = usuario.Ativo;

                    conexao.Open();
                    comando.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Erro ao inserir usuário: " + ex.Message, ex);
            }
        }

        // ATUALIZAR USUÁRIO
        public void Atualizar(Usuario usuario)
        {
            string sql;

            if (string.IsNullOrWhiteSpace(usuario.Senha))
            {
                sql = @"
                    UPDATE Usuarios SET
                    nome = @nome,
                    email = @email,
                    tipo = @tipo,
                    status = @status
                    WHERE id_usuario = @id";
            }
            else
            {
                sql = @"
                    UPDATE Usuarios SET
                    nome = @nome,
                    email = @email,
                    senha_hash = @senha_hash,
                    tipo = @tipo,
                    status = @status
                    WHERE id_usuario = @id";
            }

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@nome", MySqlDbType.VarChar)
                        .Value = usuario.Nome;

                    comando.Parameters.Add("@email", MySqlDbType.VarChar)
                        .Value = usuario.Login;

                    comando.Parameters.Add("@tipo", MySqlDbType.VarChar)
                        .Value = ConverterPerfilParaBanco(usuario.Perfil);

                    comando.Parameters.Add("@status", MySqlDbType.Bool)
                        .Value = usuario.Ativo;

                    comando.Parameters.Add("@id", MySqlDbType.Int32)
                        .Value = usuario.Id;

                    if (!string.IsNullOrWhiteSpace(usuario.Senha))
                    {
                        comando.Parameters.Add("@senha_hash", MySqlDbType.VarChar)
                            .Value = GerarHashSenha(usuario.Senha);
                    }

                    conexao.Open();

                    int linhasAfetadas = comando.ExecuteNonQuery();

                    if (linhasAfetadas == 0)
                    {
                        using (MySqlCommand verificar = new MySqlCommand(
                            "SELECT COUNT(*) FROM Usuarios WHERE id_usuario = @id",
                            conexao))
                        {
                            verificar.Parameters.Add("@id", MySqlDbType.Int32)
                                .Value = usuario.Id;

                            if (Convert.ToInt32(verificar.ExecuteScalar()) == 0)
                                throw new Exception("Usuário não encontrado.");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Erro ao atualizar usuário: " + ex.Message, ex);
            }
        }

        // EXCLUIR USUÁRIO (DESATIVAR)
        public void Excluir(int id)
        {
            // Desativação lógica para preservar o histórico de vendas e pedidos.
            string sql = @"
                UPDATE Usuarios
                SET status = FALSE
                WHERE id_usuario = @id";

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
                    {
                        using (MySqlCommand verificar = new MySqlCommand(
                            "SELECT COUNT(*) FROM Usuarios WHERE id_usuario = @id",
                            conexao))
                        {
                            verificar.Parameters.Add("@id", MySqlDbType.Int32)
                                .Value = id;

                            if (Convert.ToInt32(verificar.ExecuteScalar()) == 0)
                                throw new Exception("Usuário não encontrado.");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Erro ao desativar usuário: " + ex.Message, ex);
            }
        }

        // LISTAR TODOS OS USUÁRIOS
        public List<Usuario> ListarTodos()
        {
            List<Usuario> usuarios = new List<Usuario>();

            string sql = @"
                SELECT id_usuario, nome, email, tipo, status
                FROM Usuarios
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
                            usuarios.Add(new Usuario
                            {
                                Id = reader.GetInt32("id_usuario"),
                                Nome = reader.GetString("nome"),
                                Login = reader.GetString("email"),
                                Perfil = ConverterPerfilParaModel(
                                    reader.GetString("tipo")),
                                Ativo = reader.GetBoolean("status"),
                                Senha = ""
                            });
                        }
                    }
                }

                return usuarios;
            }
            catch (MySqlException ex)
            {
                throw new Exception("Erro ao listar usuários: " + ex.Message, ex);
            }
        }

        // OBTER USUÁRIO PELO ID
        public Usuario? ObterPorId(int id)
        {
            string sql = @"
                SELECT id_usuario, nome, email, tipo, status
                FROM Usuarios
                WHERE id_usuario = @id";

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
                            return new Usuario
                            {
                                Id = reader.GetInt32("id_usuario"),
                                Nome = reader.GetString("nome"),
                                Login = reader.GetString("email"),
                                Perfil = ConverterPerfilParaModel(
                                    reader.GetString("tipo")),
                                Ativo = reader.GetBoolean("status"),
                                Senha = ""
                            };
                        }
                    }
                }

                return null;
            }
            catch (MySqlException ex)
            {
                throw new Exception("Erro ao buscar usuário: " + ex.Message, ex);
            }
        }

        // AUTENTICAR USUÁRIO
        public Usuario? Autenticar(string email, string senha)
        {
            string sql = @"
                SELECT id_usuario, nome, email, senha_hash, tipo, status
                FROM Usuarios
                WHERE email = @email AND status = TRUE";

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(_connectionString))
                using (MySqlCommand comando =
                    new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.Add("@email", MySqlDbType.VarChar)
                        .Value = email;

                    conexao.Open();

                    using (MySqlDataReader reader = comando.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;

                        string hashArmazenado = reader.GetString("senha_hash");

                        if (!VerificarSenha(senha, hashArmazenado))
                            return null;

                        return new Usuario
                        {
                            Id = reader.GetInt32("id_usuario"),
                            Nome = reader.GetString("nome"),
                            Login = reader.GetString("email"),
                            Perfil = ConverterPerfilParaModel(
                                reader.GetString("tipo")),
                            Ativo = reader.GetBoolean("status"),
                            Senha = ""
                        };
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Erro ao autenticar usuário: " + ex.Message, ex);
            }
        }
    }
}