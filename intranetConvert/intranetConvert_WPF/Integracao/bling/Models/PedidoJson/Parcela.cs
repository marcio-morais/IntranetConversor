using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace intranetConvert_WPF.Integracao.bling.Models.PedidoJson
{
    public class Parcela
    {
        public int Id { get; set; }
        public DateTime DataVencimento { get; set; }
        public float Valor { get; set; }
        public FormaPagamento FormaPagamento { get; set; }
        public string Observacoes { get; set; }
    }
}
