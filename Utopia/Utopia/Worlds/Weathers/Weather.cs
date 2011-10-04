using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Worlds.Weathers.SharedComp;
using Utopia.Worlds.GameClocks;
using Utopia.Network;

namespace Utopia.Worlds.Weather
{
    public class Weather : GameComponent, IWeather
    {
        #region Private variable
        private Server _server;
        #endregion

        #region Public properties/variable
        public IWind Wind { get; set; }
        public IClock _clock { get; set; }
        #endregion

        public Weather(IClock clock, Server server)
        {
            Wind = new Wind(false);
            _clock = clock;
            _server = server;
            _server.ServerConnection.MessageWeather += ServerConnection_MessageWeather;
        }

        public override void Dispose()
        {
            _server.ServerConnection.MessageWeather -= ServerConnection_MessageWeather;
            Wind.Dispose();
        }

        #region Public Methods
        public override void Initialize()
        {
            Wind.Initialize();
        }

        public override void Update(ref S33M3Engines.D3D.GameTime TimeSpend)
        {
            Wind.Update(ref TimeSpend);
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            Wind.Interpolation(ref interpolation_hd, ref interpolation_ld);
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
