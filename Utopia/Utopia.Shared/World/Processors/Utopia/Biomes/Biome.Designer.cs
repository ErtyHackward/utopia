using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World.Processors.Utopia.LandformFct;
using Utopia.Shared.Settings;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    /// <summary>
    /// Contains all properties and methods related to Showing Biome information in GridProperties and serializing the informations
    /// </summary>
    public partial class Biome 
    {
        //Should only be used by Editor
        public Biome()
        {
            _config = EditorConfigHelper.Config;
        }
    }
}
