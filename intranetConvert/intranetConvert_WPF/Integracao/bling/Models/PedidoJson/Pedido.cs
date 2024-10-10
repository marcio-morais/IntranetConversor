using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace intranetConvert_WPF.Integracao.bling.Models.PedidoJson
{
    public class Pedido
    {
        public Contato Contato { get; set; }
        public DateTime Data { get; set; }
        public DateTime DataPrevista { get; set; }
        public DateTime DataSaida { get; set; }
        public List<Item> Itens { get; set; }
        public List<Parcela> Parcelas { get; set; }
        public int Numero { get; set; }
        public string NumeroLoja { get; set; }
        public Loja Loja { get; set; }
        public string NumeroPedidoCompra { get; set; }
        public float OutrasDespesas { get; set; }
        public string Observacoes { get; set; }
        public string ObservacoesInternas { get; set; }
        public Desconto Desconto { get; set; }
        public Categoria Categoria { get; set; }
        public Tributacao Tributacao { get; set; }
        public Transporte Transporte { get; set; }
        public Vendedor Vendedor { get; set; }
        public Intermediador Intermediador { get; set; }
        public Taxas Taxas { get; set; }
    }
}
