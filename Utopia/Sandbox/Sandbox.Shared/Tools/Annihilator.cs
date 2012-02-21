namespace Sandbox.Shared.Tools
{
    /// <summary>
    /// Test tool that can remove anything
    /// </summary>
    public class Annihilator : BlockRemover
    {
        public override ushort ClassId
        {
            get { return SandboxEntityClassId.Annihilator; }
        }

        public override string DisplayName
        {
            get { return "Annihilator"; }
        }        
    }

}
