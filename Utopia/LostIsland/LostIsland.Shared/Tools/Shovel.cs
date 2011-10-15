using Utopia.Shared.Cubes;
using Utopia.Shared.Interfaces;

namespace LostIsland.Shared.Tools
{
    /// <summary>
    /// a shovel is blockRemover restricted to grass & dirt
    /// </summary>
    public class Shovel : BlockRemover
    {
        public override ushort ClassId
        {
            get { return LostIslandEntityClassId.Shovel; }
        }

        public override string DisplayName
        {
            get
            {
                return "Shovel";
            }
        }

        public Shovel(ILandscapeManager2D landscapeManager) : base(landscapeManager)
        {
            RemoveableCubeIds.Add(CubeId.Dirt);
            RemoveableCubeIds.Add(CubeId.Grass);
        }
    }
}
