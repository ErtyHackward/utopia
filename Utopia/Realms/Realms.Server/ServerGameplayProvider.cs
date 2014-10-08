using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;

namespace Realms.Server
{
    /// <summary>
    /// Provides all gameplay functionality
    /// </summary>
    public class ServerGameplayProvider
    {
        private readonly Utopia.Shared.Server.ServerCore _server;
        private readonly WorldConfiguration _config;

        public ServerGameplayProvider(Utopia.Shared.Server.ServerCore server, WorldConfiguration config)
        {
            _server = server;
            _config = config;
        }

        public GodEntity CreateNewPlayerFocusEntity(uint entityId)
        {
            var entity = new GodEntity();
            entity.DynamicId = entityId;
            entity.Position = _server.LandscapeManager.GetHighestPoint(new Vector3D(10, 0, 10));
            return entity;
        }

        public PlayerCharacter CreateNewPlayerCharacter(string name, uint entityId)
        {
            var def = new Vector3D(10, 0, 10);

            var pos = _server.CustomStorage.GetVariable("SpawnPosition", def);
            
            var dEntity = new PlayerCharacter
            {
                DynamicId = entityId,
                HealthState = DynamicEntityHealthState.Normal,
                DisplacementMode = EntityDisplacementModes.Walking,
                Position = pos == def ? _server.LandscapeManager.GetHighestPoint(pos) : pos,
                CharacterName = name,
                Health = new Energy() { MaxValue = 100, CurrentValue = 100 },
                Stamina = new Energy() { MaxValue = 100, CurrentValue = 100 },
                Oxygen = new Energy() { MaxValue = 100, CurrentValue = 100 }
            };

            // give start items to the player
            var startSetName = _server.WorldParameters.Configuration.StartSet;
            if (!string.IsNullOrEmpty(startSetName))
            {
                _server.EntityFactory.FillContainer(startSetName, dEntity.Inventory);
            }
            
            return dEntity;
        }

    }
}
