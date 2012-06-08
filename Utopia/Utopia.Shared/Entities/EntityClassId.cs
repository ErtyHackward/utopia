namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Enumerates all available entities IDs (just for easy use in code). Please add new IDs to the bottom of the enum
    /// </summary>
    public class EntityClassId
    {
        // items
        public const ushort None = 0;

        // static
        public const ushort Chest = 1000;
        public const ushort Chair = 1001;
        public const ushort Door = 1002;
        public const ushort Bed = 1003;

        // tools 
        public const ushort CubeResource = 1500;

        // blocks
        public const ushort ThinGlass = 2001;

        //alive
        public const ushort PlayerCharacter = 3000;
        public const ushort NonPlayerCharacter = 3001;
        public const ushort Zombie = 3002;

        //Static Sprite
        public const ushort Grass = 4000;
        public const ushort Flower1 = 4001;
        public const ushort Flower2 = 4002;
        public const ushort Flower3 = 4003;

        //Special case
        public const ushort EditableEntity = 10001;
        
    }
}
