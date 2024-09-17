using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using Microsoft.Win32;
using System.Xml.Serialization;
using Ookii.Dialogs.Wpf;

namespace intranetConvert_WPF
{
    public partial class MainWindow : Window
    {
        private Configuracoes _configuracoes = new Configuracoes();

        private readonly string[] _csvHeader = new[]
        {
            "Numero pedido", "Nome Comprador", "Data", "CPF/CNPJ Comprador", "Endereco Comprador",
            "Bairro Comprador", "Numero Comprador", "Complemento Comprador", "CEP Comprador",
            "Cidade Comprador", "UF Comprador", "Telefone Comprador", "Celular Comprador",
            "E-mail Comprador", "Produto", "SKU", "Un", "Quantidade", "Valor Unitario",
            "Valor Total", "Total Pedido", "Valor Frete Pedido", "Valor Desconto Pedido",
            "Outras despesas", "Nome Entrega", "Endereco Entrega", "Numero Entrega",
            "Complemento Entrega", "Cidade Entrega", "UF Entrega", "CEP Entrega",
            "Bairro Entrega", "Transportadora", "Servico", "Tipo Frete", "Observacoes",
            "Qtd Parcela", "Data Prevista", "Vendedor", "Forma Pagamento", "ID Forma Pagamento"
        };

        public MainWindow()
        {
            InitializeComponent();
            CarregarConfiguracoes();
        }

        private void BtnBrowseInput_Click(object sender, RoutedEventArgs e)
        {

            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this) == true)
            {
                TxtInputFile.Text= dialog.SelectedPath;
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

        private void btnConverter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TxtInputFile.Text.Equals("") || TxtOutputFile.Text.Equals(""))
                {
                    MessageBox.Show("Pasta de origem e de destino são obrigatórios.");
                    return;
                }

                string[] arquivosRemessa = Directory.GetFiles(_configuracoes.PastaRemessa, "*.txt");
                if (arquivosRemessa.Length == 0)
                {
                    MessageBox.Show("Nenhum arquivo de remessa encontrado na pasta especificada.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string outputFile = Path.Combine(_configuracoes.PastaCSV, $"pedidos_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                List<Dictionary<string, string>> todosPedidos = new List<Dictionary<string, string>>();

                foreach (string arquivo in arquivosRemessa)
                {
                    var parser = new RemessaParser(arquivo);
                    todosPedidos.AddRange(parser.ParseRemessa());
                }

                ExportToCsv(todosPedidos, outputFile);

                MessageBox.Show($"Conversão concluída. Arquivo CSV gerado: {outputFile}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro durante a conversão: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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
