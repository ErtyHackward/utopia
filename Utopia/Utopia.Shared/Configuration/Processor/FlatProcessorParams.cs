using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Settings;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Configuration
{
    [ProtoContract]
    public class FlatProcessorParams : IBinaryStorable, IProcessorParams
    {
        #region Private Variables
        #endregion

        #region Public Properties
        public WorldConfiguration Config { get; set; }
        #endregion

        #region Public Methods
        public IEnumerable<BlockProfile> InjectDefaultCubeProfiles()
        {
            //Stone Block
            yield return (new BlockProfile()
            {
                Name = "Ground",
                Description = "A ground cube",
                Id = 1,
                Tex_Top = new TextureData("Stone.png"),
                Tex_Bottom = new TextureData("Stone.png"),
                Tex_Back = new TextureData("Stone.png"),
                Tex_Front = new TextureData("Stone.png"),
                Tex_Left = new TextureData("Stone.png"),
                Tex_Right = new TextureData("Stone.png"),
                LightAbsorbed = 255,
                IsPickable = true,
                IsSolidToEntity = true,
                IsBlockingWater = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = true
            });
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

        public static class CubeId
        {
            public const byte Air = 0;
            public const byte Ground = 1;
        }


        public void CreateDefaultValues()
        {
        }
    }
}
