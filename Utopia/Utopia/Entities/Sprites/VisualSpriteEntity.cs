using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities;
using S33M3Engines.Struct.Vertex;
using Utopia.Shared.Structs;
using SharpDX;
using Utopia.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities.Concrete.Collectible;
using S33M3Engines.Shared.Math;

namespace Utopia.Entities.Sprites
{
    public class VisualSpriteEntity : VisualEntity, IVisualStaticEntity
    {
        #region Private Variables
        #endregion

        #region Public Variables
        public SpriteEntity SpriteEntity { get; set; }
        public ByteColor color { get; set; }
        public int spriteTextureId;
        public Vector3D WorldPosition
        {
            get
            {
                return SpriteEntity.Position;
            }
        }
        #endregion

        public VisualSpriteEntity(SpriteEntity spriteEntity)
            : base(spriteEntity.Size)
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
