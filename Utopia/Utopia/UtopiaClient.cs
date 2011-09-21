using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.GUI.Forms;
using System.Windows.Forms;
using Utopia.Shared.Config;
using Utopia.Settings;
using Utopia.Network;
using Ninject;
using S33M3Engines.D3D;

namespace Utopia
{
    public class UtopiaClient : IDisposable
    {
        #region Private variables
        private static WelcomeScreen _welcomeForm;
        private Server _server;
        private IKernel _iocContainer;
        #endregion

        #region Public Properties/Variables
        #endregion

        public UtopiaClient()
        {
            //_iocContainer =  new StandardKernel(new NinjectSettings { UseReflectionBasedInjection = true }); ==> More debug infor with this if binding problems, but slower !
        }

        #region Public Methods
        public void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LoadClientsSettings();
#if STEALTH
            StartDirectXWindow();
#else 
            //StartDirectXWindow();
            ShowWelcomeScreen(true);
#endif      
        }
        #endregion

        #region Private methods
        private void LoadClientsSettings()
        {
            ClientSettings.Current = new XmlSettingsManager<ClientConfig>("UtopiaClient.config", SettingsStorage.ApplicationData);
            ClientSettings.Current.Load();
            //If file was not present create a new one with the Azerty Default mapping !
            if (ClientSettings.Current.Settings.KeyboardMapping == null)
            {
                ClientSettings.Current.Settings = ClientConfig.DefaultQwerty;
                ClientSettings.Current.Save();
            }

            ValidateSettings();
        }

        /// <summary>
        /// Do validation on the settings values
        /// </summary>
        private void ValidateSettings()
        {
            if (ClientSettings.Current.Settings.GraphicalParameters.LightPropagateSteps == 0)
            {
                ClientSettings.Current.Settings.GraphicalParameters.LightPropagateSteps = 8;
                ClientSettings.Current.Save();
            }
        }

        private void ShowWelcomeScreen(bool withFadeIn)
        {
            _iocContainer = new StandardKernel();
            _iocContainer.Bind<Server>().ToSelf().InSingletonScope();
            _server = _iocContainer.Get<Server>();

            _welcomeForm = new WelcomeScreen(_server, withFadeIn);
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
            _welcomeForm.CleanUp();
            _welcomeForm.Dispose();
            _welcomeForm = null;
            
            switch (data.RequestAction)
            {
                case FormRequestedAction.ExitGame:
                    return;
                case FormRequestedAction.StartSinglePlayer:
                    _iocContainer.Get<Server>().Connected = false;
                    StartDirectXWindow();
                    break;
                case FormRequestedAction.StartMultiPlayer:
                    _iocContainer.Get<Server>().Connected = true;
                    StartDirectXWindow();
                    break;
            }

            Cursor.Show();
            ShowWelcomeScreen(false);
        }

        private void StartDirectXWindow()
        {
            UtopiaRender main = new UtopiaRender(_iocContainer);
            main.Run();
            //Get windows Exit reason
            GameExitReasonMessage exitRease = main.GameExitReason;
            main.Dispose();

            System.Windows.Forms.Cursor.Show();

            AnalyseExitReason(exitRease);

            _iocContainer.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

        private void AnalyseExitReason(GameExitReasonMessage exitReason)
        {
            if (exitReason.GameExitReason == ExitReason.Error)
            {
                ShowMessage messageDisplayer = new ShowMessage();
                messageDisplayer.Message.Text = exitReason.MainMessage;
                messageDisplayer.MessageDetail.Text = exitReason.DetailedMessage;
                messageDisplayer.ShowDialog();
            }
        }
        #endregion

        #region CleanUp
        public void Dispose()
        {
            if(_iocContainer != null && !_iocContainer.IsDisposed) _iocContainer.Dispose(); // Will also disposed all singleton objects that have been registered !
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
