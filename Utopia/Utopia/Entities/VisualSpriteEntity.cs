using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities;
using S33M3Engines.Struct.Vertex;
using Utopia.Shared.Structs;
using SharpDX;
using Utopia.Entities.Interfaces;

namespace Utopia.Entities
{
    public class VisualSpriteEntity : IVisualEntity
    {
        #region Private Variables
        #endregion

        #region Public Variables
        public SpriteEntity SpriteEntity { get; set; }
        public ByteColor color { get; set; }
        public int spriteTextureId;
        #endregion

        public VisualSpriteEntity(SpriteEntity spriteEntity)
        {
            this.SpriteEntity = spriteEntity;
            CreateVertices();
        }

        #region Private methods
        private void CreateVertices()
        {
            //Get Texture Array from Sprite Type
            switch (SpriteEntity.ClassId)
            {
                case EntityClassId.Grass:
                    spriteTextureId = 4;     //5 level of evolution forsee by sprite formula should be (StaticSpriteTextureID * 5) + Evolution.
                    break;
                default:
                    throw new Exception("Static Sprite ID not supported");
            }
        }
        #endregion

        #region Public methods
        #endregion
    }
}
