using ProtoBuf;
using SharpDX;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message to inform about current weather
    /// </summary>
    [ProtoContract]
    public class WeatherMessage : IBinaryMessage
    {
        /// <summary>
        /// Current wind direction and strength in range [-1;1]
        /// </summary>
        [ProtoMember(1)]
        public Vector3 WindDirection { get; set; }

        /// <summary>
        /// Current world temperature offset in max range [-1;1]
        /// </summary>
        [ProtoMember(2)]
        public float TemperatureOffset { get; set; }

        /// <summary>
        /// Current world moisture offset in max range [-1;1]
        /// </summary>
        [ProtoMember(3)]
        public float MoistureOffset { get; set; }

        /// <summary>
        /// Gets a message identification number
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.Weather; }
        }
    }
}
