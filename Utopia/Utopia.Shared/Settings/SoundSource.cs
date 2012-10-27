﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Settings
{
    public partial class SoundSource : IBinaryStorable
    {
        #region Private Variables
        #endregion

        #region Public Properties
        [Description("Sound file path, can be relative one"), Category("General")]
        public string SoundFilePath { get; set; }
        [Description("Sound alias name"), Category("General")]
        public string SoundAlias { get; set; }
        [Description("Sound volume Coef. (1.0 = original file sound volume)"), Category("General")]
        public float DefaultVolume { get; set; }
        #endregion

        #region Public Methods
        public void Save(BinaryWriter writer)
        {
            writer.Write(SoundFilePath);
            writer.Write(SoundAlias);
            writer.Write(DefaultVolume);
        }

        public void Load(BinaryReader reader)
        {
            SoundFilePath = reader.ReadString();
            SoundAlias = reader.ReadString();
            DefaultVolume = reader.ReadSingle();
        }
        #endregion

        public SoundSource()
        {
            DefaultVolume = 1.0f;
        }

        #region Private Methods
        #endregion
    }
}
