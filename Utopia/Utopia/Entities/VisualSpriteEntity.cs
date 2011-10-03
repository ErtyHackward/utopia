using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities;
using S33M3Engines.Struct.Vertex;
using Utopia.Shared.Structs;
using SharpDX;

namespace Utopia.Entities
{
    public class VisualSpriteEntity
    {
        #region Private Variables
        #endregion

        #region Public Variables
        public SpriteEntity SpriteEntity;
        public VertexPositionColorTextureInstanced Vertex;
        #endregion

        public VisualSpriteEntity(SpriteEntity spriteEntity)
        {
            this.SpriteEntity = spriteEntity;
            CreateVertices();
        }

        #region Private methods
        private void CreateVertices()
        {
            //Matrix world = Matrix.Scaling(SpriteEntity.Scale) * Matrix.Translation(SpriteEntity.Position.AsVector3());
            Matrix world = Matrix.Transpose(Matrix.Translation(SpriteEntity.Position.AsVector3()));
            ByteColor color = new ByteColor();
            uint textureArrayId;
            //Get Texture Array from Sprite Type
            switch (SpriteEntity.ClassId)
            {
                case EntityClassId.Grass:
                    textureArrayId = 4;     //5 level of evolution forsee by sprite formula should be (StaticSpriteTextureID * 5) + Evolution.
                    break;
                default:
                    throw new Exception("Static Sprite ID not supported");
            }

            Vertex = new VertexPositionColorTextureInstanced(ref world, ref color, textureArrayId);
        }
        #endregion

        #region Public methods
        #endregion

    }
}
