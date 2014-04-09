using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using Utopia.Shared.Structs;
using Utopia.Network;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Net.Connections;

namespace Utopia.Worlds.GameClocks
{
    public class ClientClock : GameComponent, IClock
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private/Protected variable
        // Radian angle representing the Period of time inside a day. 0 = Midi, Pi = SunSleep, 2Pi = Midnight, 3/2Pi : Sunrise Morning
        protected float _clockTime;
        private Game _game;
        private VisualClockTime _visualClockTime;
        private ServerComponent _server;
        private bool _frozenTime;
        private float _deltaTime;

        private UtopiaTime _baseUtopiaTime;
        private DateTime _utopiaLastRealTimeUpdate;
        #endregion

        #region Public properties/variables
        public VisualClockTime ClockTime
        {
            get { return _visualClockTime; }
            set { _visualClockTime = value; }
        } 

        public float TimeFactor { get; set; }
        #endregion

        public ClientClock(Game game, ServerComponent server)
        {
            _server = server;
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
        //Allocate resources
        public override void Initialize()
        {
            _frozenTime = false;
            ClockTime = new VisualClockTime();
        }

        //Dispose resources
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
        private void Game_OnRenderLoopFrozen(float frozenTime)
        {
            _server.ServerConnection.Send(new RequestDateTimeSyncMessage());
        }

        //Synchronize hour with server
        private void ServerConnection_MessageDateTime(object sender, ProtocolMessageEventArgs<DateTimeMessage> e)
        {
            AssignTimeAndFactor(e.Message.TimeFactor, e.Message.DateTime);
            logger.Info("Received Server date time for syncing : {0}, local time was : {1}", e.Message.DateTime, ClockTime.ToString());
        }

        private void AssignTimeAndFactor(double timeFactor, UtopiaTime worldDatetime)
        {
            TimeFactor = (float)timeFactor;

            _baseUtopiaTime = worldDatetime;
            _utopiaLastRealTimeUpdate = DateTime.Now;

            _clockTime = _baseUtopiaTime.TimeOfDay.TotalSeconds / (float)UtopiaTime.SecondsPerDay; //Assign number of second in current day / Number of second per day
        }
        #endregion

        public virtual bool ShowDebugInfo { get; set; }
        public virtual string GetDebugInfo()
        {
            return string.Format("<Clock Info> {0} : Normalized Daytime : {1},  Normalized2 Daytime : {2} ", ClockTime.ToString(), ClockTime.ClockTimeNormalized, ClockTime.ClockTimeNormalized2);
        }
    }
}
