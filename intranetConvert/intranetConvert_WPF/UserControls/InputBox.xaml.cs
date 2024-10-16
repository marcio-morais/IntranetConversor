using intranetConvert_WPF.UserControls;
using Microsoft.VisualBasic.ApplicationServices;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace intranetConvert_WPF.UserControls
{
    public partial class InputBox : UserControl
    {
        public string InputText { get; private set; }
        public bool DialogResult { get; private set; }

        public InputBox()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            InputText = InputTextBox.Text;
            DialogResult = true;
            ((Window)this.Parent).Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            ((Window)this.Parent).Close();
        }

        public static string Show(string message, string defaultValue = "")
        {
            Window window = new Window
            {
                Title = "Entrada de Dados",
                Content = new InputBox { MessageText = { Text = message }, InputTextBox = { Text = defaultValue } },
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.ToolWindow
            };

            if (window.ShowDialog() == true)
            {
                return ((InputBox)window.Content).InputText;
            }

            return null;
        }
    }
}
