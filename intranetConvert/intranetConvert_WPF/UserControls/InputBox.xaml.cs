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

        public InputBox()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            InputText = InputTextBox.Text;
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.DialogResult = true;
                parentWindow.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.DialogResult = false;
                parentWindow.Close();
            }
        }

        private static Window _windowInstance;

        public static string Show(string message, string defaultValue = "")
        {
            // Se a janela já estiver aberta, traga-a para frente
            if (_windowInstance != null && _windowInstance.IsVisible)
            {
                _windowInstance.Activate();
                return ((InputBox)_windowInstance.Content).InputText; // Retorna o texto atual
            }

            InputBox inputBoxControl = new InputBox
            {
                MessageText = { Text = message },
                InputTextBox = { Text = defaultValue }
            };

            _windowInstance = new Window
            {
                Title = "Entrada de Dados",
                Content = inputBoxControl,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.ToolWindow,
                ShowInTaskbar = false,
                Topmost = true
            };

            bool? dialogResult = _windowInstance.ShowDialog();
            string result = dialogResult == true ? inputBoxControl.InputText : null;

            // Limpar a referência após fechar a janela
            _windowInstance = null;

            return result;
        }
    }
}
