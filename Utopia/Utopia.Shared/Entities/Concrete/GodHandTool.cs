using System;
using ProtoBuf;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Settings;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Special tool for god mode view, allows to pick entities
    /// </summary>
    [EditorHide]
    [ProtoContract]
    public class GodHandTool : Item, ITool
    {
        public override ushort ClassId
        {
            get { return EntityClassId.GodHand; }
        }

        public override PickType CanPickBlock(BlockProfile blockProfile)
        {
            if (blockProfile.Id == WorldConfiguration.CubeId.Air)
                return PickType.Transparent;

            // don't allow to pick blocks by hand
            return PickType.Stop;
        }

        public IToolImpact Use(IDynamicEntity owner)
        {
            var focusEntity = owner as GodEntity;

            if (focusEntity == null)
                throw new ArgumentException("Invalid owner entity, should be GodEntity");

            if (focusEntity.EntityState.IsEntityPicked)
            {
                focusEntity.SelectedEntities.Clear();
                focusEntity.SelectedEntities.Add(focusEntity.EntityState.PickedEntityLink);
            }

            return new ToolImpact();
        }

        public void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }
    }
}
