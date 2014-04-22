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
using S33M3CoreComponents.Maths;

namespace Utopia.Worlds.Weather
{
    public class Weather : GameComponent, IWeather
    {
        #region Private variable
        private ServerComponent _server;
        private IClock _clock;
        #endregion

        #region Public properties/variable
        public float MoistureOffset { get; set;}
        public float TemperatureOffset { get; set; }
        public IWind Wind { get; set; }
        #endregion

        public Weather(IClock clock, ServerComponent server)
        {
            Wind = new Wind(false);
            MoistureOffset = 0.0f;
            TemperatureOffset = 0.0f;
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

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            Wind.VTSUpdate(interpolationHd, interpolationLd, elapsedTime);
        }
        #endregion 

        #region Private methods
        void ServerConnection_MessageWeather(object sender, ProtocolMessageEventArgs<WeatherMessage> e)
        {
            //Assign new wind direction !
            Wind.WindFlow = e.Message.WindDirection;
            //Keep the offsetting from server into more real range.
            TemperatureOffset = MathHelper.FullLerp(-0.5f, 0.5f, -1, 1, e.Message.TemperatureOffset, true);
            MoistureOffset = MathHelper.FullLerp(-0.5f, 0.5f, -1, 1, e.Message.MoistureOffset, true);
            //TemperatureOffset = e.Message.TemperatureOffset;
            //MoistureOffset = e.Message.MoistureOffset;
        }
        #endregion
    }
}
