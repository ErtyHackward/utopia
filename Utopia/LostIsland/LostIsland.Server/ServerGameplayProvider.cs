using LostIsland.Shared;
using LostIsland.Shared.Tools;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;
using Utopia.Shared.Cubes;

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
            dEntity.EntityId = entityId;
            dEntity.Position = new Vector3D(10, 128, 10);
            dEntity.CharacterName = name;
            dEntity.Equipment.LeftTool = (Tool)EntityFactory.Instance.CreateEntity(LostIslandEntityClassId.Annihilator);

            var adder = (CubeResource)EntityFactory.Instance.CreateEntity(LostIslandEntityClassId.CubeResource);
            adder.CubeId = CubeId.HalfWoodPlank;//looting a terraincube will create a new blockadder instance or add to the stack

            dEntity.Equipment.RightTool = adder;

            var item = (IItem)EntityFactory.Instance.CreateEntity((LostIslandEntityClassId.Shovel));
            dEntity.Inventory.PutItem(item);
            var item2 = (IItem)EntityFactory.Instance.CreateEntity((LostIslandEntityClassId.Pickaxe));
            dEntity.Inventory.PutItem(item2);

            var item3 = (CubeResource)EntityFactory.Instance.CreateEntity((LostIslandEntityClassId.CubeResource));
            item3.CubeId = CubeId.WoodPlank;
            dEntity.Inventory.PutItem(item3);
            
            return dEntity;
        }

    }
}
