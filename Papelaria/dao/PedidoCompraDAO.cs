
using MySqlConnector;
using Papelaria.Model;

namespace Papelaria.DAO
{
    public class PedidoCompraDAO
    {
        private readonly string _connectionString;

        public PedidoCompraDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Registra um novo pedido de compra
        public int Registrar(PedidoCompra pedido)
        {
            if (pedido.Itens == null || pedido.Itens.Count == 0)
                throw new Exception("O pedido precisa ter pelo menos um produto.");

            if (pedido.FornecedorId <= 0 || pedido.UsuarioId <= 0)
                throw new Exception("Fornecedor e usuário são obrigatórios.");

            foreach (var item in pedido.Itens)
            {
                if (item.ProdutoId <= 0 ||
                    item.Quantidade <= 0 ||
                    item.PrecoUnitario < 0)
                {
                    throw new Exception("Existem itens com dados inválidos.");
                }
            }

            decimal valorTotal = pedido.Itens.Sum(item => item.Subtotal);

            using (MySqlConnection conexao = new MySqlConnection(_connectionString))
            {
                conexao.Open();

                using (MySqlTransaction transacao = conexao.BeginTransaction())
                {
                    try
                    {
                        string sqlPedido = @"
                            INSERT INTO PedidosCompra
                            (id_fornecedor, id_usuario, data_pedido, status, valor_total)
                            VALUES
                            (@fornecedor, @usuario, @data, 'PENDENTE', @total);";

                        int pedidoId;

                        using (MySqlCommand comando = new MySqlCommand(
                            sqlPedido, conexao, transacao))
                        {
                            comando.Parameters.AddWithValue(
                                "@fornecedor", pedido.FornecedorId);

                            comando.Parameters.AddWithValue(
                                "@usuario", pedido.UsuarioId);

                            comando.Parameters.AddWithValue(
                                "@data", pedido.DataPedido);

                            comando.Parameters.AddWithValue(
                                "@total", valorTotal);

                            comando.ExecuteNonQuery();

                            pedidoId = Convert.ToInt32(comando.LastInsertedId);
                        }

                        string sqlItem = @"
                            INSERT INTO ItensPedidoCompra
                            (id_pedido, id_produto, quantidade, preco_unitario)
                            VALUES
                            (@pedido, @produto, @quantidade, @preco);";

                        foreach (var item in pedido.Itens)
                        {
                            using (MySqlCommand comando = new MySqlCommand(
                                sqlItem, conexao, transacao))
                            {
                                comando.Parameters.AddWithValue(
                                    "@pedido", pedidoId);

                                comando.Parameters.AddWithValue(
                                    "@produto", item.ProdutoId);

                                comando.Parameters.AddWithValue(
                                    "@quantidade", item.Quantidade);

                                comando.Parameters.AddWithValue(
                                    "@preco", item.PrecoUnitario);

                                comando.ExecuteNonQuery();
                            }
                        }

                        transacao.Commit();

                        return pedidoId;
                    }
                    catch
                    {
                        transacao.Rollback();
                        throw;
                    }
                }
            }
        }

        // Lista todos os pedidos
        public List<PedidoCompra> Listar()
        {
            List<PedidoCompra> pedidos = new List<PedidoCompra>();

            using (MySqlConnection conexao = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT id_pedido
                    FROM PedidosCompra
                    ORDER BY data_pedido DESC;";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    conexao.Open();

                    using (MySqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = Convert.ToInt32(reader["id_pedido"]);
                            pedidos.Add(ObterPorId(id)!);
                        }
                    }
                }
            }

            return pedidos;
        }

        // Busca um pedido pelo código
        public PedidoCompra? ObterPorId(int id)
        {
            PedidoCompra? pedido = null;

            using (MySqlConnection conexao = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT id_pedido, id_fornecedor, id_usuario,
                           data_pedido, status, valor_total
                    FROM PedidosCompra
                    WHERE id_pedido = @id;";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.AddWithValue("@id", id);

                    conexao.Open();

                    using (MySqlDataReader reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pedido = new PedidoCompra
                            {
                                Id = Convert.ToInt32(reader["id_pedido"]),
                                FornecedorId = Convert.ToInt32(reader["id_fornecedor"]),
                                UsuarioId = Convert.ToInt32(reader["id_usuario"]),
                                DataPedido = Convert.ToDateTime(reader["data_pedido"]),
                                Status = reader["status"].ToString() ?? "PENDENTE",
                                ValorTotal = Convert.ToDecimal(reader["valor_total"])
                            };
                        }
                    }
                }
            }

            if (pedido != null)
            {
                pedido.Itens = ListarItens(pedido.Id);
            }

            return pedido;
        }

        // Lista os produtos de um pedido
        private List<ItemPedidoCompra> ListarItens(int pedidoId)
        {
            List<ItemPedidoCompra> itens = new List<ItemPedidoCompra>();

            using (MySqlConnection conexao = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT id_item_pedido, id_pedido, id_produto,
                           quantidade, preco_unitario
                    FROM ItensPedidoCompra
                    WHERE id_pedido = @pedido;";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.AddWithValue("@pedido", pedidoId);

                    conexao.Open();

                    using (MySqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            itens.Add(new ItemPedidoCompra
                            {
                                Id = Convert.ToInt32(reader["id_item_pedido"]),
                                PedidoId = Convert.ToInt32(reader["id_pedido"]),
                                ProdutoId = Convert.ToInt32(reader["id_produto"]),
                                Quantidade = Convert.ToInt32(reader["quantidade"]),
                                PrecoUnitario = Convert.ToDecimal(reader["preco_unitario"])
                            });
                        }
                    }
                }
            }

            return itens;
        }

        // Atualiza o status do pedido
        public void AtualizarStatus(int pedidoId, string novoStatus)
        {
            novoStatus = novoStatus.ToUpperInvariant();

            if (novoStatus != "PENDENTE" &&
                novoStatus != "RECEBIDO" &&
                novoStatus != "CANCELADO")
            {
                throw new Exception("Status do pedido inválido.");
            }

            using (MySqlConnection conexao = new MySqlConnection(_connectionString))
            {
                conexao.Open();

                using (MySqlTransaction transacao = conexao.BeginTransaction())
                {
                    try
                    {
                        string statusAtual;

                        string sqlBusca = @"
                            SELECT status
                            FROM PedidosCompra
                            WHERE id_pedido = @id
                            FOR UPDATE;";

                        using (MySqlCommand comando = new MySqlCommand(
                            sqlBusca, conexao, transacao))
                        {
                            comando.Parameters.AddWithValue("@id", pedidoId);

                            object? resultado = comando.ExecuteScalar();

                            if (resultado == null)
                                throw new Exception("Pedido não encontrado.");

                            statusAtual = resultado.ToString()!;
                        }

                        if (statusAtual != "PENDENTE")
                            throw new Exception(
                                "Somente pedidos pendentes podem ser atualizados.");

                        if (novoStatus == "RECEBIDO")
                        {
                            string sqlItens = @"
                                SELECT id_produto, quantidade
                                FROM ItensPedidoCompra
                                WHERE id_pedido = @pedido;";

                            using (MySqlCommand comando = new MySqlCommand(
                                sqlItens, conexao, transacao))
                            {
                                comando.Parameters.AddWithValue("@pedido", pedidoId);

                                using (MySqlDataReader reader = comando.ExecuteReader())
                                {
                                    List<(int ProdutoId, int Quantidade)> itens = new();

                                    while (reader.Read())
                                    {
                                        itens.Add((
                                            Convert.ToInt32(reader["id_produto"]),
                                            Convert.ToInt32(reader["quantidade"])
                                        ));
                                    }

                                    reader.Close();

                                    foreach (var item in itens)
                                    {
                                        string sqlEstoque = @"
                                            UPDATE Estoque
                                            SET quantidade_atual = quantidade_atual + @quantidade
                                            WHERE id_produto = @produto;";

                                        using (MySqlCommand estoque = new MySqlCommand(
                                            sqlEstoque, conexao, transacao))
                                        {
                                            estoque.Parameters.AddWithValue(
                                                "@quantidade", item.Quantidade);

                                            estoque.Parameters.AddWithValue(
                                                "@produto", item.ProdutoId);

                                            int linhas = estoque.ExecuteNonQuery();

                                            if (linhas == 0)
                                                throw new Exception(
                                                    "Não foi encontrado estoque para um dos produtos.");
                                        }
                                    }
                                }
                            }
                        }

                        string sqlAtualiza = @"
                            UPDATE PedidosCompra
                            SET status = @status
                            WHERE id_pedido = @id;";

                        using (MySqlCommand comando = new MySqlCommand(
                            sqlAtualiza, conexao, transacao))
                        {
                            comando.Parameters.AddWithValue("@status", novoStatus);
                            comando.Parameters.AddWithValue("@id", pedidoId);
                            comando.ExecuteNonQuery();
                        }

                        transacao.Commit();
                    }
                    catch
                    {
                        transacao.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
