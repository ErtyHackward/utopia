using Utopia.Shared.Interfaces;

namespace LostIsland.Shared.Tools
{
    /// <summary>
    /// Test tool that can remove anything
    /// </summary>
    public class Annihilator : BlockRemover
    {
        public override ushort ClassId
        {
            get { return LostIslandEntityClassId.Annihilator; }
        }

        public override string DisplayName
        {
            get { return "Annihilator"; }
        }

        public Annihilator(ILandscapeManager2D landscapeManager) : base(landscapeManager)
        {
            
        }
        
    }

}
