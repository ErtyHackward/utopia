using S33M3Engines.Shared.Math;
using Sandbox.Shared.Items;
using Sandbox.Shared.Tools;
using Utopia.Shared.Cubes;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace Sandbox.Server
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

            var adder = _server.EntityFactory.CreateEntity<CubeResource>();
            adder.CubeId = CubeId.HalfWoodPlank;//looting a terraincube will create a new blockadder instance or add to the stack

            dEntity.Equipment.Equip(EquipmentSlotType.LeftHand, new EquipmentSlot<ITool> { Item = adder }, out outItem);

            //Add Items in inventory, every cubes
            foreach (var cubeId in CubeId.All())
            {
                if (cubeId == CubeId.Air)
                    continue;

                var item3 = _server.EntityFactory.CreateEntity<CubeResource>();
                item3.CubeId = cubeId;
                dEntity.Inventory.PutItem(item3);
            }
            //Add coins + Torch
            var goldCoins = _server.EntityFactory.CreateEntity<GoldCoin>();
            dEntity.Inventory.PutItem(goldCoins);
            var Torch = _server.EntityFactory.CreateEntity<Torch>();
            dEntity.Inventory.PutItem(Torch);

            //var item = (IItem)EntityFactory.Instance.CreateEntity((LostIslandEntityClassId.Shovel));
            //dEntity.Inventory.PutItem(item);

            //dEntity.Inventory.PutItem(new GoldCoin(), 45821);

            dEntity.Inventory.PutItem(_server.EntityFactory.CreateEntity<Carver>());
            
            return dEntity;
        }

    }
}
