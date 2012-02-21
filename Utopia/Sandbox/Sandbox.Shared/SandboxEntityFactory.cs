using Sandbox.Shared.Items;
using Sandbox.Shared.Tools;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;

namespace Sandbox.Shared
{
    public class SandboxEntityFactory : EntityFactory
    {
        public SandboxEntityFactory(ILandscapeManager2D landscapeManager)
            : base(landscapeManager)
        {
        }

        protected override Entity CreateCustomEntity(ushort classId)
        {
            switch (classId)
            {
                case SandboxEntityClassId.Annihilator: return new Annihilator();
                case SandboxEntityClassId.Shovel: return new Shovel();
                case SandboxEntityClassId.GoldCoin: return new GoldCoin();
                case SandboxEntityClassId.Editor: return new Editor();
                case SandboxEntityClassId.Carver: return new Carver();
                case SandboxEntityClassId.Torch: return new Torch();
            }

            return null;
        }

    }
}
