using S33M3DXEngine.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.GUI;
using Utopia.Shared.Structs;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// Will contain energy management logic for the player
    /// </summary>
    public partial class PlayerEntityManager
    {
        private void EnergyFTSUpdate(GameTime timeSpent)
        {
            //Auto Regen Stamina
            var staminaRegenForRunning = _staminaRegenAmountPerSecond * timeSpent.ElapsedGameTimeInS_LD;
            _playerCharacter.Stamina.CurrentValue += staminaRegenForRunning;

            if (_playerCharacter.Oxygen.CurrentValue <= 0)
            {
                _playerCharacter.HealthState = Shared.Entities.Dynamic.DynamicEntityHealthState.Drowning;
                DrowningDamate(timeSpent);
            }

            if (IsHeadInsideWater)
            {
                OxygenForUnderWaterSwimming(timeSpent);
            }

            if (!IsHeadInsideWater && _playerCharacter.Oxygen.CurrentAsPercent < 1)
            {
                _playerCharacter.Oxygen.CurrentValue = _playerCharacter.Oxygen.MaxValue;
                _playerCharacter.HealthState = Shared.Entities.Dynamic.DynamicEntityHealthState.Normal;
            }
        }

        #region Stamina Energy managment
        private float _staminaRegenAmountPerSecond = 2.5f;
        private float _staminaAmountForRunningPerSecond = 15;
        private float _staminaAmountPerJump = 5;

        //Called when a stamina value is changed
        private void Stamina_ValueChanged(object sender, Shared.Entities.Events.EnergyChangedEventArgs e)
        {
        }

        private bool GetStaminaForRunning(GameTime timeSpent)
        {
            var staminaNeededForRunning = _staminaAmountForRunningPerSecond * timeSpent.ElapsedGameTimeInS_LD;

            if (_playerCharacter.Stamina.CurrentValue >= staminaNeededForRunning)
            {
                _playerCharacter.Stamina.CurrentValue -= staminaNeededForRunning;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool GetStaminaForJumping()
        {
            if (_playerCharacter.Stamina.CurrentValue >= _staminaAmountPerJump)
            {
                _playerCharacter.Stamina.CurrentValue -= _staminaAmountPerJump;
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Oxygen Energy
        private float _oxygenAmountForUnderWaterPerSecond = 5;
        private void Oxygen_ValueChanged(object sender, Shared.Entities.Events.EnergyChangedEventArgs e)
        {
        }

        private void OxygenForUnderWaterSwimming(GameTime timeSpent)
        {
            var oxygenConsumption = _oxygenAmountForUnderWaterPerSecond * timeSpent.ElapsedGameTimeInS_LD;

            _playerCharacter.Oxygen.CurrentValue -= oxygenConsumption;
        }

        #endregion

        #region Health Energy
        private float _healthDamagePerFallMeter = 5.0f;
        private float _healthDamageDrowningPerSecond = 15.0f;
        private void Health_ValueChanged(object sender, Shared.Entities.Events.EnergyChangedEventArgs e)
        {
            if (e.EnergyChanged.CurrentAsPercent <= 0)
            {
                _playerCharacter.HealthState = Shared.Entities.Dynamic.DynamicEntityHealthState.Dead;
                ActivateDeadState();
            }
            else
            {
                _playerCharacter.HealthState = Shared.Entities.Dynamic.DynamicEntityHealthState.Normal;
                _postEffectComponent.DeactivateEffect();
            }

            //If lost more than 30pt of life at the same time ! Risk of Stunt effect !
            if (e.ValueChangedAmount < -30)
            {
                logger.Debug("RISK of STUNT");
            }
        }

        private void PlayerEntityManager_OnLanding(double fallHeight, TerraCubeWithPosition landedCube)
        {
            //The first 5 meter are "free", no damage computed
            if (fallHeight <= 5.0) return;
            var damageComputed = (fallHeight - 5) * _healthDamagePerFallMeter;
            _playerCharacter.Health.CurrentValue -= (float)damageComputed;
        }

        private void DrowningDamate(GameTime timeSpent)
        {
            var healthLost = _healthDamageDrowningPerSecond * timeSpent.ElapsedGameTimeInS_LD;
            _playerCharacter.Health.CurrentValue -= healthLost;
        }

        private void ActivateDeadState()
        {
            _postEffectComponent.ActivateEffect("Dead");
            
            //Change backGround music
            //Change player model to ghost
            //Add graveyard object at death location
            //Change move type to "GhostFlying"

        }
        #endregion



    }
}
