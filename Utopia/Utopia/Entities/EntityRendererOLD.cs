using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.StatesManager;

namespace Utopia.Entities
{
    public class EntityRendererOLD : DrawableGameComponent
    {
        #region Private variables
        #endregion

        #region Public variables/properties
        public List<IEntity> Entities = new List<IEntity>();
        #endregion

        public EntityRendererOLD()
        {
        }

        #region Public Methods
        public override void Initialize()
        {
        }

        public override void LoadContent()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].LoadContent();
            }
        }

        public override void UnloadContent()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].UnloadContent();
            }
        }

        public override void Update(ref GameTime TimeSpend)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].Update(ref TimeSpend);
            }
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].Interpolation(ref interpolation_hd, ref interpolation_ld);
            }
        }

        public override void Draw()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].Draw();
            }
        }
        #endregion

        #region Private Methods
        #endregion        
    }
}
