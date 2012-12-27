using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Worlds.Weathers.SharedComp;
using Utopia.Worlds.GameClocks;
using Utopia.Network;
using S33M3DXEngine.Main;

namespace Utopia.Worlds.Weather
{
    public class Weather : GameComponent, IWeather
    {
        #region Private variable
        private ServerComponent _server;
        private IClock _clock;
        #endregion

        #region Public properties/variable
        public IWind Wind { get; set; }
        #endregion

        public Weather(IClock clock, ServerComponent server)
        {
            Wind = new Wind(false);
            _clock = clock;
            _server = server;
            _server.MessageWeather += ServerConnection_MessageWeather;
        }

        public override void BeforeDispose()
        {
            _server.MessageWeather -= ServerConnection_MessageWeather;
            Wind.Dispose();
        }

        #region Public Methods
        public override void Initialize()
        {
            Wind.Initialize();
        }

        public override void FTSUpdate(GameTime timeSpend)
        {
            Wind.FTSUpdate(timeSpend);
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, long timePassed)
        {
            Wind.VTSUpdate(interpolationHd, interpolationLd, timePassed);
        }
        #endregion 

        #region Private methods
        void ServerConnection_MessageWeather(object sender, ProtocolMessageEventArgs<WeatherMessage> e)
        {
            //Assign new wind direction !
            Wind.WindFlow = e.Message.WindDirection;
        }
        #endregion
    }
}
