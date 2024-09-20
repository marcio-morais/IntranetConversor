using Ookii.Dialogs.Wpf;
using System.IO;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Forms;

namespace intranetConvert_WPF
{
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        private System.Timers.Timer _timer;
        private bool _isHidden = false;

        public MainWindow()
        {
            InitializeComponent();
            CarregarConfiguracoes();
        }

        private void ConfigurarTimer()
        {
            _timer = new System.Timers.Timer(_configuracoes.TempoDeEspera * 1000); // 1000 milissegundos  = 1s
            _timer.Elapsed += Timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_isHidden)
            {
                // Invoque o método btnConverter_Click na thread da UI
                Dispatcher.Invoke(() =>
                {
                    btnConverter_Click(null, null);
                });
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                base.OnClosing(e);
            }
        }

        private void MinimizeToBandeja_Click(object sender, RoutedEventArgs e)
        {
            _isHidden = true;
            this.Hide();
            NotifyIcon.ShowBalloonTip($"Integrador de pedido", $"Modo automático ativado.\nAtualização a cada {_configuracoes.TempoDeEspera} segundos.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);

        }

        private void OpenFromTray_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            _isHidden = false;

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            _isHidden = false;
            if (WindowState == WindowState.Minimized)
            {
                this.Hide();
                NotifyIcon.ShowBalloonTip("Integrador de pedido", $"Modo automático ativado.\nAtualização a cada {_configuracoes.TempoDeEspera} segundos.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                _isHidden = true;
            }
            base.OnStateChanged(e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            NotifyIcon.ShowBalloonTip("IntranetConvert", "Aplicativo minimizado para a bandeja.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }

        private Configuracoes _configuracoes = new Configuracoes();

        private readonly string[] _csvHeader = new[]
        {
            "Número pedido", "Nome Comprador", "Data", "CPF/CNPJ Comprador", "Endereço Comprador",
            "Bairro Comprador", "Numero Comprador", "Complemento Comprador", "CEP Comprador",
            "Cidade Comprador", "UF Comprador", "Telefone Comprador", "Celular Comprador",
            "E-mail Comprador", "Produto", "SKU", "Un", "Quantidade", "Valor Unitario",
            "Valor Total", "Total Pedido", "Valor Frete Pedido", "Valor Desconto Pedido",
            "Outras despesas", "Nome Entrega", "Endereco Entrega", "Numero Entrega",
            "Complemento Entrega", "Cidade Entrega", "UF Entrega", "CEP Entrega",
            "Bairro Entrega", "Transportadora", "Serviço", "Tipo Frete", "Observações",
            "Qtd Parcela", "Data Prevista", "Vendedor", "Forma Pagamento", "ID Forma Pagamento"
        };

        private void BtnBrowseInput_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this) == true)
            {
                TxtInputFile.Text = dialog.SelectedPath;
            }
        }

        private void BtnBrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this) == true)
            {
                TxtOutputFile.Text = dialog.SelectedPath;
            }
        }

        private async void btnConverter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TxtInputFile.Text.Equals("") || TxtOutputFile.Text.Equals(""))
                {
                    if (!_isHidden)
                        System.Windows.MessageBox.Show("Pasta de origem e de destino são obrigatórios.");

                    return;
                }

                string[] arquivosRemessa = Directory.GetFiles(_configuracoes.PastaRemessa, "*.txt");
                if (arquivosRemessa.Length == 0)
                {
                    if (!_isHidden)
                        System.Windows.MessageBox.Show("Nenhum arquivo de remessa encontrado na pasta especificada.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                string nomeArquivoCsv = $"pedidos_FC_{DateTime.Now:yyyyMMdd_HHmmss}";
                string outputFile = Path.Combine(_configuracoes.PastaCSV, $"{nomeArquivoCsv}.csv");

                string pastaProcessados = Path.Combine(_configuracoes.PastaRemessa, "jaProcessados");
                if (!Directory.Exists(pastaProcessados))
                {
                    Directory.CreateDirectory(pastaProcessados);
                }

                var pastaDeArquivoDeRemessaProcessados = Path.Combine(_configuracoes.PastaRemessa + "\\jaProcessados\\", nomeArquivoCsv);
                if (!Directory.Exists(pastaDeArquivoDeRemessaProcessados))
                {
                    Directory.CreateDirectory(pastaDeArquivoDeRemessaProcessados);
                }

                List<Dictionary<string, string>> todosPedidos = new List<Dictionary<string, string>>();

                foreach (string arquivo in arquivosRemessa)
                {
                    var parser = new RemessaParser(arquivo);
                    todosPedidos.AddRange(await parser.ParseRemessa());

                    // Mover o arquivo processado
                    string nomeArquivo = Path.GetFileName(arquivo);
                    string destino = Path.Combine(pastaProcessados, nomeArquivo);
                    File.Move(arquivo, destino);
                }

                ExportToCsv(todosPedidos, outputFile);

                if (!_isHidden)
                    System.Windows.MessageBox.Show($"Conversão concluída. Arquivo CSV gerado: {outputFile}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                if (!_isHidden)
                    System.Windows.MessageBox.Show($"Erro durante a conversão: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private void btnConverter_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (TxtInputFile.Text.Equals("") || TxtOutputFile.Text.Equals(""))
        //        {
        //            if (!_isHidden)
        //                System.Windows.MessageBox.Show("Pasta de origem e de destino são obrigatórios.");

        //            return;
        //        }                

        //        string[] arquivosRemessa = Directory.GetFiles(_configuracoes.PastaRemessa, "*.txt");
        //        if (arquivosRemessa.Length == 0)
        //        {
        //            if (!_isHidden)
        //                System.Windows.MessageBox.Show("Nenhum arquivo de remessa encontrado na pasta especificada.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

        //            return;
        //        }

        //        string nomeArquivoCsv = $"pedidos_FC_{DateTime.Now:yyyyMMdd_HHmmss}";
        //        string outputFile = Path.Combine(_configuracoes.PastaCSV, $"{nomeArquivoCsv}.csv");

        //        string pastaProcessados = Path.Combine(_configuracoes.PastaRemessa, "jaProcessados");
        //        if (!Directory.Exists(pastaProcessados))
        //        {
        //            Directory.CreateDirectory(pastaProcessados);
        //        }

        //        var pastaDeArquivoDeRemessaProcessados = Path.Combine(_configuracoes.PastaRemessa + "\\jaProcessados\\", nomeArquivoCsv);
        //        if (!Directory.Exists(pastaDeArquivoDeRemessaProcessados))
        //        {
        //            Directory.CreateDirectory(pastaDeArquivoDeRemessaProcessados);
        //        }

        //        List<Dictionary<string, string>> todosPedidos = new List<Dictionary<string, string>>();
        //        foreach (string arquivo in arquivosRemessa)
        //        {
        //            var parser = new RemessaParser(arquivo);
        //            todosPedidos.AddRange(parser.ParseRemessa());                    

        //            // Move o arquivo processado para a pasta "jaProcessados"
        //            string nomeArquivo = Path.GetFileName(arquivo);
        //            string destino = Path.Combine(pastaDeArquivoDeRemessaProcessados, nomeArquivo);
        //            File.Move(arquivo, destino);
        //        }

        //        ExportToCsv(todosPedidos, outputFile);

        //        if (!_isHidden)
        //            System.Windows.MessageBox.Show($"Conversão concluída. Arquivo CSV gerado: {outputFile}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (!_isHidden)
        //            System.Windows.MessageBox.Show($"Erro durante a conversão: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void MenuItemConfiguracoes_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new ConfiguracoesWindow(_configuracoes);
            if (configWindow.ShowDialog() == true)
            {
                _configuracoes = configWindow.ConfiguracoesAtualizadas;
                TxtInputFile.Text = _configuracoes.PastaRemessa;
                TxtOutputFile.Text = _configuracoes.PastaCSV;
            }
        }

        private void CarregarConfiguracoes()
        {
            _configuracoes = ConfiguracaoManager.CarregarConfiguracoes();
            if (_configuracoes != null)
            {
                TxtInputFile.Text = _configuracoes.PastaRemessa;
                TxtOutputFile.Text = _configuracoes.PastaCSV;

                ConfigurarTimer();
            }
        }

        private void ExportToCsv(List<Dictionary<string, string>> pedidos, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                writer.WriteLine(string.Join(",", _csvHeader));

                foreach (var pedido in pedidos)
                {
                    if (pedido.TryGetValue("Produtos", out var produtosString))
                    {
                        var produtos = produtosString.Split(';', StringSplitOptions.RemoveEmptyEntries);

                        foreach (var produtoStr in produtos)
                        {
                            var produto = produtoStr.Split('|')
                                .Select(item => item.Split(':'))
                                .ToDictionary(split => split[0], split => split[1]);

                            var linha = new Dictionary<string, string>(pedido);
                            foreach (var kv in produto)
                            {
                                linha[kv.Key] = kv.Value;
                            }

                            // Remover a entrada "Produtos" para não duplicar informações
                            linha.Remove("Produtos");

                            writer.WriteLine(FormatCsvLine(linha));
                        }
                    }
                    else
                    {
                        writer.WriteLine(FormatCsvLine(pedido));
                    }
                }
            }
        }

        private string FormatCsvLine(Dictionary<string, string> data)
        {
            var returno = string.Join(",", _csvHeader.Select(header =>
                data.TryGetValue(header, out var value) ? $"\"{value.Replace("\"", "\"\"")}\"" : "\"\""));

            return returno;
        }

        private string FormatCsvLine(Dictionary<string, string> data, string[] headers)
        {
            return string.Join(",", headers.Select(header =>
                data.ContainsKey(header) ? $"\"{data[header].Replace("\"", "\"\"")}\"" : "\"\""));
        }

    }
}