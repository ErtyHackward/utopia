using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct.Vertex;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete.Collectible;
using Utopia.Shared.Structs;
using SharpDX;
using S33M3Engines.Shared.Math;

namespace Utopia.Entities.Sprites
{
    public class VisualSpriteEntity : VisualEntity
    {
        #region Private Variables
        #endregion

        #region Public Variables
        public SpriteEntity SpriteEntity { get; set; }
        public int spriteTextureId;
        #endregion

        public VisualSpriteEntity(SpriteEntity spriteEntity)
            : base(spriteEntity.Size, spriteEntity.Scale, spriteEntity)
        {
            this.SpriteEntity = spriteEntity;
            CreateVertices();

            RefreshWorldBoundingBox(SpriteEntity.Position);
        }

        #region Private methods
        private void CreateVertices()
        {
            //Get Texture Array from Sprite Type
            switch (SpriteEntity.ClassId)
            {
                case EntityClassId.Grass:
                    Grass grassEntity = (Grass)SpriteEntity;
                    spriteTextureId = (0 * 5) + grassEntity.GrowPhase ;     //5 level of evolution forsee by sprite formula should be (StaticSpriteTextureID * 5) + Evolution.
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
