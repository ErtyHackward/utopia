using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Server.Structs
{
    /// <summary>
    /// Handles npc head rotation (where the npc looks at)
    /// </summary>
    public class FocusAI
    {
        private readonly ServerNpc _parentNpc;

        private IEntity _target;

        public ServerNpc Npc { get { return _parentNpc; } }

        public IEntity Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public FocusAI(ServerNpc parentNpc)
        {
            _parentNpc = parentNpc;
        }

        public void Update(DynamicUpdateState gameTime)
        {
            {
                if (Target != null)
                {
                    var lookDirection = Target.Position - Npc.DynamicEntity.Position;
                    lookDirection.Normalize();
                    Npc.DynamicEntity.HeadRotation = Quaternion.RotationMatrix(Matrix.LookAtLH(Npc.DynamicEntity.Position.AsVector3(), Npc.DynamicEntity.Position.AsVector3() + lookDirection.AsVector3(), Vector3D.Up.AsVector3()));
                }
            }
        }
        
        public void LookAt(Vector3D pos)
        {
            Target = null;
            var lookDirection = pos - Npc.DynamicEntity.Position;
            lookDirection.Normalize();
            Npc.DynamicEntity.HeadRotation = Quaternion.RotationMatrix(Matrix.LookAtLH(Npc.DynamicEntity.Position.AsVector3(), Npc.DynamicEntity.Position.AsVector3() + lookDirection.AsVector3(), Vector3D.Up.AsVector3()));
        }

        public void LookAt(IEntity entity)
        {
            Target = entity;
        }
    }
}