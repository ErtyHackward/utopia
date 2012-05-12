using Utopia.Shared.Cubes;

namespace Realms.Shared.Tools
{
    /// <summary>
    /// a shovel is blockRemover restricted to grass & dirt
    /// </summary>
    public class Shovel : BlockRemover
    {
        public override ushort ClassId
        {
            get { return RealmsEntityClassId.Shovel; }
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

        public Shovel()
        {
            RemoveableCubeIds.Add(CubeId.Dirt);
            RemoveableCubeIds.Add(CubeId.Grass);
        }
    }
}
