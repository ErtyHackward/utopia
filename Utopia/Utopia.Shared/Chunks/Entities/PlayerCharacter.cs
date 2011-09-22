using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Structs;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Cubes;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;

namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Represents a player character (it has a toolbar)
    /// </summary>
    public class PlayerCharacter : SpecialCharacterEntity
    {
        #region Private variables
        #endregion

        #region Public variables/properties
        public SlotContainer<ToolbarSlot> Toolbar { get; set; }
        
        public override EntityClassId ClassId
        {
            get { return EntityClassId.PlayerCharacter; }
        }

        public override string DisplayName
        {
            get { return CharacterName; }
        }
        #endregion

        public PlayerCharacter()
        {
            //Define the default PlayerCharacter ToolBar
            Toolbar = new SlotContainer<ToolbarSlot>(new Location2<byte>(10,1));
            
            //Create a default Tool for playerCharacter = its Hand !
            Hand playerHand = new Hand();
            Equipment.LeftTool = playerHand;
            Equipment.RightTool = playerHand;

            MoveSpeed = 5f;               //Default player MoveSpeed
            RotationSpeed = 10f;          //Default Player Rotation Speed
            Size = new SharpDX.Vector3(0.5f, 1.9f, 0.5f); //Default player size
            
            //Default Player Voxel Body
            Model = new VoxelModel();
            Model.Blocks = new byte[1, 1, 1];
            Model.Blocks[0, 0, 0] = CubeId.PlayerHead;
        }

        #region Public Methods
        public void LeftToolUse()
        {
            if (Equipment.LeftTool != null)
            {
                var args = EntityUseEventArgs.FromState(EntityState);
                args.Tool = Equipment.LeftTool;
                OnUse(args);
            }
        }

        public void RightToolUse()
        {
            if (Equipment.RightTool != null)
            {
                var args = EntityUseEventArgs.FromState(EntityState);
                args.Tool = Equipment.RightTool;
                OnUse(args);
            }
        }

        public void EntityUse()
        {
            if (EntityState.PickedEntityId != 0)
            {
                var args = EntityUseEventArgs.FromState(EntityState);
                OnUse(args);
            }
        }

        public override void Load(System.IO.BinaryReader reader)
        {
            base.Load(reader);
            Toolbar.Load(reader);
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);
            Toolbar.Save(writer);
        }
        #endregion
    }
}
