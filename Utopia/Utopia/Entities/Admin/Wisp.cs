using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Cameras;
using S33M3Engines.D3D;
using S33M3Engines.InputHandler;
using SharpDX;
using System.Windows.Forms;
using S33M3Engines.Maths;
using S33M3Engines;
using Utopia.Action;
using S33M3Engines.Shared.Math;

namespace Utopia.Entities.Admin
{
    public class Wisp : Entity, IEntity
    {
        ActionsManager _actions;
        Vector3 _entityXAxis, _entityYAxis, _entityZAxis;

        public Wisp(string Name, ICamera camera, ActionsManager actions, DVector3 startUpWorldPosition)
            : base(startUpWorldPosition, new Vector3(0, 0, 0))
        {
            _actions = actions;
        }

        public override void Initialize()
        {
            _entityXAxis = Vector3.UnitX;
            _entityYAxis = Vector3.UnitY;
            _entityZAxis = Vector3.UnitZ;
            
            base.Initialize();
        }

        public override void Update(ref GameTime TimeSpend)
        {
            InputHandler();

            base.Update(ref TimeSpend);
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            InputHandler();

            base.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        private void InputHandler()
        {
        }
    }
}
