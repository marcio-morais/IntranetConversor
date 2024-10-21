using System.Windows.Input;

namespace intranetConvert_WPF.Resources
{
    public partial class Instancia
    {
        public static ISplashScreen splashScreen;
        private ManualResetEvent ResetSplashCreated;
        private string txtSplash;

        // Exibe a splashscreen de Carregamento
        private void ShowSplash()
        {
            SplashScreen spl = new SplashScreen(txtSplash) { Topmost = false };
            splashScreen = spl;
            spl.Show();

            ResetSplashCreated.Set();
            System.Windows.Threading.Dispatcher.Run();
        }

        /// <summary>
        /// Cria a thread para exibição da splashscreeen
        /// </summary>
        public void Carregamento(string texto = "Carregando", string nome = "Carregando")// Cria a thread para exibição da splashscreen
        {
            try
            {
                if (Threads.SplashThread == null || (Threads.SplashThread != null && Threads.SplashThread.ThreadState == ThreadState.Stopped))
                {
                    ResetSplashCreated = new ManualResetEvent(false);
                    Mouse.OverrideCursor = Cursors.Wait;
                    txtSplash = texto;

                    // Cria uma nova thread para a splash screen
                    Threads.SplashThread = new Thread(ShowSplash);
                    Threads.SplashThread.SetApartmentState(ApartmentState.STA);
                    Threads.SplashThread.IsBackground = true;
                    Threads.SplashThread.Name = nome;
                    Threads.SplashThread.Start();
                    ResetSplashCreated.WaitOne();
                }
            }
            catch { }
        }
    }
}
