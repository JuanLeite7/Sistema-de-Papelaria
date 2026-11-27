
using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using Papelaria.Model;

namespace Papelaria.DAO
{
    public class VendaDAO
    {
        private readonly string _connectionString;

        public VendaDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        // CONVERTER FORMA DE PAGAMENTO PARA O BANCO
        private string ConverterPagamento(string pagamento)
        {
            return pagamento.Trim().ToUpperInvariant() switch
            {
                "DINHEIRO" => "DINHEIRO",
                "PIX" => "PIX",
                "CARTÃO DE CRÉDITO" or "CARTAO DE CREDITO"
                    => "CARTAO_CREDITO",
                "CARTÃO DE DÉBITO" or "CARTAO DE DEBITO"
                    => "CARTAO_DEBITO",
                _ => throw new Exception("Forma de pagamento inválida.")
            };
        }

        // CONVERTER FORMA DE PAGAMENTO PARA A TELA
        private string ConverterPagamentoParaTela(string pagamento)
        {
            return pagamento switch
            {
                "DINHEIRO" => "Dinheiro",
                "PIX" => "Pix",
                "CARTAO_CREDITO" => "Cartão de crédito",
                "CARTAO_DEBITO" => "Cartão de débito",
                _ => pagamento
            };
        }

        // REGISTRAR VENDA COM TRANSAÇÃO
        public void Registrar(Venda venda, int usuarioId)
        {
            if (venda.Itens == null || venda.Itens.Count == 0)
                throw new Exception("A venda precisa possuir pelo menos um item.");

            if (venda.Total < 0 || venda.Desconto < 0 ||
                venda.Desconto > venda.Subtotal)
                throw new Exception("Valores da venda inválidos.");

            if (venda.Itens.Any(i => i.Quantidade <= 0 ||
                i.PrecoUnitario < 0))
                throw new Exception("Existem itens com valores inválidos.");

            if (venda.Itens.GroupBy(i => i.ProdutoId)
                .Any(g => g.Count() > 1))
                throw new Exception("Existem produtos duplicados nos itens da venda.");

            decimal subtotalCalculado = venda.Itens.Sum(i => i.Subtotal);

            if (subtotalCalculado != venda.Subtotal ||
                venda.Total != venda.Subtotal - venda.Desconto)
                throw new Exception("Os valores da venda não conferem.");

            string sqlVenda = @"
                INSERT INTO Vendas
                (id_cliente, id_usuario, data_venda,
                 valor_total, forma_pagamento, status)
                VALUES
                (@cliente, @usuario, @data,
                 @total, @pagamento, 'CONCLUIDA')";

            string sqlVerificarEstoque = @"
                SELECT quantidade_atual
                FROM Estoque
                WHERE id_produto = @produto
                FOR UPDATE";

            string sqlItem = @"
                INSERT INTO ItensVenda
                (id_venda, id_produto, quantidade,
                 preco_unitario, desconto)
                VALUES
                (@venda, @produto, @quantidade,
                 @preco, @desconto)";

            string sqlBaixarEstoque = @"
                UPDATE Estoque
                SET quantidade_atual = quantidade_atual - @quantidade
                WHERE id_produto = @produto
                AND quantidade_atual >= @quantidade";

            using (MySqlConnection conexao =
                new MySqlConnection(_connectionString))
            {
                MySqlTransaction? transacao = null;

                try
                {
                    conexao.Open();
                    transacao = conexao.BeginTransaction();

                    // 1. VERIFICAR ESTOQUE DE TODOS OS PRODUTOS
                    foreach (var item in venda.Itens)
                    {
                        using (MySqlCommand verificar = new MySqlCommand(
                            sqlVerificarEstoque, conexao, transacao))
                        {
                            verificar.Parameters.Add("@produto", MySqlDbType.Int32)
                                .Value = item.ProdutoId;

                            object resultado = verificar.ExecuteScalar();

                            if (resultado == null)
                                throw new Exception(
                                    $"Produto {item.ProdutoNome} não possui registro de estoque.");

                            int quantidadeDisponivel = Convert.ToInt32(resultado);

                            if (quantidadeDisponivel < item.Quantidade)
                                throw new Exception(
                                    $"Estoque insuficiente para o produto {item.ProdutoNome}.");
                        }
                    }

                    // 2. INSERIR VENDA
                    using (MySqlCommand comando = new MySqlCommand(
                        sqlVenda, conexao, transacao))
                    {
                        comando.Parameters.Add("@cliente", MySqlDbType.Int32)
                            .Value = venda.ClienteId.HasValue
                                ? venda.ClienteId.Value : DBNull.Value;

                        comando.Parameters.Add("@usuario", MySqlDbType.Int32)
                            .Value = usuarioId;

                        comando.Parameters.Add("@data", MySqlDbType.DateTime)
                            .Value = venda.Data;

                        comando.Parameters.Add("@total", MySqlDbType.Decimal)
                            .Value = venda.Total;

                        comando.Parameters.Add("@pagamento", MySqlDbType.VarChar)
                            .Value = ConverterPagamento(venda.FormaPagamento);

                        comando.ExecuteNonQuery();

                        venda.Id = Convert.ToInt32(comando.LastInsertedId);
                    }

                    // 3. DISTRIBUIR DESCONTO ENTRE OS ITENS
                    decimal descontoAcumulado = 0m;

                    for (int i = 0; i < venda.Itens.Count; i++)
                    {
                        var item = venda.Itens[i];

                        decimal descontoItem;

                        if (i == venda.Itens.Count - 1)
                        {
                            descontoItem = venda.Desconto - descontoAcumulado;
                        }
                        else
                        {
                            descontoItem = Math.Round(
                                venda.Desconto * item.Subtotal /
                                venda.Subtotal, 2,
                                MidpointRounding.AwayFromZero);

                            descontoAcumulado += descontoItem;
                        }

                        // 4. INSERIR ITEM DA VENDA
                        using (MySqlCommand comandoItem = new MySqlCommand(
                            sqlItem, conexao, transacao))
                        {
                            comandoItem.Parameters.Add("@venda", MySqlDbType.Int32)
                                .Value = venda.Id;

                            comandoItem.Parameters.Add("@produto", MySqlDbType.Int32)
                                .Value = item.ProdutoId;

                            comandoItem.Parameters.Add("@quantidade", MySqlDbType.Int32)
                                .Value = item.Quantidade;

                            comandoItem.Parameters.Add("@preco", MySqlDbType.Decimal)
                                .Value = item.PrecoUnitario;

                            comandoItem.Parameters.Add("@desconto", MySqlDbType.Decimal)
                                .Value = descontoItem;

                            comandoItem.ExecuteNonQuery();
                        }

                        // 5. BAIXAR ESTOQUE
                        using (MySqlCommand estoque = new MySqlCommand(
                            sqlBaixarEstoque, conexao, transacao))
                        {
                            estoque.Parameters.Add("@quantidade", MySqlDbType.Int32)
                                .Value = item.Quantidade;

                            estoque.Parameters.Add("@produto", MySqlDbType.Int32)
                                .Value = item.ProdutoId;

                            if (estoque.ExecuteNonQuery() != 1)
                                throw new Exception(
                                    $"Não foi possível atualizar o estoque de {item.ProdutoNome}.");
                        }
                    }

                    // 6. CONFIRMAR TODAS AS OPERAÇÕES
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
                        // Preserva a exceção original.
                    }

                    throw new Exception(
                        "Erro ao registrar venda: " + ex.Message, ex);
                }
                finally
                {
                    transacao?.Dispose();
                }
            }
        }

        // LISTAR HISTÓRICO DE VENDAS
        public List<Venda> Listar()
        {
            List<Venda> vendas = new List<Venda>();

            string sql = @"
                SELECT
                    v.id_venda,
                    v.id_cliente,
                    COALESCE(c.nome, 'Consumidor final') AS cliente_nome,
                    v.data_venda,
                    v.valor_total,
                    v.forma_pagamento
                FROM Vendas v
                LEFT JOIN Clientes c
                    ON v.id_cliente = c.id_cliente
                WHERE v.status = 'CONCLUIDA'
                ORDER BY v.data_venda DESC";

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
                            vendas.Add(new Venda
                            {
                                Id = reader.GetInt32("id_venda"),

                                ClienteId = reader.IsDBNull(
                                    reader.GetOrdinal("id_cliente"))
                                    ? null : reader.GetInt32("id_cliente"),

                                ClienteNome = reader.GetString("cliente_nome"),
                                Data = reader.GetDateTime("data_venda"),
                                Total = reader.GetDecimal("valor_total"),

                                FormaPagamento = ConverterPagamentoParaTela(
                                    reader.GetString("forma_pagamento"))
                            });
                        }
                    }
                }

                foreach (var venda in vendas)
                {
                    venda.Itens = ListarItens(venda.Id);

                    venda.Desconto = venda.Itens.Sum(i =>
                        ObterDescontoItem(venda.Id, i.ProdutoId));

                    venda.Subtotal = venda.Itens.Sum(i => i.Subtotal);
                }

                return vendas;
            }
            catch (MySqlException ex)
            {
                throw new Exception(
                    "Erro ao listar vendas: " + ex.Message, ex);
            }
        }

        // LISTAR ITENS DE UMA VENDA
        private List<ItemVenda> ListarItens(int vendaId)
        {
            List<ItemVenda> itens = new List<ItemVenda>();

            string sql = @"
                SELECT
                    iv.id_produto,
                    p.nome AS produto_nome,
                    iv.quantidade,
                    iv.preco_unitario
                FROM ItensVenda iv
                INNER JOIN Produtos p
                    ON iv.id_produto = p.id_produto
                WHERE iv.id_venda = @venda
                ORDER BY iv.id_item_venda";

            using (MySqlConnection conexao =
                new MySqlConnection(_connectionString))
            using (MySqlCommand comando =
                new MySqlCommand(sql, conexao))
            {
                comando.Parameters.Add("@venda", MySqlDbType.Int32)
                    .Value = vendaId;

                conexao.Open();

                using (MySqlDataReader reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        itens.Add(new ItemVenda
                        {
                            ProdutoId = reader.GetInt32("id_produto"),
                            ProdutoNome = reader.GetString("produto_nome"),
                            Quantidade = reader.GetInt32("quantidade"),
                            PrecoUnitario = reader.GetDecimal("preco_unitario")
                        });
                    }
                }
            }

            return itens;
        }

        // OBTER DESCONTO DE UM ITEM
        private decimal ObterDescontoItem(int vendaId, int produtoId)
        {
            string sql = @"
                SELECT desconto
                FROM ItensVenda
                WHERE id_venda = @venda
                AND id_produto = @produto";

            using (MySqlConnection conexao =
                new MySqlConnection(_connectionString))
            using (MySqlCommand comando =
                new MySqlCommand(sql, conexao))
            {
                comando.Parameters.Add("@venda", MySqlDbType.Int32)
                    .Value = vendaId;

                comando.Parameters.Add("@produto", MySqlDbType.Int32)
                    .Value = produtoId;

                conexao.Open();

                return Convert.ToDecimal(comando.ExecuteScalar());
            }
        }

        // OBTER VENDA PELO ID
        public Venda? ObterPorId(int id)
        {
            Venda? venda = Listar()
                .FirstOrDefault(v => v.Id == id);

            return venda;
        }

        // CANCELAR VENDA E DEVOLVER PRODUTOS AO ESTOQUE
        public void CancelarVenda(int vendaId)
        {
            string sqlVerificar = @"
                SELECT status
                FROM Vendas
                WHERE id_venda = @id
                FOR UPDATE";

            string sqlItens = @"
                SELECT id_produto, quantidade
                FROM ItensVenda
                WHERE id_venda = @id";

            string sqlEstoque = @"
                UPDATE Estoque
                SET quantidade_atual = quantidade_atual + @quantidade
                WHERE id_produto = @produto";

            string sqlCancelar = @"
                UPDATE Vendas
                SET status = 'CANCELADA'
                WHERE id_venda = @id
                AND status = 'CONCLUIDA'";

            using (MySqlConnection conexao =
                new MySqlConnection(_connectionString))
            {
                MySqlTransaction? transacao = null;

                try
                {
                    conexao.Open();
                    transacao = conexao.BeginTransaction();

                    // 1. VERIFICAR VENDA
                    using (MySqlCommand verificar = new MySqlCommand(
                        sqlVerificar, conexao, transacao))
                    {
                        verificar.Parameters.Add("@id", MySqlDbType.Int32)
                            .Value = vendaId;

                        object resultado = verificar.ExecuteScalar();

                        if (resultado == null)
                            throw new Exception("Venda não encontrada.");

                        if (Convert.ToString(resultado) != "CONCLUIDA")
                            throw new Exception(
                                "Esta venda já está cancelada.");
                    }

                    // 2. BUSCAR ITENS DA VENDA
                    List<ItemVenda> itens = new List<ItemVenda>();

                    using (MySqlCommand comando = new MySqlCommand(
                        sqlItens, conexao, transacao))
                    {
                        comando.Parameters.Add("@id", MySqlDbType.Int32)
                            .Value = vendaId;

                        using (MySqlDataReader reader = comando.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                itens.Add(new ItemVenda
                                {
                                    ProdutoId = reader.GetInt32("id_produto"),
                                    Quantidade = reader.GetInt32("quantidade")
                                });
                            }
                        }
                    }

                    // 3. DEVOLVER QUANTIDADES AO ESTOQUE
                    foreach (var item in itens)
                    {
                        using (MySqlCommand estoque = new MySqlCommand(
                            sqlEstoque, conexao, transacao))
                        {
                            estoque.Parameters.Add("@quantidade", MySqlDbType.Int32)
                                .Value = item.Quantidade;

                            estoque.Parameters.Add("@produto", MySqlDbType.Int32)
                                .Value = item.ProdutoId;

                            if (estoque.ExecuteNonQuery() != 1)
                                throw new Exception(
                                    "Não foi possível devolver um produto ao estoque.");
                        }
                    }

                    // 4. ALTERAR STATUS DA VENDA
                    using (MySqlCommand cancelar = new MySqlCommand(
                        sqlCancelar, conexao, transacao))
                    {
                        cancelar.Parameters.Add("@id", MySqlDbType.Int32)
                            .Value = vendaId;

                        if (cancelar.ExecuteNonQuery() != 1)
                            throw new Exception(
                                "Não foi possível cancelar a venda.");
                    }

                    // 5. CONFIRMAR OPERAÇÕES
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
                        // Preserva a exceção original.
                    }

                    throw new Exception(
                        "Erro ao cancelar venda: " + ex.Message, ex);
                }
                finally
                {
                    transacao?.Dispose();
                }
            }
        }
    }
}
