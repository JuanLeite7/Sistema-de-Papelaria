
using MySqlConnector;
using Papelaria.Model;

namespace Papelaria.DAO
{
    public class ItemVendaDAO
    {
        private readonly string _connectionString;

        public ItemVendaDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Lista os itens de uma venda específica
        public List<ItemVenda> ListarPorVenda(int vendaId)
        {
            List<ItemVenda> itens = new List<ItemVenda>();

            using (MySqlConnection conexao = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT
                        iv.id_produto,
                        p.nome AS produto_nome,
                        iv.quantidade,
                        iv.preco_unitario
                    FROM ItensVenda iv
                    INNER JOIN Produtos p
                        ON iv.id_produto = p.id_produto
                    WHERE iv.id_venda = @vendaId
                    ORDER BY p.nome;";

                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.AddWithValue("@vendaId", vendaId);

                    conexao.Open();

                    using (MySqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ItemVenda item = new ItemVenda
                            {
                                ProdutoId = Convert.ToInt32(reader["id_produto"]),
                                ProdutoNome = reader["produto_nome"].ToString() ?? "",
                                Quantidade = Convert.ToInt32(reader["quantidade"]),
                                PrecoUnitario = Convert.ToDecimal(reader["preco_unitario"])
                            };

                            itens.Add(item);
                        }
                    }
                }
            }

            return itens;
        }
    }
}
