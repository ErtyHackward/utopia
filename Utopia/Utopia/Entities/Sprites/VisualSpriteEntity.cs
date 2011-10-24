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
            Initialize();

            RefreshWorldBoundingBox(SpriteEntity.Position);
        }

        #region Private methods
        private void Initialize()
        {
            //Assign Texture Array from Sprite Type
            switch (SpriteEntity.ClassId)
            {
                case EntityClassId.Grass:
                    Grass grassEntity = (Grass)SpriteEntity;
                    spriteTextureId = (0 * 5) + grassEntity.GrowPhase ;     //5 level of evolution forsee by sprite formula should be (StaticSpriteTextureID * 5) + Evolution.
                    break;
                case EntityClassId.Flower1:
                    spriteTextureId = 7;
                    break;
                case EntityClassId.Flower2:
                    spriteTextureId = 8;
                    break;
                case EntityClassId.Mushroom1:
                    spriteTextureId = 5;
                    break;
                case EntityClassId.Mushroom2:
                    spriteTextureId = 6;
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
