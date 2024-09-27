using Ookii.Dialogs.Wpf;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace intranetConvert_WPF
{
    public partial class ConfiguracoesWindow : Window
    {
        public Configuracoes ConfiguracoesAtualizadas { get; private set; }

        public ConfiguracoesWindow(Configuracoes configuracoesAtuais)
        {
            InitializeComponent();
            ConfiguracoesAtualizadas = new Configuracoes
            {
                PastaRemessa = configuracoesAtuais.PastaRemessa,
                PastaCSV = configuracoesAtuais.PastaCSV,
                TempoDeEspera = configuracoesAtuais.TempoDeEspera
            };
            DataContext = ConfiguracoesAtualizadas;
        }

        private void btnSalvar_Click(object sender, RoutedEventArgs e)
        {
            SalvarConfiguracoes();
            DialogResult = true;
            Close();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnSelecionarPastaRemessa_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this) == true)
            {
                ConfiguracoesAtualizadas.PastaRemessa = dialog.SelectedPath;
            }
        }

        private void btnSelecionarPastaCSV_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this) == true)
            {
                ConfiguracoesAtualizadas.PastaCSV = dialog.SelectedPath;
            }
        }

        private void SalvarConfiguracoes()
        {
            XDocument xdoc = new XDocument(
                new XElement("Configuracoes",
                    new XElement("PastaRemessa", ConfiguracoesAtualizadas.PastaRemessa),
                    new XElement("PastaCSV", ConfiguracoesAtualizadas.PastaCSV),
                    new XElement("TempoDeEspera", ConfiguracoesAtualizadas.TempoDeEspera)

                )
            );

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string configPath = Path.Combine(appDataPath, "IntranetConvert");
            Directory.CreateDirectory(configPath);
            string configFile = Path.Combine(configPath, "config.xml");

            xdoc.Save(configFile);
        }

        private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class ConfiguracaoManager
    {
        private static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string ConfigPath = Path.Combine(AppDataPath, "IntranetConvert");
        private static readonly string ConfigFile = Path.Combine(ConfigPath, "config.xml");

        public static Configuracoes CarregarConfiguracoes()
        {
            if (File.Exists(ConfigFile))
            {
                try
                {
                    XDocument doc = XDocument.Load(ConfigFile);
                    if (doc != null)
                        return new Configuracoes
                        {
                            PastaRemessa = doc.Root.Element("PastaRemessa")?.Value ?? "",
                            PastaCSV = doc.Root.Element("PastaCSV")?.Value ?? "",
                            TempoDeEspera = Convert.ToInt32(doc.Root.Element("TempoDeEspera")?.Value)
                        };
                }
                catch (Exception)
                {
                    // Em caso de erro, retorna configurações padrão
                    return new Configuracoes();
                }
            }
            return new Configuracoes();
        }

        public static void SalvarConfiguracoes(Configuracoes config)
        {
            XDocument doc = new XDocument(
                new XElement("Configuracoes",
                    new XElement("PastaRemessa", config.PastaRemessa),
                    new XElement("PastaCSV", config.PastaCSV),
                    new XElement("TempoDeEspera", config.TempoDeEspera)

                )
            );
            doc.Save(ConfigFile);
        }
    }
}