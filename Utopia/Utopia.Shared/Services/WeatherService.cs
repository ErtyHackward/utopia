using System;
using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Server;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Services
{
    [ProtoContract]
    public class WeatherService : Service
    {
        private ServerCore _server;
        private WeatherMessage _lastMessage;
        
        /// <summary>
        /// This field is actually UtopiaTime.TimeConfiguration singleton
        /// </summary>
        [ProtoMember(3)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public UtopiaTimeConfiguration TimeConfiguration
        {
            get { return UtopiaTime.TimeConfiguration; }
            set {

                if (value.Seasons == null)
                    return;

                UtopiaTime.TimeConfiguration = value;
            }
        }

        /// <summary>
        /// Current day [0; DaysPerSeason)
        /// </summary>
        [Browsable(false)]
        public UtopiaTime CurrentDate { get; set; }
        
        /// <summary>
        /// Gets current season or null if no seasons exists
        /// </summary>
        [Browsable(false)]
        public Season CurrentSeason {
            get { return TimeConfiguration.Seasons.Count > 0 ? TimeConfiguration.Seasons[CurrentDate.SeasonIndex] : null; }
        }

        public WeatherService()
        {
            TimeConfiguration = new UtopiaTimeConfiguration();
        }

        public override void Initialize(ServerCore server)
        {
            _server = server;
            _server.LoginManager.PlayerAuthorized += LoginManager_PlayerAuthorized;

            server.Clock.ClockTimers.Add(new Clock.GameClockTimer(UtopiaTimeSpan.FromDays(1), server.Clock, PerDayTrigger));

            CurrentDate = server.CustomStorage.GetVariable("date", new UtopiaTime());
            
            _lastMessage = UpdateWeather();
        }

        void LoginManager_PlayerAuthorized(object sender, Server.Events.ConnectionEventArgs e)
        {
            e.Connection.Send(_lastMessage);
        }

        public override void Dispose()
        {
            _server.CustomStorage.SetVariable("date", CurrentDate);
            _server.LoginManager.PlayerAuthorized -= LoginManager_PlayerAuthorized;
        }

        private void PerDayTrigger(UtopiaTime gametime)
        {
            CurrentDate += UtopiaTimeSpan.FromDays(1);

            // each day we will recalculate temperature and humidity values
            _lastMessage = UpdateWeather();
            _server.ConnectionManager.Broadcast(_lastMessage);
        }

        private WeatherMessage UpdateWeather()
        {
            if (TimeConfiguration.Seasons.Count == 0)
                return new WeatherMessage();

            var growing = CurrentDate.Day / ((float)TimeConfiguration.DaysPerSeason / 2) <= 1;

            Season s1, s2;
            float power;

            var seasonIndex = CurrentDate.SeasonIndex;
            
            if (growing)
            {
                var s1Index = seasonIndex - 1 < 0 ? TimeConfiguration.Seasons.Count - 1 : seasonIndex - 1;
                s1 = TimeConfiguration.Seasons[s1Index];
                s2 = TimeConfiguration.Seasons[seasonIndex];

                power = CurrentDate.Day / ((float)TimeConfiguration.DaysPerSeason) + 0.5f;
            }
            else
            {
                var s2Index = seasonIndex + 1 >= TimeConfiguration.Seasons.Count ? 0 : seasonIndex + 1;
                s1 = TimeConfiguration.Seasons[seasonIndex];
                s2 = TimeConfiguration.Seasons[s2Index];

                power = CurrentDate.Day / ((float)TimeConfiguration.DaysPerSeason) - 0.5f;
            }

            var temperature = Lerp(s1.Temperature, s2.Temperature, power);
            var moisture    = Lerp(s1.Moisture,    s2.Moisture,    power);

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
