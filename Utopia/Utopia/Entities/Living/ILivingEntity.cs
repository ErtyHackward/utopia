using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Engines.Struct;
using S33M3Engines.Cameras;
using S33M3Engines.D3D.DebugTools;
using Utopia.Shared.Chunks;

namespace Utopia.Entities.Living
{
    public interface ILivingEntity : IEntity, ICameraPlugin, IDebugInfo
    {
        Vector3 LookAt { get; }
        FTSValue<Quaternion> LookAtDirection { get; }

        float WalkingSpeed { get; set; }
        float FlyingSpeed { get; set; }
        float MoveSpeed { get; set; }
        float MoveRotationSpeed { get; set; }
        float HeadRotationSpeed { get; set; }
        bool HeadInsideWater { get; set; }
        Entities.Living.LivingEntity.RefreshHeadUnderWaterDelegate RefreshHeadUnderWater { get; set; }
    }

    public enum LivingEntityMode
    {
        FreeFirstPerson,
        WalkingFirstPerson
    }
}
