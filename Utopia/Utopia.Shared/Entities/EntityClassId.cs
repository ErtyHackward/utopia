namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Enumerates all available entities IDs (just for easy use in code). Please add new IDs to the bottom of the enum
    /// </summary>
    public class EntityClassId
    {
        // items
        public const ushort None = 0;
        public const ushort Hand = 1;
        public const ushort GodHand = 2;
        public const ushort GodBlockSelector = 3;

        // static
        public const ushort Container = 1000;
        public const ushort Chair = 1001;
        public const ushort Door = 1002;
        public const ushort Bed = 1003;
        public const ushort LightSource = 1004;
        public const ushort OrientedBlockLinkedItem = 1005;
        public const ushort OrientedBlockItem = 1006;
        public const ushort Stuff = 1007;

        // tools 
        public const ushort CubeResource = 1500;
        public const ushort BasicCollector = 1501;

        // blocks
        public const ushort ThinGlass = 2001;

        // alive
        public const ushort PlayerCharacter = 3000;
        public const ushort NonPlayerCharacter = 3001;
        public const ushort Zombie = 3002;

        public const ushort Plant = 4000;
        public const ushort Food = 4001;

        // special
        public const ushort FocusEntity = 10000;
    }
}
