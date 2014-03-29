using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Server.Structs
{
    /// <summary>
    /// Handles npc head rotation (where the npc look at)
    /// </summary>
    public class FocusAI
    {
        private readonly ServerNpc _parentNpc;
        private int _checkCounter = 0;

        private IDynamicEntity _target;

        public ServerNpc Npc { get { return _parentNpc; } }

        public FocusAI(ServerNpc parentNpc)
        {
            _parentNpc = parentNpc;
            _checkCounter = _parentNpc.Random.Next(0, 20);
        }

        public void Update(DynamicUpdateState gameTime)
        {
            {
                if (_target != null)
                {
                    if (Vector3D.Distance(_target.Position, Npc.DynamicEntity.Position) < 10)
                    {
                        var lookDirection = _target.Position - Npc.DynamicEntity.Position;
                        lookDirection.Normalize();
                        Npc.DynamicEntity.HeadRotation = Quaternion.RotationMatrix(Matrix.LookAtLH(Npc.DynamicEntity.Position.AsVector3(), Npc.DynamicEntity.Position.AsVector3() + lookDirection.AsVector3(), Vector3D.Up.AsVector3()));
                    }
                    else
                    {
                        _target = null;
                    }
                }
                else
                {
                    if (_checkCounter++ > 10)
                    {
                        _checkCounter = 0;
                        // try to find target
                        //Npc.Server.AreaManager.Find(area =>
                        //                   {
                        //                       foreach (var serverEntity in area.Enumerate())
                        //                       {
                        //                           if (serverEntity.GetType() != this.GetType() &&
                        //                               Vector3D.Distance(serverEntity.DynamicEntity.Position, Npc.DynamicEntity.Position) < 10)
                        //                           {
                        //                               _target = serverEntity.DynamicEntity;
                        //                               return true;
                        //                           }
                        //                       }
                        //                       return false;
                        //                   });
                    }
                }
            }
        }
        
        public void LookAt(Vector3D pos)
        {
            var lookDirection = pos - Npc.DynamicEntity.Position;
            lookDirection.Normalize();
            Npc.DynamicEntity.HeadRotation = Quaternion.RotationMatrix(Matrix.LookAtLH(Npc.DynamicEntity.Position.AsVector3(), Npc.DynamicEntity.Position.AsVector3() + lookDirection.AsVector3(), Vector3D.Up.AsVector3()));
        }
    }
}