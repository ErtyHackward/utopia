using System;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// God mode tool to select range of blocks
    /// </summary>
    [ProtoContract]
    public class GodBlockSelectorTool : Item, ITool
    {
        private Vector3I _selectionStart;

        public override ushort ClassId
        {
            get { return EntityClassId.GodBlockSelector;  }
        }

        public IToolImpact Use(IDynamicEntity owner)
        {
            var godEntity = owner as GodEntity;

            if (godEntity == null)
                throw new ArgumentException("Invalid owner entity, should be GodEntity");

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

            return new ToolImpact();
        }

        public void SetSelectionStart(IDynamicEntity owner)
        {
            var godEntity = owner as GodEntity;

            if (godEntity == null)
                throw new ArgumentException("Invalid owner entity, should be GodEntity");

            if (owner.EntityState.IsBlockPicked)
            {
                _selectionStart = owner.EntityState.PickedBlockPosition;
            }

        }

        public void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }
    }
}
