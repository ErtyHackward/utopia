using System.Collections.Generic;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Invisible entity to link god-mode camera to
    /// This entity is always located on top of the surface of somewhere inside the surface (leveling mode)
    /// HeadRotation holds 3rd person mode camera rotation
    /// BodyRotation contains rotation in horisontal plane only
    /// </summary>
    [ProtoContract]
    [EditorHide]
    public class GodEntity : CharacterEntity
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets or sets camera rotation
        /// </summary>
        [ProtoMember(1)]
        public override SharpDX.Quaternion HeadRotation
        {
            get
            {
                return base.HeadRotation;
            }
            set
            {
                base.HeadRotation = value;
            }
        }

        /// <summary>
        /// Gets or sets position to fly to
        /// </summary>
        public Vector3D FinalPosition { get; set; }

        /// <summary>
        /// Gets or sets entity position
        /// </summary>
        public override Vector3D Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
                FinalPosition = value;
            }
        }

        /// <summary>
        /// Gets list of selected entities by the god-entity
        /// </summary>
        public List<EntityLink> SelectedEntities { get; private set; }

        /// <summary>
        /// Gets or sets selected entity type to create designation to place item somewhere
        /// </summary>
        public ushort DesignationBlueprintId { get; set; }
        
        /// <summary>
        /// Gets god main tool
        /// </summary>
        public GodHandTool GodHand { get; private set; }
        
        public GodEntity()
        {
            SelectedEntities = new List<EntityLink>();
            GodHand = new GodHandTool();
        }

        public void ToolUse()
        {
            GodHand.Use(this);
            OnUse(EntityUseEventArgs.FromState(this));
        }
    }
}
