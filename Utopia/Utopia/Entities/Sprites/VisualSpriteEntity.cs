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
using Utopia.Shared.Settings;
using Utopia.Shared.Entities.Interfaces;

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
            : base(spriteEntity.Size, spriteEntity)
        {
            this.SpriteEntity = spriteEntity;
            Initialize();

            RefreshWorldBoundingBox(SpriteEntity.Position);
        }

        #region Private methods
        private void Initialize()
        {
            EntityProfile profile = GameSystemSettings.Current.Settings.EntityProfile[SpriteEntity.ClassId];

            if (profile.NbrGrowSprites > 0)
            {
                IGrowEntity growableEntity = (IGrowEntity)SpriteEntity;
                spriteTextureId = (profile.SpriteID * profile.NbrGrowSprites) + growableEntity.GrowPhase;
            }
            else
            {
                spriteTextureId = profile.SpriteID;
            }
        }

        #endregion

        #region Public methods
        #endregion
    }
}
