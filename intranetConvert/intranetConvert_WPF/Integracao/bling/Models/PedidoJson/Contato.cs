using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace intranetConvert_WPF.Integracao.bling.Models.PedidoJson
{
    public class Contato
    {
        public int Id { get; set; }
        public string TipoPessoa { get; set; }
        public string NumeroDocumento { get; set; }

        public Contato(string _numeroDocumento)
        {
            NumeroDocumento = _numeroDocumento;
            TipoPessoa = "J";
        }
    }
}
