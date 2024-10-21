using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace intranetConvert_WPF.Resources
{
    /// <summary>
    /// Lógica interna para SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window, ISplashScreen
    {
        public SplashScreen()
        {
            InitializeComponent();
            // Ajuste o tamanho da janela para o tamanho da área de trabalho
            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;
            this.Left = SystemParameters.WorkArea.Left;
            this.Top = SystemParameters.WorkArea.Top;
        }

        public SplashScreen(string mensagem)
        {
            this.Topmost = false;
            try
            {
                InitializeComponent();
                messageText.Text = mensagem;

                this.Width = SystemParameters.WorkArea.Width;
                this.Height = SystemParameters.WorkArea.Height;
                this.Left = SystemParameters.WorkArea.Left;
                this.Top = SystemParameters.WorkArea.Top;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void LoadComplete()
        {
            try
            {
                Dispatcher.InvokeShutdown();
                Mouse.OverrideCursor = null;
                Owner.Focus();
            }
            catch { }
        }
    }

    public interface ISplashScreen
    {
        void LoadComplete();
    }

    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double originalValue && parameter is string percentageString && double.TryParse(percentageString, out double percentage))
            {
                return originalValue * percentage / 100;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
