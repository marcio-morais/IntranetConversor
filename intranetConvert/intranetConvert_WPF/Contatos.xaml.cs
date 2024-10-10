using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NPOI.HSSF.UserModel; // Para arquivos .xls
using NPOI.SS.UserModel; // Interface comum para células, linhas, etc.
using NPOI.XSSF.UserModel; // Para arquivos .xlsx

namespace intranetConvert_WPF
{
    /// <summary>
    /// Lógica interna para Contatos.xaml
    /// </summary>
    public partial class ContatosWindow : Window
    {
        private static Configuracoes configuracoes;
        private static List<CNPJInfo> listaCNPJs;

        public ContatosWindow(ref List<CNPJInfo> _listaCNPJs, Configuracoes _configuracoes)
        {
            InitializeComponent();

            // Carrega os dados da planilha ao abrir a janela
            configuracoes = _configuracoes;
            listaCNPJs = _listaCNPJs;

            dataGridContatos.ItemsSource = listaCNPJs;
        }

        private static void AtualizarDadosPlanilha()
        {
            listaCNPJs = CNPJConsulta.LerPlanilhaExcel(configuracoes.CaminhoConsultaCnpj);
        }
    }
}
