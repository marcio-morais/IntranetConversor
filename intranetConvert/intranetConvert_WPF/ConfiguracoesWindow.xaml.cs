using intranetConvert_WPF.Integracao.bling.Models;
using Ookii.Dialogs.Wpf;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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
                TempoDeEspera = configuracoesAtuais.TempoDeEspera,
                TipoIntegracao = configuracoesAtuais.TipoIntegracao,
                ConsultarCNPJ = configuracoesAtuais.ConsultarCNPJ,
                CaminhoConsultaCnpj = configuracoesAtuais.CaminhoConsultaCnpj,
                UltimoPedido = configuracoesAtuais.UltimoPedido,
                ApiBlingConfig = configuracoesAtuais.ApiBlingConfig
                
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
            if (ConfiguracoesAtualizadas.TipoIntegracao.Equals("API"))
            {
                ApiBlingConfig apiBlingConfig = new ApiBlingConfig
                {
                    ClientId = txtApiBlingConfigClientId.Text,
                    ClientSecret = txtApiBlingConfigClientSecret.Text,
                    State = txtApiBlingConfigState.Text,
                    Url = txtApiBlingConfigUrl.Text
                };

                ConfiguracoesAtualizadas.ApiBlingConfig = apiBlingConfig;
            }

            XDocument xdoc = new XDocument(
                new XElement("Configuracoes",
                    new XElement("PastaRemessa", ConfiguracoesAtualizadas.PastaRemessa),
                    new XElement("PastaCSV", ConfiguracoesAtualizadas.PastaCSV),
                    new XElement("TempoDeEspera", ConfiguracoesAtualizadas.TempoDeEspera),
                    new XElement("TipoIntegracao", ConfiguracoesAtualizadas.TipoIntegracao),
                    new XElement("ConsultarCNPJ", ConfiguracoesAtualizadas.ConsultarCNPJ),
                    new XElement("CaminhoConsultaCnpj", ConfiguracoesAtualizadas.CaminhoConsultaCnpj),
                    new XElement("UltimoPedido", ConfiguracoesAtualizadas.UltimoPedido),
                    new XElement("ApiBlingConfig",
                        new XElement("ClientId", ConfiguracoesAtualizadas.ApiBlingConfig.ClientId),
                        new XElement("ClientSecret", ConfiguracoesAtualizadas.ApiBlingConfig.ClientSecret),
                        new XElement("State", ConfiguracoesAtualizadas.ApiBlingConfig.State),
                        new XElement("Url", ConfiguracoesAtualizadas.ApiBlingConfig.Url)
                    )
                )
            );

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string configPath = Path.Combine(appDataPath, "IntranetConvert");
            Directory.CreateDirectory(configPath);
            string configFile = Path.Combine(configPath, "Config.xml");

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

        private void rdbTipoCSV_Checked(object sender, RoutedEventArgs e)
        {
            ConfiguracoesAtualizadas.TipoIntegracao = "CSV";
        }

        private void rdbTipoAPI_Checked(object sender, RoutedEventArgs e)
        {
            ConfiguracoesAtualizadas.TipoIntegracao = "API";
        }
    }

    public static class ConfiguracaoManager
    {
        private static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string ConfigPath = Path.Combine(AppDataPath, "IntranetConvert");
        private static readonly string ConfigFile = Path.Combine(ConfigPath, "Config.xml");

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
                            TempoDeEspera = Convert.ToInt32(doc.Root.Element("TempoDeEspera")?.Value),
                            ConsultarCNPJ = Convert.ToBoolean(doc.Root.Element("ConsultarCNPJ")?.Value),
                            TipoIntegracao = doc.Root.Element("TipoIntegracao")?.Value ?? "",
                            CaminhoConsultaCnpj = doc.Root.Element("CaminhoConsultaCnpj")?.Value ?? "",                            
                            UltimoPedido = doc.Root.Element("UltimoPedido")?.Value ?? "0",
                            ApiBlingConfig = new ApiBlingConfig
                            {
                                ClientId = doc.Root.Element("ApiBlingConfig")?.Element("ClientId")?.Value ?? "",
                                ClientSecret = doc.Root.Element("ApiBlingConfig")?.Element("ClientSecret")?.Value ?? "",
                                State = doc.Root.Element("ApiBlingConfig")?.Element("State")?.Value ?? "",
                                Url = doc.Root.Element("ApiBlingConfig")?.Element("Url")?.Value ?? ""
                            }
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
                new XElement("TempoDeEspera", config.TempoDeEspera),
                new XElement("TipoIntegracao", config.TipoIntegracao),
                new XElement("ConsultarCNPJ", config.ConsultarCNPJ),
                new XElement("CaminhoConsultaCnpj", config.CaminhoConsultaCnpj),
                new XElement("UltimoPedido", config.UltimoPedido),
                new XElement("ApiBlingConfig",
                    new XElement("ClientId", config.ApiBlingConfig.ClientId),
                    new XElement("ClientSecret", config.ApiBlingConfig.ClientSecret),
                    new XElement("State", config.ApiBlingConfig.State),
                    new XElement("Url", config.ApiBlingConfig.Url)
                )
            )
        );
            doc.Save(ConfigFile);
        }
    }
}