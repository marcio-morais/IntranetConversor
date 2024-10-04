using intranetConvert_WPF.Integracao.bling.Models.PedidoXml;
using System.Text;

namespace intranetConvert_WPF.Integracao.bling.Services
{
    public class PedidoParseXML
    {
        public string GerarXml(Pedido pedido)
        {
            var xmlBuilder = new StringBuilder();
            xmlBuilder.AppendLine("<?xml version='1.0' encoding='UTF-8'?>");
            xmlBuilder.AppendLine("<pedido>");
            xmlBuilder.AppendLine($"<cliente>" +
                                        $"<nome>{pedido.cliente.nome}</nome>" +
                                        $"<tipoPessoa>{pedido.cliente.tipoPessoa}</tipoPessoa>" +
                                        $"<endereco>{pedido.cliente.endereco}</endereco>" +
                                        $"<cpf_cnpj>{pedido.cliente.cpf_cnpj}</cpf_cnpj>" +
                                        $"<ie>{pedido.cliente.ie}</ie><numero>{pedido.cliente.Number}</numero>" +
                                        $"<complemento>{pedido.cliente.complemento}</complemento>" +
                                        $"<bairro>{pedido.cliente.bairro}</bairro>" +
                                        $"<cep>{pedido.cliente.cep}</cep>" +
                                        $"<cidade>{pedido.cliente.cidade}</cidade>" +
                                        $"<uf>{pedido.cliente.uf}</uf>" +
                                        $"<fone>{pedido.cliente.fone}</fone>" +
                                        $"<email>{pedido.cliente.email}</email>" +
                                  $"</cliente>");

            xmlBuilder.AppendLine("<itens>");
            foreach (var item in pedido.itens)
            {
                xmlBuilder.AppendLine($"<item><codigo>{item.codigo}</codigo><descricao>{item.descricao}</descricao><un>{item.un}</un><qtde>{item.qtde}</qtde><vlr_unit>{item.vlr_unit}</vlr_unit></item>");
            }
            xmlBuilder.AppendLine("</itens>");
            xmlBuilder.AppendLine("<parcelas>");
            foreach (var parcela in pedido.parcelas)
            {
                xmlBuilder.AppendLine($"<parcela>" +
                                        //$"<dias>{parcela.dias}</dias>" +
                                        //$"<data>{parcela.data:dd / MM / yyyy}</data>" +
                                        //$"<vlr>{parcela.vlr}</vlr>" +
                                        //$"<obs>{parcela.obs}</obs>" +
                                        $"<forma_pagamento>" +
                                            $"<id>{parcela.forma_pagamento.id}</id>" +
                                        $"</forma_pagamento>" +
                                      $"</parcela>");
            }
            xmlBuilder.AppendLine("</pedido>");
            return xmlBuilder.ToString();
        }
    }
}
