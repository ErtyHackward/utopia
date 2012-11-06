using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Settings;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public class BiomeSoundSource : SoundSource,  IBinaryStorable
    {
        public enum TimeOfDaySound
        {
            FullDay = 0,
            Day = 1,
            Night = 2
        }

        #region Private Variables
        #endregion

        #region Public Properties
        [Description("Time of day when the ambient sound can be played"), Category("General")]
        public TimeOfDaySound TimeOfDay { get; set; }
        #endregion

        #region Public Methods
        public override void Save(BinaryWriter writer)
        {
            writer.Write((int)TimeOfDay);

            base.Save(writer);
        }

        public override void Load(BinaryReader reader)
        {
            TimeOfDay = (TimeOfDaySound)reader.ReadInt32();

            base.Load(reader);
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
