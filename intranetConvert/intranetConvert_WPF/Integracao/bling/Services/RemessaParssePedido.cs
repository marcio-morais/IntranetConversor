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

        public List<Pedido> parssePedido()
        {
            string observacoes = "";
            int cont = 0;

            string fileContent;
            EncodingDetector.DetectTextEncoding(_inputFile, out fileContent);
            List<Pedido> pedidos = new List<Pedido>();

            using (var reader = new StringReader(fileContent))
            {
                var pedido = new Pedido();

                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.Split('§');
                    var identifier = fields[0];

                    switch (identifier)
                    {
                        case "22":
                            // Parse order header
                            pedido = new Pedido
                            {
                                cliente = new Cliente(),
                                itens = new List<Item>(),
                                parcelas = new List<Parcela>()
                            };

                            pedido.cliente = new Cliente(fields[12]);
                            var parcela = new Parcela
                            {
                                //data = DateTime.Parse(fields[1]),
                                //vlr = decimal.Parse(fields[2]),
                                //obs = fields[3]
                                forma_pagamento = new Forma_pagamento { id = Convert.ToInt32(fields[4]) }
                            };
                            pedido.parcelas.Add(parcela);
                            break;
                        case "24":
                            // Parse order item
                            var item = new Item
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
    }
}


