using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace intranetConvert_WPF.Integracao.bling.Models.PedidoJson
{
    public class Item
    {
        public float Quantidade { get; set; }
        public float Valor { get; set; }
        public string Descricao { get; set; }
        public string Codigo { get; set; }
        public string Unidade { get; set; }
        public float Desconto { get; set; }
        public float AliquotaIPI { get; set; }
        public string DescricaoDetalhada { get; set; }
        public Produto Produto { get; set; }
        public Comissao Comissao { get; set; }
    }
}
