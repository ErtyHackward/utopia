using System;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Settings;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Special tool for god mode view, allows to pick entities and mark blocks
    /// </summary>
    [EditorHide]
    [ProtoContract]
    public class GodHandTool : Item, ITool
    {
        private Vector3I _selectionStart;

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
            var godEntity = owner as GodEntity;

            if (godEntity == null)
                throw new ArgumentException("Invalid owner entity, should be GodEntity");

            if (!godEntity.EntityState.MouseUp)
            {
                if (owner.EntityState.IsBlockPicked)
                {
                    _selectionStart = owner.EntityState.PickedBlockPosition;
                }
                return new ToolImpact();
            }
            
            if (godEntity.EntityState.IsBlockPicked)
            {
                var select = !godEntity.SelectedBlocks.Contains(godEntity.EntityState.PickedBlockPosition);

                var range = Range3I.FromTwoVectors(_selectionStart, godEntity.EntityState.PickedBlockPosition);

                var cursor = EntityFactory.LandscapeManager.GetCursor(range.Position);

                foreach (var vector in range)
                {
                    cursor.GlobalPosition = vector;

                    if (cursor.Read() == WorldConfiguration.CubeId.Air)
                        continue;

                    if (select)
                    {
                        if (!godEntity.SelectedBlocks.Contains(vector))
                            godEntity.SelectedBlocks.Add(vector);
                    }
                    else
                    {
                        if (godEntity.SelectedBlocks.Contains(vector))
                            godEntity.SelectedBlocks.Remove(vector);
                    }
                }
            }

            if (godEntity.EntityState.IsEntityPicked)
            {
                godEntity.SelectedEntities.Clear();
                godEntity.SelectedEntities.Add(godEntity.EntityState.PickedEntityLink);
            }

            return new ToolImpact();
        }

        public void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }
    }
}
