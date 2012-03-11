using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Settings;
using Utopia.Network;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using S33M3_CoreComponents.Inputs.Actions;
using S33M3_DXEngine.Main;
using S33M3_CoreComponents.Maths;
using S33M3_CoreComponents.Inputs;

namespace Utopia.Worlds.GameClocks
{
    public class WorldClock : Clock
    {
        #region Private variables
        private float _deltaTime;
        private bool _frozenTime;
        private InputsManager _input;
        private ServerComponent _server;
        #endregion

        #region Public variable/properties
        public float TimeFactor { get; set; }
        #endregion

        /// <summary>
        /// Control the Game time of teh world
        /// </summary>
        /// <param name="game">Base tools class</param>
        /// <param name="clockSpeed">The Ingame time speed in "Nbr of second ingame for each realtime seconds"; ex : 1 = Real time, 60 = 60times faster than realtime</param>
        /// <param name="gameTimeStatus">The startup time</param>
        public WorldClock(InputsManager input, ServerComponent server)
        {
            if (server == null) throw new ArgumentNullException("server");
            _server = server;
            _input = input;
            AssignTimeAndFactor(server.TimeFactor, server.WorldDateTime);

            _server.ConnectionInitialized += _server_ConnectionInitialized;

            if(_server.ServerConnection != null)
                _server.ServerConnection.MessageDateTime += ServerConnection_MessageDateTime;
        }

        void _server_ConnectionInitialized(object sender, ServerComponentConnectionInitializeEventArgs e)
        {
            if (e.PrevoiusConnection != null)
                e.PrevoiusConnection.MessageDateTime -= ServerConnection_MessageDateTime;

            if(e.ServerConnection != null)
                e.ServerConnection.MessageDateTime += ServerConnection_MessageDateTime;
        }

        #region Public methods
        public override void Initialize()
        {
            _frozenTime = false;
            base.Initialize();
        }

        public override void Dispose()
        {
            _server.ServerConnection.MessageDateTime -= ServerConnection_MessageDateTime;
            base.Dispose();
        }

        public override void Update(GameTime timeSpend)
        {
            InputHandler();

            if (_frozenTime) return;

            _deltaTime = TimeFactor * timeSpend.ElapsedGameTimeInS_LD * (float)Math.PI / 43200.0f;

            //Back UP previous values
            _clockTime.BackUpValue();
            _clockTime.Value += _deltaTime;

            if (_clockTime.Value > Math.PI * 2)
            {
                //+1 Day
                _clockTime.Value = _clockTime.Value - (2 * (float)Math.PI);
            }
            if (_clockTime.Value < Math.PI * -2)
            {
                //-1 Day
                _clockTime.Value = _clockTime.Value + (2 * (float)Math.PI);
            }

            _visualClockTime.Time = _clockTime.Value;
        }

        #endregion

        #region Private methods
        private void InputHandler()
        {
        }

        //Synchronize hour with server
        private void ServerConnection_MessageDateTime(object sender, ProtocolMessageEventArgs<DateTimeMessage> e)
        {
            AssignTimeAndFactor(e.Message.TimeFactor, e.Message.DateTime);
        }

        private void AssignTimeAndFactor(double timeFactor, DateTime worldDatetime)
        {
            TimeFactor = (float)timeFactor;
            int Hour = worldDatetime.Hour;
            int Minute = worldDatetime.Minute;
            int Second = worldDatetime.Second;

            //86400 seconds/day
            _clockTime.Value = ((Second + (Hour * 3600) + (Minute * 60)) / 86400.0f) * MathHelper.TwoPi;
            _clockTime.ValuePrev = base._clockTime.Value;
        }
        #endregion


    }
}

