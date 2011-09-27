using System.IO;
using SharpDX;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message to inform about current weather
    /// </summary>
    public struct WeatherMessage : IBinaryMessage
    {
        private Vector3 _windDirection;

        /// <summary>
        /// Current wind direction and strength in range [-1;1]
        /// </summary>
        public Vector3 WindDirection
        {
            get { return _windDirection; }
            set { _windDirection = value; }
        }

        /// <summary>
        /// Gets a message identification number
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.Weather; }
        }

        public static WeatherMessage Read(BinaryReader reader)
        {
            WeatherMessage msg;

            msg._windDirection = reader.ReadVector3();

            return msg;
        }

        /// <summary>
        /// Writes all necessary instance members
        /// </summary>
        /// <param name="writer"></param>
        public void Write(BinaryWriter writer)
        {
            writer.Write(_windDirection);
        }
    }
}
