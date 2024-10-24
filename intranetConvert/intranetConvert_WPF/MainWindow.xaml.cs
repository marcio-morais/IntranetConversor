﻿using intranetConvert_WPF.Integracao.bling.Services;
using Ookii.Dialogs.Wpf;
using System.IO;
using System.Text;
using System.Timers;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Data;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using intranetConvert_WPF.UserControls;
using intranetConvert_WPF.Integracao.bling.Models.PedidoJson;
using System.Runtime.ConstrainedExecution;
using NPOI.SS.Formula.Functions;
using intranetConvert_WPF.Resources;


namespace intranetConvert_WPF
{
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        private System.Timers.Timer _timer;
        private bool _isHidden = false;
        private BlingApi blingApi;
        private List<object> todosPedidos = new();
        private static List<CNPJInfo> listaCNPJs = new List<CNPJInfo>();
        private string numeroPedido = "";
        List<string> arquivosRemessaCarregados = new();

        public static string SystemVersion { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            CarregarConfiguracoes();

            SystemVersion = GetSystemVersion();
            DataContext = this;
        }

        private string GetSystemVersion()
        {
            // Lógica para obter a versão do sistema
            return "Sistema de Integração de Pedidos - Eco X First Class (v.1.0.12)"; // Exemplo de versão
        }

        private void ConfigurarTimer()
        {
            //_timer = new System.Timers.Timer(_configuracoes.TempoDeEspera * 1000); // 1000 milissegundos  = 1s
            //_timer.Elapsed += Timer_Elapsed;
            //_timer.AutoReset = true;
            //_timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_isHidden)
            {
                // Invoque o método btnConverter_Click na thread da UI
                Dispatcher.Invoke(() =>
                {
                   // if (_isHidden)
                        //btnConverter_Click(null, null);
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
            //NotifyIcon.ShowBalloonTip($"Integrador de pedido", $"Modo automático ativado.\nAtualização a cada {_configuracoes.TempoDeEspera} segundos.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
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
                //NotifyIcon.ShowBalloonTip("Integrador de pedido", $"Modo automático ativado.\nAtualização a cada {_configuracoes.TempoDeEspera} segundos.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                _isHidden = true;

            }
            else
            {
                try
                {
                    if (_timer != null)
                    {
                        _timer.Stop();
                        _timer.Dispose();
                    }
                }
                catch { }

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
                if (todosPedidos == null)
                    todosPedidos = new List<object>();

                if (todosPedidos.Count() == 0)
                {
                    string[] arquivosRemessa = Directory.GetFiles(_configuracoes.PastaRemessa, "*.txt");
                    if (arquivosRemessa.Length == 0)
                    {
                        if (!_isHidden)
                            System.Windows.MessageBox.Show("Nenhum arquivo de remessa encontrado na pasta especificada.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

                        return;
                    }

                    numeroPedido = InputBox.Show("Informe o número do primeiro pedido a ser gerado.", _configuracoes.UltimoPedido);
                    if (numeroPedido != null)
                    {
                        foreach (string arquivo in arquivosRemessa)
                        {
                            switch (_configuracoes.TipoIntegracao)
                            {
                                case ("API"):
                                    {
                                        var parser = new RemessaParssePedido(arquivo);
                                        todosPedidos.AddRange(parser.parssePedidoJson());
                                    }
                                    break;
                                case ("CSV"):
                                    {
                                        var parser = new RemessaParser(arquivo);
                                        numeroPedido = todosPedidos.Count() > 0 ? ((PedidoCSV)todosPedidos.Last()).NumeroPedido : (Convert.ToInt32(numeroPedido)).ToString();
                                        todosPedidos.AddRange(ProcessarPedidos(await parser.ParseRemessa(Convert.ToInt32(numeroPedido))));
                                    }
                                    break;
                            }

                            if (!arquivosRemessaCarregados.Contains(arquivo))
                                arquivosRemessaCarregados.Add(arquivo);
                        }

                    }
                }

                if (todosPedidos.Count > 0)
                    if (System.Windows.MessageBox.Show($"Gerar os pedidos de {((PedidoCSV)todosPedidos.First()).NumeroPedido} a {((PedidoCSV)todosPedidos.Last()).NumeroPedido}.", "Exportando...", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        if (TxtInputFile.Text.Equals("") || TxtOutputFile.Text.Equals(""))
                        {
                            if (!_isHidden)
                                System.Windows.MessageBox.Show("Pasta de origem e de destino são obrigatórios.");

                            return;
                        }

                        string nomeArquivoCsv = $"pedidos_FC_{DateTime.Now:yyyyMMdd_HHmmss}";
                        string outputFile = Path.Combine(_configuracoes.PastaCSV, $"{nomeArquivoCsv}.csv");

                        string pastaProcessados = Path.Combine(_configuracoes.PastaRemessa, "jaProcessados");
                        if (!Directory.Exists(pastaProcessados))
                            Directory.CreateDirectory(pastaProcessados);

                        var pastaDeArquivoDeRemessaProcessados = Path.Combine(_configuracoes.PastaRemessa + "\\jaProcessados\\", nomeArquivoCsv);
                        if (!Directory.Exists(pastaDeArquivoDeRemessaProcessados))
                            Directory.CreateDirectory(pastaDeArquivoDeRemessaProcessados);

                        switch (_configuracoes.TipoIntegracao)
                        {
                            case ("API"):
                                ExportToApi();
                                break;
                            case ("CSV"):
                                ExportToCsv(outputFile);
                                _configuracoes.UltimoPedido = (todosPedidos.Count() > 0 ? Convert.ToInt32(((PedidoCSV)todosPedidos.Last()).NumeroPedido) + 1 : Convert.ToInt32(_configuracoes.UltimoPedido) + 1).ToString();
                                ConfiguracaoManager.SalvarConfiguracoes(_configuracoes);
                                todosPedidos.Clear();
                                break;
                        }

                        if (!_isHidden)
                            System.Windows.MessageBox.Show($"Conversão concluída. Arquivo CSV gerado: {outputFile}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                        lbQntPedidos.Visibility = Visibility.Collapsed;
                        dtgPedidosList.Visibility = Visibility.Collapsed;
                        btnConverter.Visibility = Visibility.Collapsed;

                        foreach (var arquivo in arquivosRemessaCarregados)
                        {
                            // Mover o arquivo processado
                            string nomeArquivo = Path.GetFileName(arquivo);
                            string destino = Path.Combine(pastaDeArquivoDeRemessaProcessados, nomeArquivo);
                            File.Move(arquivo, destino);
                        }

                        arquivosRemessaCarregados.Clear();
                    }
            }
            catch (Exception ex)
            {
                if (!_isHidden)
                    System.Windows.MessageBox.Show($"Erro durante a conversão: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuContatos_Click(object sender, RoutedEventArgs e)
        {
            var contatosWindow = new ContatosWindow(ref listaCNPJs, _configuracoes);
            contatosWindow.Show();
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

                if (_configuracoes.TipoIntegracao.Equals("API"))
                {
                    webBrowser.Visibility = Visibility.Visible;
                    _ = GetAuthorizationCode();
                }
                else
                {
                    webBrowser.Visibility = Visibility.Collapsed;
                }

                listaCNPJs = CNPJConsulta.LerPlanilhaExcel(_configuracoes.CaminhoConsultaCnpj);
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
            _ = blingApi.ExportToApiAsync(todosPedidos);
        }

        private void ExportToCsv(string outputFile, string a = "")
        {
            using (var writer = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                writer.WriteLine(string.Join(",", RemessaParser._csvHeader));

                foreach (Dictionary<string, string> pedido in todosPedidos)
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

        private void ExportToCsv(string outputFile)
        {
            using (var writer = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                writer.WriteLine(string.Join(",", RemessaParser._csvHeader));
                foreach (PedidoCSV pedido in todosPedidos)
                {
                    foreach (var produto in pedido.Produtos)
                    {
                        var line = new StringBuilder();
                        line.Append($"\"{pedido.NumeroPedido}\",");
                        line.Append($"\"{pedido.NomeComprador}\",");
                        line.Append($"\"{pedido.Data}\",");
                        line.Append($"\"{pedido.CpfCnpjComprador}\",");
                        line.Append($"\"{pedido.EnderecoComprador}\",");
                        line.Append($"\"{pedido.BairroComprador}\",");
                        line.Append($"\"{pedido.NumeroComprador}\",");
                        line.Append($"\"{pedido.ComplementoComprador}\",");
                        line.Append($"\"{pedido.CepComprador}\",");
                        line.Append($"\"{pedido.CidadeComprador}\",");
                        line.Append($"\"{pedido.UfComprador}\",");
                        line.Append($"\"{pedido.TelefoneComprador}\",");
                        line.Append(","); // Celular Comprador
                        line.Append($"\"{pedido.EmailComprador}\",");
                        line.Append(","); // Produto
                        line.Append($"\"{produto.SKU}\",");
                        line.Append(","); // Unidade
                        line.Append($"{produto.Quantidade},");
                        line.Append($"\"{produto.ValorUnitario.ToString()}\",");
                        line.Append($"\"{produto.ValorTotal.ToString()}\",");
                        line.Append($"\"{pedido.Total.ToString()}\",");
                        line.Append(","); // Valor Frete Pedido
                        line.Append(","); // Valor Desconto Pedido
                        line.Append(","); // Outras despesas
                        line.Append(","); // Nome Entrega
                        line.Append(","); // Endereço Entrega
                        line.Append(","); // Número Entrega
                        line.Append(","); // Complemento Entrega
                        line.Append(","); // Cidade Entrega
                        line.Append(","); // UF Entrega
                        line.Append(","); // CEP Entrega
                        line.Append(","); // Bairro Entrega
                        line.Append(","); // Transportadora
                        line.Append(","); // Serviço
                        line.Append($"\"{pedido.TipoFrete}\",");
                        line.Append($"\"{pedido.Observacoes}\",");
                        line.Append(","); // Qtd Parcela
                        line.Append(","); // Data Prevista
                        line.Append(","); // Vendedor
                        line.Append($"\"{pedido.FormaPagamento}\"");
                        // ID Forma Pagamento

                        writer.WriteLine(line.ToString());
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

        private async void btnCarregar_Click(object sender, RoutedEventArgs e)
        {
            if (TxtInputFile.Text.Equals("") || TxtOutputFile.Text.Equals(""))
            {
                if (!_isHidden)
                    System.Windows.MessageBox.Show("Pasta de origem e de destino são obrigatórios.");

                dtgPedidosList.Visibility = Visibility.Collapsed;
                btnConverter.Visibility = Visibility.Collapsed;

                return;
            }

            string[] arquivosRemessa = Directory.GetFiles(_configuracoes.PastaRemessa, "*.txt");
            if (arquivosRemessa.Length == 0)
            {
                if (!_isHidden)
                    System.Windows.MessageBox.Show("Nenhum arquivo de remessa encontrado na pasta especificada.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

                dtgPedidosList.Visibility = Visibility.Collapsed;
                btnConverter.Visibility = Visibility.Collapsed;

                return;
            }

            await carregarTodosPedidosListAsync();

            // Limpa as colunas existentes no DataGrid
            dtgPedidosList.Columns.Clear();

            // Define a fonte de itens como nula para resetar o DataGrid
            dtgPedidosList.ItemsSource = null;

            if (todosPedidos.Count() > 0)
            {
                AddButtonColumn();

                dtgPedidosList.Visibility = Visibility.Visible;
                var view = (CollectionView)CollectionViewSource.GetDefaultView(todosPedidos);
                dtgPedidosList.ItemsSource = view;

                btnConverter.Visibility = Visibility.Visible;
            }
            else
            {
                btnConverter.Visibility = Visibility.Collapsed;
            }
        }

        private void dtgPedidosList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            PropertyDescriptor propertyDescriptor = (PropertyDescriptor)e.PropertyDescriptor;
            e.Column.Header = propertyDescriptor.DisplayName ?? propertyDescriptor.Name;

            // Mover a coluna "Total" para o final
            if (e.PropertyName == "Total")
            {
                e.Column.DisplayIndex = dtgPedidosList.Columns.Count;
                ((DataGridTextColumn)e.Column).Binding.StringFormat = "C2";
            }

            // Exibir a contagem de produtos
            if (e.PropertyName == "Produtos")
            {
                e.Cancel = true; // Cancela a geração automática da coluna
                DataGridTextColumn countColumn = new DataGridTextColumn
                {
                    Header = "Produtos",
                    Binding = new System.Windows.Data.Binding("Produtos.Count")
                };
                dtgPedidosList.Columns.Add(countColumn);
            }
        }

        private async Task<List<object>> carregarTodosPedidosListAsync()
        {
            todosPedidos = new List<object>();

            string nomeArquivoCsv = $"pedidos_FC_{DateTime.Now:yyyyMMdd_HHmmss}";
            string outputFile = Path.Combine(_configuracoes.PastaCSV, $"{nomeArquivoCsv}.csv");

            string[] arquivosRemessa = Directory.GetFiles(_configuracoes.PastaRemessa, "*.txt");

            numeroPedido = InputBox.Show("Informe o número do primeiro pedido a ser gerado.", _configuracoes.UltimoPedido);
            if (numeroPedido != null)
            {
                (new Instancia()).Carregamento("Carregando");

                foreach (string arquivo in arquivosRemessa)
                {
                    switch (_configuracoes.TipoIntegracao)
                    {
                        case ("API"):
                            {
                                var parser = new RemessaParssePedido(arquivo);
                                todosPedidos.AddRange(parser.parssePedidoJson());
                            }
                            break;
                        case ("CSV"):
                            {
                                numeroPedido = todosPedidos.Count() > 0 ? ((PedidoCSV)todosPedidos.Last()).NumeroPedido : (Convert.ToInt32(numeroPedido)).ToString();
                                var parser = new RemessaParser(arquivo);
                                todosPedidos.AddRange(ProcessarPedidos(await parser.ParseRemessa(Convert.ToInt32(numeroPedido))));
                            }
                            break;
                    }

                    if (!arquivosRemessaCarregados.Contains(arquivo))
                        arquivosRemessaCarregados.Add(arquivo);
                }

                Instancia.splashScreen.LoadComplete();

            }

            lbQntPedidos.Content = $"Quantidade de pedidos: {todosPedidos.Count()}";
            lbQntPedidos.Visibility = Visibility.Visible; 

            return todosPedidos;
        }

        private List<PedidoCSV> ProcessarPedidos(List<Dictionary<string, string>> todosPedidos)
        {
            List<PedidoCSV> pedidosProcessados = new List<PedidoCSV>();

            foreach (var pedidoDict in todosPedidos)
            {
                PedidoCSV pedido = new PedidoCSV
                {
                    NumeroPedido = pedidoDict.GetValueOrDefault("Número pedido"),
                    CpfCnpjComprador = pedidoDict.GetValueOrDefault("CPF/CNPJ Comprador"),
                    NomeComprador = pedidoDict.GetValueOrDefault("Nome Comprador"),
                    Data = pedidoDict.GetValueOrDefault("Data"),
                    EmailComprador = pedidoDict.GetValueOrDefault("E-mail Comprador"),
                    TelefoneComprador = pedidoDict.GetValueOrDefault("Telefone Comprador"),
                    DataPrevista = pedidoDict.GetValueOrDefault("Data Prevista"),
                    FormaPagamento = pedidoDict.GetValueOrDefault("Forma Pagamento"),
                    Observacoes = pedidoDict.GetValueOrDefault("Observações"),
                    TipoFrete = pedidoDict.GetValueOrDefault("Tipo Frete"),
                    EnderecoComprador = pedidoDict.GetValueOrDefault("Endereço Comprador"),
                    NumeroComprador = pedidoDict.GetValueOrDefault("Número Comprador"),
                    ComplementoComprador = pedidoDict.GetValueOrDefault("Complemento Comprador"),
                    BairroComprador = pedidoDict.GetValueOrDefault("Bairro Comprador"),
                    CepComprador = pedidoDict.GetValueOrDefault("CEP Comprador"),
                    CidadeComprador = pedidoDict.GetValueOrDefault("Cidade Comprador"),
                    UfComprador = pedidoDict.GetValueOrDefault("UF Comprador"),
                    Produtos = ProcessarProdutos(pedidoDict.GetValueOrDefault("Produtos")),
                };

                pedidosProcessados.Add(pedido);
            }

            return pedidosProcessados;
        }

        private List<ProdutoCSV> ProcessarProdutos(string produtosString)
        {
            List<ProdutoCSV> produtos = new List<ProdutoCSV>();

            if (string.IsNullOrEmpty(produtosString))
                return produtos;

            string[] produtosArray = produtosString.Split(';');

            foreach (string produtoString in produtosArray)
            {
                if (produtoString.Equals(""))
                    continue;

                string[] campos = produtoString.Split('|');
                ProdutoCSV produto = new ProdutoCSV();

                foreach (string campo in campos)
                {
                    string[] partes = campo.Split(':');
                    if (partes.Length == 2)
                    {
                        switch (partes[0])
                        {
                            case "SKU":
                                produto.SKU = partes[1];
                                break;
                            case "Quantidade":
                                produto.Quantidade = int.Parse(partes[1]);
                                break;
                            case "Valor Unitário":
                                produto.ValorUnitario = decimal.Parse(partes[1].Replace(",", "."), CultureInfo.InvariantCulture);
                                break;
                            case "Valor Total":
                                produto.ValorTotal = decimal.Parse(partes[1].Replace(",", "."), CultureInfo.InvariantCulture);
                                break;
                        }
                    }
                }

                produtos.Add(produto);
            }

            return produtos;
        }

        private void AddButtonColumn()
        {
            // Verifica se a coluna já existe
            bool columnExists = false;
            foreach (var column in dtgPedidosList.Columns)
            {
                if (column is DataGridTemplateColumn templateColumn &&
                    templateColumn.Header.ToString() == "::")
                {
                    columnExists = true;
                    break;
                }
            }

            // Se a coluna não existir, adicione-a
            if (!columnExists)
            {
                DataGridTemplateColumn buttonColumn = new DataGridTemplateColumn();
                buttonColumn.Header = "::";

                DataTemplate buttonTemplate = new DataTemplate();
                FrameworkElementFactory buttonFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.Button));
                buttonFactory.SetValue(System.Windows.Controls.Button.ContentProperty, "Ver Itens");
                buttonFactory.AddHandler(System.Windows.Controls.Button.ClickEvent, new RoutedEventHandler(ToggleRowDetails_Click));
                buttonTemplate.VisualTree = buttonFactory;

                buttonColumn.CellTemplate = buttonTemplate;

                dtgPedidosList.Columns.Add(buttonColumn);
            }
        }

        private void ToggleRowDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            var row = DataGridRow.GetRowContainingElement(button);

            if (row != null)
            {
                row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        private void DtgPedidosList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                var row = (DataGridRow)dtgPedidosList.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                {
                    row.DetailsVisibility = Visibility.Collapsed;
                }
            }
        }

        private void DtgPedidosList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.DetailsVisibility = Visibility.Collapsed;
        }

    }

    public class PedidoCSV
    {

        [DisplayName("Número do Pedido")]
        public string NumeroPedido { get; set; }

        [DisplayName("CPF/CNPJ do Comprador")]
        public string CpfCnpjComprador { get; set; }

        [DisplayName("Nome do Comprador")]
        public string NomeComprador { get; set; }

        [DisplayName("E-mail do Comprador")]
        public string EmailComprador { get; set; }

        [DisplayName("Forma de Pagamento")]
        public string FormaPagamento { get; set; }

        [DisplayName("Data")]
        public string Data { get; set; }

        [DisplayName("Data Prevista")]
        public string DataPrevista { get; set; }

        [DisplayName("Observações")]
        public string Observacoes { get; set; }

        [DisplayName("Endereço do Comprador")]
        public string EnderecoComprador { get; set; }

        [DisplayName("Número do Comprador")]
        public string NumeroComprador { get; set; }

        [DisplayName("Complemento do Comprador")]
        public string ComplementoComprador { get; set; }

        [DisplayName("Bairro do Comprador")]
        public string BairroComprador { get; set; }

        [DisplayName("CEP do Comprador")]
        public string CepComprador { get; set; }

        [DisplayName("Cidade do Comprador")]
        public string CidadeComprador { get; set; }

        [DisplayName("UF do Comprador")]
        public string UfComprador { get; set; }



        [DisplayName("Tipo de Frete")]
        public string TipoFrete { get; set; }

        [DisplayName("Telefone do Comprador")]
        public string TelefoneComprador { get; set; }

        [Browsable(false)]
        public List<ProdutoCSV> Produtos { get; set; }

        [DisplayName("Total")]
        public decimal Total => Produtos?.Sum(p => p.ValorTotal) ?? 0;
    }

    public class ProdutoCSV
    {
        public string SKU { get; set; }
        public int Quantidade { get; set; }

        [DisplayName("Valor Unitário")]
        public decimal ValorUnitario { get; set; }

        [DisplayName("Valor Total")]
        public decimal ValorTotal { get; set; }
    }

}