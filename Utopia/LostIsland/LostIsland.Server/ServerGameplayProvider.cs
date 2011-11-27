using System;
using LostIsland.Shared;
using LostIsland.Shared.Items;
using LostIsland.Shared.Tools;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Shared.Cubes;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace LostIsland.Server
{
    /// <summary>
    /// Provides all gameplay functionality
    /// </summary>
    public class ServerGameplayProvider
    {
        private readonly Utopia.Server.Server _server;

        public ServerGameplayProvider(Utopia.Server.Server server)
        {
            _server = server;
        }

        public PlayerCharacter CreateNewPlayerCharacter(string name, uint entityId)
        {
            var dEntity = new PlayerCharacter();
            dEntity.DynamicId = entityId;
            dEntity.DisplacementMode = EntityDisplacementModes.Walking;
            dEntity.Position = _server.LandscapeManager.GetHighestPoint(new Vector3D(10, 0, 10));
            dEntity.CharacterName = name;
            ContainedSlot outItem;
            //dEntity.Equipment.Equip(EquipmentSlotType.LeftHand, new EquipmentSlot<ITool> { Item = (ITool)EntityFactory.Instance.CreateEntity(LostIslandEntityClassId.Annihilator) }, out outItem);

            var adder = (CubeResource)EntityFactory.Instance.CreateEntity(EntityClassId.CubeResource);
            adder.CubeId = CubeId.HalfWoodPlank;//looting a terraincube will create a new blockadder instance or add to the stack

            dEntity.Equipment.Equip(EquipmentSlotType.LeftHand, new EquipmentSlot<ITool> { Item = adder }, out outItem);

            foreach (var cubeId in CubeId.All())
            {
                if (cubeId == CubeId.Air)
                    continue;

                var item3 = (CubeResource)EntityFactory.Instance.CreateEntity((EntityClassId.CubeResource));
                item3.CubeId = cubeId;
                dEntity.Inventory.PutItem(item3);
            }

            //var item = (IItem)EntityFactory.Instance.CreateEntity((LostIslandEntityClassId.Shovel));
            //dEntity.Inventory.PutItem(item);

            //dEntity.Inventory.PutItem(new GoldCoin(), 45821);

            dEntity.Inventory.PutItem((Carver)EntityFactory.Instance.CreateEntity((LostIslandEntityClassId.Carver)));
            
            return dEntity;
        }

    }
}
