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

        public override string UniqueName
        {
            get
            {
                return "Shovel";
            }
        }


        public override string Description
        {
            get
            {
                return "Famous Simon's shovel. Can be transformed to jetpack.";
            }
        }

        public Shovel(ILandscapeManager2D landscapeManager, LostIslandEntityFactory factory)
            : base(landscapeManager, factory)
        {
            RemoveableCubeIds.Add(CubeId.Dirt);
            RemoveableCubeIds.Add(CubeId.Grass);
        }
    }
}
