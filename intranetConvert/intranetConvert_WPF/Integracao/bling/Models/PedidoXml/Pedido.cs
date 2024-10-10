// Models/Order.cs
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace intranetConvert_WPF.Integracao.bling.Models.PedidoXml
{

    public class Pedido
    {
        Date data { get; set; }
        string numero { get; set; }
        public Cliente cliente { get; set; }
        public List<Item> itens { get; set; }
        public List<Parcela> parcelas { get; set; }
        public string obs { get; set; } 
    }
}