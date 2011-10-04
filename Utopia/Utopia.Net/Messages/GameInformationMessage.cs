﻿using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.World.PlanGenerator;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Describes a message used to send game specified information to client
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GameInformationMessage : IBinaryMessage
    {
        /// <summary>
        /// Defines a maximum chunk distance that client can query (in chunks)
        /// </summary>
        private int _maxViewRange;
        /// <summary>
        /// Defines a chunk size used on the server
        /// </summary>
        private Vector3I _chunkSize;

        private int _worldSeed;
        private int _waterLevel;
        private GenerationParameters _planGenerationParameters;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.GameInformation; }
        }

        /// <summary>
        /// Gets or sets a maximum chunk distance that client can query (in chunks)
        /// </summary>
        public int MaxViewRange
        {
            get { return _maxViewRange; }
            set { _maxViewRange = value; }
        }

        /// <summary>
        /// Gets or sets a chunk size used on the server
        /// </summary>
        public Vector3I ChunkSize
        {
            get { return _chunkSize; }
            set { _chunkSize = value; }
        }

        public int WorldSeed
        {
            get { return _worldSeed; }
            set { _worldSeed = value; }
        }
        
        public int WaterLevel
        {
            get { return _waterLevel; }
            set { _waterLevel = value; }
        }
        
        /// <summary>
        /// Contains plan generation details
        /// </summary>
        public GenerationParameters PlanGenerationParameters
        {
            get { return _planGenerationParameters; }
            set { _planGenerationParameters = value; }
        }

        public static GameInformationMessage Read(BinaryReader reader)
        {
            GameInformationMessage gi;

            gi._maxViewRange = reader.ReadInt32();

            gi._chunkSize = reader.ReadVector3I();
            gi._worldSeed = reader.ReadInt32();
            gi._waterLevel = reader.ReadInt32();
            var plan = new GenerationParameters();
            plan.Load(reader);
            gi._planGenerationParameters = plan;
            

            return gi;
        }

        public static void Write(BinaryWriter writer, GameInformationMessage info)
        {
            writer.Write(info._maxViewRange);
            writer.Write(info._chunkSize);
            writer.Write(info._worldSeed);
            writer.Write(info._waterLevel);
            info._planGenerationParameters.Save(writer);
        }
        
        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
