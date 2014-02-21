using System;
using System.ComponentModel;
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
    /// Special tool for god mode view, allows to pick entities, mark blocks and make items designations
    /// </summary>
    [EditorHide]
    [ProtoContract]
    public class GodHandTool : Item, ITool
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Vector3I _selectionStart;

        [Description("Is the tool will be used multiple times when the mouse putton is pressed")]
        [ProtoMember(1)]
        public bool RepeatedActionsAllowed { get; set; }

        public override PickType CanPickBlock(BlockProfile blockProfile)
        {
            if (blockProfile.Id == WorldConfiguration.CubeId.Air)
                return PickType.Transparent;

            // allow to pick blocks by hand
            return PickType.Pick;
        }

        public IToolImpact Use(IDynamicEntity owner)
        {
            var godEntity = owner as GodEntity;

            if (godEntity == null)
                throw new ArgumentException("Invalid owner entity, should be GodEntity");

            var godHandToolState = (GodHandToolState)godEntity.EntityState.ToolState;

            if (godEntity.EntityState.MouseButton == MouseButton.LeftButton)
            {
                if (godHandToolState.DesignationBlueprintId != 0)
                {
                    return ItemPlacement(godEntity, godHandToolState);
                }

                return Selection(owner, godEntity, godHandToolState);
            }

            if (godEntity.EntityState.MouseButton == MouseButton.RightButton)
            {
                return UnitMoveOrder(godEntity);
            }

            return new ToolImpact();
        }

        private ToolImpact ItemPlacement(GodEntity godEntity, GodHandToolState godHandToolState)
        {
            Faction faction = EntityFactory.GlobalStateManager.GlobalState.Factions[godEntity.FactionId];
            var item = (IItem)EntityFactory.Config.BluePrints[godHandToolState.DesignationBlueprintId];

            var pos = item.GetPosition(godEntity);

            if (pos.Valid)
            {
                faction.Designations.Add(new PlaceDesignation
                {
                    BlueprintId = godHandToolState.DesignationBlueprintId,
                    Position = pos
                });
            }

            return new ToolImpact();
        }

        private ToolImpact Selection(IDynamicEntity owner, GodEntity godEntity, GodHandToolState godHandToolState)
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
                Faction faction = EntityFactory.GlobalStateManager.GlobalState.Factions[godEntity.FactionId];

                var select = !faction.Designations.OfType<DigDesignation>().Any(d => d.BlockPosition == godEntity.EntityState.PickedBlockPosition);

                if (godHandToolState != null && _selectionStart.Y == godHandToolState.SliceValue - 1)
                    _selectionStart.y--;

                var range = Range3I.FromTwoVectors(_selectionStart, godEntity.EntityState.PickedBlockPosition);

                var cursor = EntityFactory.LandscapeManager.GetCursor(range.Position);

                foreach (var vector in range)
                {
                    cursor.GlobalPosition = vector;

                    if (cursor.Read() == WorldConfiguration.CubeId.Air)
                        continue;

                    if (select)
                    {
                        if (!faction.Designations.OfType<DigDesignation>().Any(d => d.BlockPosition == vector))
                            faction.Designations.Add(new DigDesignation { BlockPosition = vector });
                    }
                    else
                    {
                        if (faction.Designations.OfType<DigDesignation>().Any(d => d.BlockPosition == vector))
                            faction.Designations.RemoveAll(d => d is DigDesignation && ((DigDesignation)d).BlockPosition == vector);
                    }
                }
            }

            if (godEntity.EntityState.IsEntityPicked)
            {
                godEntity.SelectedEntities.Clear();
                godEntity.SelectedEntities.Add(godEntity.EntityState.PickedEntityLink);

                logger.Warn("Selected entity " + godEntity.EntityState.PickedEntityLink);
            }
            return new ToolImpact();
        }

        private ToolImpact UnitMoveOrder(GodEntity godEntity)
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

            return new ToolImpact();
        }
    }
}
