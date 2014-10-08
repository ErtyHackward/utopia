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
        protected double _clockTime;
        private Game _game;
        private VisualClockTime _visualClockTime;
        private ServerComponent _server;
        private bool _frozenTime;
        private double _deltaTime;

        private UtopiaTime _baseUtopiaTime;
        private DateTime _utopiaLastRealTimeUpdate;
        #endregion

        #region Public properties/variables
        public float TimeFactor { get; set; }

        public VisualClockTime ClockTime
        {
            get { return _visualClockTime; }
            set { _visualClockTime = value; }
        }

        public UtopiaTime Now
        {
            get
            {
                var realTimeDiff = DateTime.Now - _utopiaLastRealTimeUpdate;
                return _baseUtopiaTime + UtopiaTimeSpan.FromSeconds(realTimeDiff.TotalSeconds * TimeFactor);
            }
        }

        #endregion

        public ClientClock(Game game, ServerComponent server)
        {
            _server = server;
            _game = game;

            AssignTimeAndFactor(server.TimeFactor, server.WorldDateTime);
            _server.MessageDateTime += ServerConnection_MessageDateTime;
            _game.OnRenderLoopFrozen += Game_OnRenderLoopFrozen;
            ShowDebugInfo = true;
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
            FrozenTime = false;
            ClockTime = new VisualClockTime();
        }

        public override void FTSUpdate(GameTime timeSpend)
        {
            if (FrozenTime) return;
            if (_clockTime == -1)
            {
                _clockTime = Now.TimeOfDay.TotalSeconds / (float)UtopiaTime.SecondsPerDay; //Assign number of second in current day / Number of second per day
            }

            _deltaTime = TimeFactor * timeSpend.ElapsedGameTimeInS_HD / (double)UtopiaTime.SecondsPerDay;

            //Back UP previous values
            _clockTime += _deltaTime;

            if (_clockTime > 1)
            {
                //+1 Day
                _clockTime -= 1.0f;
            }

            _visualClockTime.ClockTimeNormalized = (float)_clockTime;
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

            _clockTime = -1;
            var prevFrozen = FrozenTime;
            FrozenTime = false;
            FTSUpdate(new GameTime());
            FrozenTime = prevFrozen;

            float y = (float)Math.Cos(ClockTime.ClockTimeNormalized * MathHelper.TwoPi);
            float x = (float)Math.Sin(ClockTime.ClockTimeNormalized * MathHelper.TwoPi);
            
            logger.Info("SunLight Vector is {0}", new SharpDX.Vector3(x, y, 0));
        }
        #endregion

        public virtual bool ShowDebugInfo { get; set; }

        public bool FrozenTime
        {
            get { return _frozenTime; }
            set { _frozenTime = value; }
        }

        public virtual string GetDebugInfo()
        {
            return string.Format("<Clock Info> {0}", Now.ToString());
        }
    }
}
