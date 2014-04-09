using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Network;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using S33M3CoreComponents.Inputs.Actions;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Maths;
using S33M3CoreComponents.Inputs;
using Utopia.Shared.Structs;

namespace Utopia.Worlds.GameClocks
{
    public class WorldClock : Clock
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        private float _deltaTime;
        private bool _frozenTime;
        private InputsManager _input;
        private ServerComponent _server;
        private Game _game;
        private UtopiaTime _baseUtopiaTime;
        private DateTime _utopiaLastRealTimeUpdate;
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
        public WorldClock(Game game, InputsManager input, ServerComponent server)
        {
            if (server == null) throw new ArgumentNullException("server");
            _server = server;
            _input = input;
            _game = game;

            AssignTimeAndFactor(server.TimeFactor, server.WorldDateTime);
            _server.MessageDateTime += ServerConnection_MessageDateTime;
            _game.OnRenderLoopFrozen += Game_OnRenderLoopFrozen;

            this.UpdateOrder = 9;
        }

        public override void BeforeDispose()
        {
            _server.MessageDateTime -= ServerConnection_MessageDateTime;
            _game.OnRenderLoopFrozen -= Game_OnRenderLoopFrozen;
        }

        #region Public methods
        public override void Initialize()
        {
            _frozenTime = false;
            base.Initialize();
        }

        public override void FTSUpdate(GameTime timeSpend)
        {
            if (_frozenTime) return;

            _deltaTime = TimeFactor * timeSpend.ElapsedGameTimeInS_LD / (float)UtopiaTime.SecondsPerDay;

            //Back UP previous values
            _clockTime += _deltaTime;

            if (_clockTime > 1)
            {
                //+1 Day
                _clockTime -= 1.0f;
            }

            _visualClockTime.ClockTimeNormalized = _clockTime;
        }
        #endregion

        #region Private methods
        //A freeze did occurded in the client rendering loop, ask server current datetime for syncing.
        private void Game_OnRenderLoopFrozen(float frozenTime)
        {
            _server.ServerConnection.Send(new RequestDateTimeSyncMessage());
        }

        //Synchronize hour with server
        private void ServerConnection_MessageDateTime(object sender, ProtocolMessageEventArgs<DateTimeMessage> e)
        {
            AssignTimeAndFactor(e.Message.TimeFactor, e.Message.DateTime);
            logger.Info("Received Server date time for syncing : {0}, local time was : {1}", e.Message.DateTime, _visualClockTime.ToString());
        }

        private void AssignTimeAndFactor(double timeFactor, UtopiaTime worldDatetime)
        {
            TimeFactor = (float)timeFactor;

            _baseUtopiaTime = worldDatetime;
            _utopiaLastRealTimeUpdate = DateTime.Now;

            _clockTime = _baseUtopiaTime.TimeOfDay.TotalSeconds / (float)UtopiaTime.SecondsPerDay; //Assign number of second in current day / Number of second per day
        }
        #endregion
    }
}

