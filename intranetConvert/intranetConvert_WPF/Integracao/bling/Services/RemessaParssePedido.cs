using intranetConvert_WPF.Integracao.bling.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace intranetConvert_WPF.Integracao.bling.Services
{

    // Services/FileReaderService.cs
    public class RemessaParssePedido
    {
        private readonly string _inputFile;
        private readonly string _outputFile;

        public RemessaParssePedido(string inputFile)
        {
            _inputFile = inputFile;
        }

        public List<Models.PedidoXml.Pedido> parssePedidoXml()
        {
            string observacoes = "";
            int cont = 0;

            string fileContent;
            EncodingDetector.DetectTextEncoding(_inputFile, out fileContent);
            List<Models.PedidoXml.Pedido> pedidos = new List<Models.PedidoXml.Pedido>();

            using (var reader = new StringReader(fileContent))
            {
                var pedido = new Models.PedidoXml.Pedido();

                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.Split('§');
                    var identifier = fields[0];

                    switch (identifier)
                    {
                        case "22":
                            // Parse order header
                            pedido = new Models.PedidoXml.Pedido
                            {
                                cliente = new Models.PedidoXml.Cliente(),
                                itens = new List<Models.PedidoXml.Item>(),
                                parcelas = new List<Models.PedidoXml.Parcela>()
                            };
                            pedido.cliente = new Models.PedidoXml.Cliente(fields[12]);                            
                            var parcela = new Models.PedidoXml.Parcela
                            {
                                //data = DateTime.Parse(fields[1]),
                                //vlr = decimal.Parse(fields[2]),
                                //obs = fields[3]
                                forma_pagamento = new Models.PedidoXml.Forma_pagamento { id = Convert.ToInt32(fields[4]) }
                            };
                            pedido.parcelas.Add(parcela);
                            break;
                        case "24":
                            // Parse order item
                            var item = new Models.PedidoXml.Item
                            {
                                codigo = fields[2],
                                //descricao = fields[2],
                                //un = fields[3],
                                qtde = int.Parse(fields[3]),
                                vlr_unit = decimal.Parse(fields[5])
                            };
                            pedido.itens.Add(item);
                            break;
                        case "23":
                            if (pedido != null)
                                if (cont > 2)
                                    observacoes += string.Join(" ", fields.Skip(1));
                            cont++;
                            break;
                        case "25":
                            if (pedido != null)
                            {
                                if (!observacoes.Equals(""))
                                    pedido.obs = observacoes.ToString().Trim();

                                pedidos.Add(pedido);
                            }
                            break;
                    }
                }
            }
            return pedidos;
        }

        public List<Models.PedidoJson.Pedido> parssePedidoJson()
        {
            string observacoes = "";
            int cont = 0;

            string fileContent;
            EncodingDetector.DetectTextEncoding(_inputFile, out fileContent);
            List<Models.PedidoJson.Pedido> pedidos = new List<Models.PedidoJson.Pedido>();

            using (var reader = new StringReader(fileContent))
            {
                var pedido = new Models.PedidoJson.Pedido();

                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.Split('§');
                    var identifier = fields[0];

                    switch (identifier)
                    {
                        case "22":
                            // Parse order header
                            pedido = new Models.PedidoJson.Pedido
                            {
                                Parcelas = new List<Models.PedidoJson.Parcela>(),
                                Itens = new List<Models.PedidoJson.Item>(),
                                Data = new DateTime(),
                                Numero = Convert.ToInt32(fields[13])
                            };

                            pedido.Contato = new Models.PedidoJson.Contato(fields[12]);
                            var parcela = new Models.PedidoJson.Parcela
                            {
                                //data = DateTime.Parse(fields[1]),
                                //vlr = decimal.Parse(fields[2]),
                                //obs = fields[3]
                                FormaPagamento = new Models.PedidoJson.FormaPagamento{ Id = Convert.ToInt32(fields[4]) }
                            };

                            pedido.Parcelas.Add(parcela);
                            break;
                        case "24":
                            // Parse order item
                            var item = new Models.PedidoJson.Item
                            {
                                Codigo = fields[2],
                                //descricao = fields[2],
                                //un = fields[3],
                                Quantidade = int.Parse(fields[3]),
                                Valor = float.Parse(fields[5])
                            };
                            pedido.Itens.Add(item);
                            break;
                        case "23":
                            if (pedido != null)
                                if (cont > 2)
                                    observacoes += string.Join(" ", fields.Skip(1));
                            cont++;
                            break;
                        case "25":
                            if (pedido != null)
                            {
                                if (!observacoes.Equals(""))
                                    pedido.Observacoes = observacoes.ToString().Trim();

                                pedidos.Add(pedido);
                            }
                            break;
                    }
                }
            }
            return pedidos;
        }

    }
}


