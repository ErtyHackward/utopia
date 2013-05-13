using System;
using System.Linq;
using ProtoBuf;
using S33M3CoreComponents.Inputs.Actions;
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
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
            return PickType.Pick;
        }

        public IToolImpact Use(IDynamicEntity owner)
        {
            var godEntity = owner as GodEntity;

            if (godEntity == null)
                throw new ArgumentException("Invalid owner entity, should be GodEntity");

            if (godEntity.EntityState.MouseButton == MouseButton.LeftButton)
            {

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

                    logger.Warn("Selected entity " + godEntity.EntityState.PickedEntityLink);
                }
            }

            if (godEntity.EntityState.MouseButton == MouseButton.RightButton)
            {
                if (godEntity.EntityState.MouseUp)
                {
                    foreach (var entityLink in godEntity.SelectedEntities.Where(e => e.IsDynamic))
                    {
                        var entity = EntityFactory.DynamicEntityManager.FindEntity(entityLink);

                        var controller = entity.Controller as INpc;

                        if (controller != null && godEntity.EntityState.IsBlockPicked &&
                            godEntity.EntityState.PickPointNormal == Vector3I.Up)
                        {
                            controller.Movement.Goto(godEntity.EntityState.PickedBlockPosition + Vector3I.Up);
                        }

                        if (controller != null && godEntity.EntityState.IsEntityPicked && godEntity.EntityState.PickedEntityLink.IsDynamic)
                        {
                            controller.Movement.Leader = EntityFactory.DynamicEntityManager.FindEntity(godEntity.EntityState.PickedEntityLink);
                        }
                    }
                }
            }

            return new ToolImpact();
        }

        public void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }
    }
}
