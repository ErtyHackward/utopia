using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities;
using S33M3Engines.Struct.Vertex;
using Utopia.Shared.Structs;

namespace Utopia.Entities
{
    public class VisualSpriteEntity
    {
        #region Private Variables
        #endregion

        #region Public Variables
        public SpriteEntity SpriteEntity;
        public VertexPointSprite Vertex;
        #endregion

        public VisualSpriteEntity(SpriteEntity spriteEntity)
        {
            this.SpriteEntity = spriteEntity;
            CreateVertices();
        }

        #region Private methods
        private void CreateVertices()
        {
            Vertex = new VertexPointSprite(SpriteEntity.Position.AsVector3(), new ByteColor() ,new ByteVector4(4, 1, 0, 0));
        }
        #endregion

        #region Public methods
        #endregion

    }
}
