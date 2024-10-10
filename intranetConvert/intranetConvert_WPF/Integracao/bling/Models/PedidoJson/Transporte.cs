using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace intranetConvert_WPF.Integracao.bling.Models.PedidoJson
{
    public class Transporte
    {
        public int FretePorConta { get; set; }
        public float Frete { get; set; }
        public int QuantidadeVolumes { get; set; }
        public float PesoBruto { get; set; }
        public int PrazoEntrega { get; set; }
        public ContatoTransporte Contato { get; set; }
        public Etiqueta Etiqueta { get; set; }
        public List<Volume> Volumes { get; set; }
    }
}
