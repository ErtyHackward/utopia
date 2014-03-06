using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Server;

namespace Utopia.Shared.Services
{
    [ProtoContract]
    public class WeatherService : Service
    {
        private ServerCore _server;
        private DateTime _lastTime;
        private WeatherMessage _lastMessage;

        [Description("All possible seasons")]
        [ProtoMember(1, OverwriteList = true)]
        public List<Season> Seasons { get; set; }

        [Description("How many days in each season")]
        [ProtoMember(2)]
        public int DaysPerSeason { get; set; }

        /// <summary>
        /// Current day [0; DaysPerSeason)
        /// </summary>
        [Browsable(false)]
        public int Day { get; set; }

        [Browsable(false)]
        public int SeasonIndex { get; set; }

        public WeatherService()
        {
            Seasons = new List<Season>( new [] { 
                new Season { Name = "Summer", Temperature = 1, Moisture = -0.3f}, 
                new Season { Name = "Winter", Temperature = -1, Moisture = 0.7f} 
            });

            DaysPerSeason = 10;
        }

        public override void Initialize(ServerCore server)
        {
            _server = server;
            _server.Scheduler.AddPeriodic(TimeSpan.FromSeconds(1), Tick);
            _server.LoginManager.PlayerAuthorized += LoginManager_PlayerAuthorized;
            

            Day = server.CustomStorage.GetVariable("day", 0);
            SeasonIndex = server.CustomStorage.GetVariable("season", 0);

            if (SeasonIndex >= Seasons.Count)
                SeasonIndex = 0;

            _lastMessage = UpdateWeather();
        }

        void LoginManager_PlayerAuthorized(object sender, Server.Events.ConnectionEventArgs e)
        {
            e.Connection.Send(_lastMessage);
        }

        public override void Dispose()
        {
            _server.CustomStorage.SetVariable("day", Day);
            _server.CustomStorage.SetVariable("season", SeasonIndex);

            _server.LoginManager.PlayerAuthorized -= LoginManager_PlayerAuthorized;
            _server.Scheduler.Remove(Tick);

        }

        private void Tick()
        {
            if (_lastTime > _server.Clock.Now)
            {
                // new day

                if (Day++ >= DaysPerSeason)
                {
                    if (SeasonIndex++ >= Seasons.Count)
                    {
                        // new year
                        SeasonIndex = 0;
                    }

                    // new season
                    Day = 0;
                }

                // each day we will recalculate temperature and humidity values

                _lastMessage = UpdateWeather();

                _server.ConnectionManager.Broadcast(_lastMessage);
            }

            _lastTime = _server.Clock.Now;
        }

        private WeatherMessage UpdateWeather()
        {
            if (Seasons.Count == 0)
                return new WeatherMessage();

            var growing = Day / ( (float)DaysPerSeason / 2 ) <= 1;

            Season s1, s2;
            float power;

            if (growing)
            {
                var s1Index = SeasonIndex - 1 < 0 ? Seasons.Count - 1 : SeasonIndex - 1;

                s1 = Seasons[s1Index];
                s2 = Seasons[SeasonIndex];

                power = Day / ( (float)DaysPerSeason ) + 0.5f;
            }
            else
            {
                var s2Index = SeasonIndex + 1 >= Seasons.Count ? 0 : SeasonIndex + 1;
                s1 = Seasons[SeasonIndex];
                s2 = Seasons[s2Index];

                power = Day / ( (float)DaysPerSeason ) - 0.5f;
            }

            var temperature = Lerp(s1.Temperature, s2.Temperature, power);
            var moisture = Lerp(s1.Moisture, s2.Moisture, power);

            var msg = new WeatherMessage
            {
                MoistureOffset = moisture,
                TemperatureOffset = temperature
            };
            return msg;
        }

        private float Lerp(float start, float end, float val)
        {
            return start + val * (end - start);
        }
    }
}
