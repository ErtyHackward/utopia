using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.GUI.Forms;
using System.Windows.Forms;
using Utopia.Shared.Config;
using Utopia.Settings;

namespace Utopia
{
    public class UtopiaClient : IDisposable
    {
        #region Private variables
        private static WelcomeScreen _welcomeForm;
        #endregion

        #region Public Properties/Variables
        #endregion

        #region Public Methods
        public void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LoadClientsSettings();
#if STEALTH
            StartDirectXWindow(false);
#else 
            //StartDirectXWindow(true);
            ShowWelcomeScreen(true);
#endif      
        }
        #endregion

        #region Private methods
        private void LoadClientsSettings()
        {
            ClientSettings.Current = new XmlSettingsManager<ClientSettings.ClientConfig>("UtopiaClient.config", SettingsStorage.ApplicationData);
            ClientSettings.Current.Load();
            //If file was not present create a new one with the Azerty Default mapping !
            if (ClientSettings.Current.Settings.KeyboardMapping == null)
            {
                ClientSettings.Current.Settings = ClientSettings.ClientConfig.DefaultQwerty;
                ClientSettings.Current.Save();
            }
        }

        private void ShowWelcomeScreen(bool withFadeIn)
        {  
            _welcomeForm = new WelcomeScreen(withFadeIn);
            _welcomeForm.Text = "Utopia Client Alpha " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            _welcomeForm.ShowDialog();
            if (_welcomeForm.IsDisposed == false)
            {
                AnalyseData(_welcomeForm.Data);
            }
        }

        //User request the Client to enter game
        private void AnalyseData(FormData data)
        {
            if (data == null) return;
            //Close and cleanUP welcome screen;
            _welcomeForm.Close();
            _welcomeForm.Dispose();
            
            switch (data.RequestAction)
            {
                case FormRequestedAction.ExitGame:
                    return;
                case FormRequestedAction.StartSinglePlayer:
                    StartDirectXWindow(data.SinglePlData.NewStruct);
                    break;
                case FormRequestedAction.StartMultiPlayer:
                    break;
            }

            Cursor.Show();
            ShowWelcomeScreen(false);
        }

        //Start the DirectX Render Loop
        private void StartDirectXWindow(bool newStruct)
        {
            using (UtopiaRender main = new UtopiaRender(newStruct))
            {
                main.Run();
            }
        }
        #endregion

        #region CleanUp
        public void Dispose()
        {
        }
        #endregion
    }

    //Sample calling a WPF form from within a class library, in case we use WPF as interface instead of WinForm.
    //thread = new Thread(() =>
    //    {
    //        bw = new BusyWindow();
    //        bw.Show();
    //        bw.Closed += (s, e) => bw.Dispatcher.InvokeShutdown(); 
    //        Dispatcher.Run();
    //    });
    //    thread.SetApartmentState(ApartmentState.STA);
    //    //thread.IsBackground = true;
    //    thread.Start();
}
