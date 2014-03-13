using S33M3DXEngine.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.GUI;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// Will contain energy management logic for the player
    /// </summary>
    public partial class PlayerEntityManager
    {
        private Hud _hudGui;

        private void Stamina_ValueChanged(object sender, Shared.Entities.Events.EnergyChangedEventArgs e)
        {
        }

        private void Oxygen_ValueChanged(object sender, Shared.Entities.Events.EnergyChangedEventArgs e)
        {
            //If 0 => Health decrease
        }

        private void Health_ValueChanged(object sender, Shared.Entities.Events.EnergyChangedEventArgs e)
        {
            //if 0 => Dead ghost state
            //if < X then chance of stun status
        }

        private float _staminaAmountForRunningPerSecond = 2;
        private bool GetStaminaForRunning(GameTime timeSpent)
        {
            var staminaNeededForRunning = _staminaAmountForRunningPerSecond * timeSpent.ElapsedGameTimeInS_LD;

            if (_playerCharacter.Stamina.CurrentValue > staminaNeededForRunning)
            {
                _playerCharacter.Stamina.CurrentValue -= staminaNeededForRunning;
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
