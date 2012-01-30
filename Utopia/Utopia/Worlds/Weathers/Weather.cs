﻿using System;
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

            _server.ConnectionInitialized += _server_ConnectionInitialized;

            if(_server.ServerConnection != null)
                _server.ServerConnection.MessageWeather += ServerConnection_MessageWeather;
        }

        void _server_ConnectionInitialized(object sender, ServerComponentConnectionInitializeEventArgs e)
        {
            if (e.PrevoiusConnection != null)
                e.PrevoiusConnection.MessageWeather -= ServerConnection_MessageWeather;

            if (e.ServerConnection != null)
                e.ServerConnection.MessageWeather += ServerConnection_MessageWeather;
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

        public override void Update(ref S33M3Engines.D3D.GameTime timeSpend)
        {
            Wind.Update(ref timeSpend);
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld, ref long timePassed)
        {
            Wind.Interpolation(ref interpolation_hd, ref interpolation_ld, ref timePassed);
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
