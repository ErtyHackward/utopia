using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Settings;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Configuration
{
    public class FlatProcessorParams : IBinaryStorable, IProcessorParams
    {
        #region Private Variables
        #endregion

        #region Public Properties
        public WorldConfiguration Config { get; set; }
        #endregion

        #region Public Methods
        public IEnumerable<CubeProfile> InjectDefaultCubeProfiles()
        {
            yield break;
        }

        public IEnumerable<IEntity> InjectDefaultEntities()
        {
            yield break;
        }

        public void Save(System.IO.BinaryWriter writer)
        {
        }

        public void Load(System.IO.BinaryReader reader)
        {
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
