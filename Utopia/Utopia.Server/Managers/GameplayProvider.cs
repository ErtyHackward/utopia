using S33M3Engines.Shared.Math;
using Utopia.Server.Tools;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Provides all gameplay functionality
    /// </summary>
    public class GameplayProvider
    {
        private readonly Server _server;
        private readonly CubeToolLogic _logic;

        public GameplayProvider(Server server)
        {
            _server = server;
            EntityFactory.Instance.EntityCreated += InstanceEntityCreated;
            _logic = new CubeToolLogic(_server.LandscapeManager);
        }

        public PlayerCharacter CreateNewPlayerCharacter(string name, uint entityId)
        {
            var dEntity = new PlayerCharacter();
            dEntity.EntityId = entityId;
            dEntity.Position = new Vector3D(10, 128, 10);
            dEntity.CharacterName = name;
            dEntity.Equipment.LeftTool = (Tool)EntityFactory.Instance.CreateEntity(EntityClassId.Annihilator);
            dEntity.Equipment.RightTool = (Tool)EntityFactory.Instance.CreateEntity(EntityClassId.DirtAdder);

            var item = (IItem)EntityFactory.Instance.CreateEntity((EntityClassId.Shovel));
            dEntity.Inventory.PutItem(item);
            return dEntity;
        }

        private void InstanceEntityCreated(object sender, EntityFactoryEventArgs e)
        {
            // set tool logic
            if (e.Entity is Annihilator || e.Entity is DirtAdder || e.Entity is Shovel)
            {
                (e.Entity as Tool).ToolLogic = _logic;
            }
        }

    }
}
