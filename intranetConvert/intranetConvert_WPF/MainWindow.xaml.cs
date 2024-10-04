using intranetConvert_WPF.Integracao.bling.Models;
using intranetConvert_WPF.Integracao.bling.Services;
using Ookii.Dialogs.Wpf;
using System.IO;
using System.Text;
using System.Timers;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using intranetConvert_WPF.Integracao.bling.Models.PedidoJson;


namespace intranetConvert_WPF
{
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        private System.Timers.Timer _timer;
        private bool _isHidden = false;
        private BlingApi blingApi;
        private List<Pedido>  todosPedidosApi = new List<Pedido>();

        public MainWindow()
        {
            InitializeComponent();
            //webBrowser.Source = new System.Uri("https://www.bling.com.br/login");
            CarregarConfiguracoes();
            _ = GetAuthorizationCode();
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
            ConfigurarTimer();
            NotifyIcon.ShowBalloonTip($"Integrador de pedido", $"Modo automático ativado.\nAtualização a cada {_configuracoes.TempoDeEspera} segundos.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }

        private void OpenFromTray_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
            try
            {
                _timer.Stop();
                _timer.Dispose();
            }
            catch { }

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

        private Configuracoes _configuracoes = new Configuracoes();

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

                todosPedidosApi = new List<Pedido>();
                var todosPedidosCsv = new List<Dictionary<string, string>>();

                foreach (string arquivo in arquivosRemessa)
                {
                    switch (_configuracoes.TipoIntegracao)
                    {
                        case ("API"):
                            {
                                var parser = new RemessaParssePedido(arquivo);
                                todosPedidosApi.AddRange(parser.parssePedidoJson());
                            }
                            break;
                        case ("CSV"):
                            {
                                var parser = new RemessaParser(arquivo);
                                todosPedidosCsv.AddRange(await parser.ParseRemessa());
                            }
                            break;
                    }

                    // Mover o arquivo processado
                    string nomeArquivo = Path.GetFileName(arquivo);
                    string destino = Path.Combine(pastaDeArquivoDeRemessaProcessados, nomeArquivo);
                    File.Move(arquivo, destino);
                }

                switch (_configuracoes.TipoIntegracao)
                {
                    case ("API"):
                        ExportToApi();
                        break;
                    case ("CSV"):
                        ExportToCsv(todosPedidosCsv, outputFile);
                        break;
                }

                if (!_isHidden)
                    System.Windows.MessageBox.Show($"Conversão concluída. Arquivo CSV gerado: {outputFile}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                if (!_isHidden)
                    System.Windows.MessageBox.Show($"Erro durante a conversão: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItemConfiguracoes_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new ConfiguracoesWindow(_configuracoes);
            if (configWindow.ShowDialog() == true)
                CarregarConfiguracoes();
        }

        private void CarregarConfiguracoes()
        {
            _configuracoes = ConfiguracaoManager.CarregarConfiguracoes();
            if (_configuracoes != null)
            {
                TxtInputFile.Text = _configuracoes.PastaRemessa;
                TxtOutputFile.Text = _configuracoes.PastaCSV;

                grdIntegracaoIntegracaoPorCSV.Visibility = _configuracoes.TipoIntegracao.Equals("CSV") ?
                    Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async Task GetAuthorizationCode()
        {
            blingApi = new BlingApi(_configuracoes.ApiBlingConfig);

            _configuracoes.ApiBlingConfig.State = blingApi.GenerateState();
            string authorizationUrl = blingApi.GetAuthorizationUrl();

            webBrowser.Source = new Uri(authorizationUrl);            
        }

        private void ExportToApi()
        {
            blingApi = new BlingApi(_configuracoes.ApiBlingConfig);
            _ = blingApi.ExportToApiAsync(todosPedidosApi);
        }

        private void ExportToCsv(List<Dictionary<string, string>> pedidos, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                writer.WriteLine(string.Join(",", RemessaParser._csvHeader));

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
            var returno = string.Join(",", RemessaParser._csvHeader.Select(header =>
                data.TryGetValue(header, out var value) ? $"\"{value.Replace("\"", "\"\"")}\"" : "\"\""));

            return returno;
        }

        private string FormatCsvLine(Dictionary<string, string> data, string[] headers)
        {
            return string.Join(",", headers.Select(header =>
                data.ContainsKey(header) ? $"\"{data[header].Replace("\"", "\"\"")}\"" : "\"\""));
        }

        private void MenuItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Show();
            _timer.Stop();
            _timer.Dispose();
            this.WindowState = WindowState.Normal;
            _isHidden = false;
        }

        private async void WebBrowser_Navigated(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
                var uri = new Uri(e.Uri);
            if (uri.AbsoluteUri.StartsWith("https://www.gestoque.com.br/"))
            {
                var query = HttpUtility.ParseQueryString(uri.Query);
                string? code = query["code"];
                string? state = query["state"];

                if (!String.IsNullOrEmpty(state))
                    _configuracoes.ApiBlingConfig.State = state;

                if (!String.IsNullOrEmpty(code))
                    _configuracoes.ApiBlingConfig.Code = code;
                                
                if (!String.IsNullOrEmpty(code))
                  _configuracoes.ApiBlingConfig.Token = await blingApi.GetAccessTokenAsync(_configuracoes.ApiBlingConfig.Code);
            }
        }
    }
}