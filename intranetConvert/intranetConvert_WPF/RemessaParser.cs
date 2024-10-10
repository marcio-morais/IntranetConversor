using System.IO;
using System.Text;

namespace intranetConvert_WPF
{
    internal partial class RemessaParser
    {
        private readonly string _inputFile;
        private readonly string _outputFile;
        public int sequencia = 0;

        public static readonly string[] _csvHeader = new[]
        {
            "Número pedido", "Nome Comprador", "Data", "CPF/CNPJ Comprador", "Endereço Comprador",
            "Bairro Comprador", "Número Comprador", "Complemento Comprador", "CEP Comprador",
            "Cidade Comprador", "UF Comprador", "Telefone Comprador", "Celular Comprador",
            "E-mail Comprador", "Produto", "SKU", "Un", "Quantidade", "Valor Unitário",
            "Valor Total", "Total Pedido", "Valor Frete Pedido", "Valor Desconto Pedido",
            "Outras despesas", "Nome Entrega", "Endereço Entrega", "Número Entrega",
            "Complemento Entrega", "Cidade Entrega", "UF Entrega", "CEP Entrega",
            "Bairro Entrega", "Transportadora", "Serviço", "Tipo Frete", "Observações",
            "Qtd Parcela", "Data Prevista", "Vendedor", "Forma Pagamento", "ID Forma Pagamento"
        };

        public RemessaParser(string inputFile)
        {
            _inputFile = inputFile;
        }

        public async Task<List<Dictionary<string, string>>> ParseRemessa()
        {
            var pedidos = new List<Dictionary<string, string>>();
            Dictionary<string, string> currentPedido = null;
            string observacoes = "";
            int cont = 0;

            string fileContent;
            EncodingDetector.DetectTextEncoding(_inputFile, out fileContent);

            using (var reader = new StringReader(fileContent))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.Split('§');
                    var identifier = fields[0];

                    switch (identifier)
                    {
                        case "22":
                            {
                                sequencia++;
                                currentPedido = await ParsePedidoHeader(fields);
                                observacoes = "";
                            }
                            break;
                        case "23":
                            {
                                if (currentPedido != null)
                                    if (cont > 2)
                                        observacoes += string.Join(" ", fields.Skip(1));
                                cont++;
                            }
                            break;

                        case "24":
                            if (currentPedido != null)
                                ParsePedidoItem(fields, currentPedido);
                            break;

                        case "25":
                            if (currentPedido != null)
                            {
                                if (!observacoes.Equals(""))
                                    currentPedido["Observações"] = observacoes.ToString().Trim();

                                pedidos.Add(currentPedido);
                            }
                            break;
                    }
                }
            }

            return pedidos;
        }

        private async Task<Dictionary<string, string>> ParsePedidoHeader(string[] fields)
        {
            var pedido = new Dictionary<string, string>
            {
                //["ID Forma Pagamento"] = fields[4],
                ["Forma Pagamento"] = fields[4],
                ["Data"] = fields[8],
                ["Data Prevista"] = fields[10],
                ["CPF/CNPJ Comprador"] = fields[12],
                ["Número pedido"] = $"{fields[13]}_{sequencia}",
                ["Tipo Frete"] = fields[16] == "Normal" ? "Normal" : "Especial",
                ["Observações"] = fields[17]
            };

            // Consulta CNPJ
            var cnpjInfo = await CNPJConsulta.ConsultarCNPJ(fields[12]);
            if (cnpjInfo != null)
            {
                pedido["Nome Comprador"] = cnpjInfo.Nome;
                pedido["Endereço Comprador"] = cnpjInfo.Logradouro;
                pedido["Número Comprador"] = cnpjInfo.Numero;
                pedido["Complemento Comprador"] = cnpjInfo.Complemento;
                pedido["Bairro Comprador"] = cnpjInfo.Bairro;
                pedido["CEP Comprador"] = cnpjInfo.Cep;
                pedido["Cidade Comprador"] = cnpjInfo.Municipio;
                pedido["UF Comprador"] = cnpjInfo.Uf;
                pedido["Telefone Comprador"] = cnpjInfo.Telefone;
                pedido["E-mail Comprador"] = cnpjInfo.Email;
            }

            return pedido;
        }

        private void ParsePedidoItem(string[] fields, Dictionary<string, string> pedido)
        {
            if (!pedido.ContainsKey("Produtos"))
            {
                pedido["Produtos"] = "";
            }

            var produto = new Dictionary<string, string>
            {
                ["SKU"] = fields[2],
                ["Quantidade"] = fields[3],
                ["Valor Unitário"] = FormatPrice(fields[5]),
                ["Valor Total"] = CalculateTotal(fields[3], fields[5]).Replace(",", "."),
                ["Total Pedido"] = CalculateTotal(fields[3], fields[5]).Replace(",",".")
            };

            pedido["Produtos"] += string.Join("|", produto.Select(kv => $"{kv.Key}:{kv.Value}")) + ";";
        }

        private string FormatPrice(string price)
        {
            return (int.Parse(price) / 100.0).ToString("F2");
        }

        private string CalculateTotal(string _quantity, string _price)
        {
            int quantity = int.Parse(_price);
            int price = int.Parse(_quantity);

            var result = quantity * price / 100.0;

            return result.ToString("F2");
        }
    }
}