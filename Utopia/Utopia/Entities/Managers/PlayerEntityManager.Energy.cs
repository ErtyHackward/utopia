using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
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
        private enum ressurectionStates
        {
            PendingRequest,
            PreventRessurection,
            None
        }

        private ressurectionStates ressurectionState = ressurectionStates.None;
        private Vector3D playerSpawnLocation = default(Vector3D);
        public Vector3D PlayerSpawnLocation
        {
            get { return playerSpawnLocation; }
            set { playerSpawnLocation = value; }
        }

        private void EnergyFTSUpdate(GameTime timeSpent)
        {
            if (_playerCharacter.BindedSoulStone == null && playerSpawnLocation == default(Vector3D))
            {
                //Player not binded to a soulstone, remember player spawn location for ressurection only.
                playerSpawnLocation = _playerCharacter.Position;
            }

            Vector3D BindingPosition = _playerCharacter.BindedSoulStone != null ? _playerCharacter.BindedSoulStone.Position : playerSpawnLocation;

            bool isWithinSoulStoneRange = Vector3D.DistanceSquared(BindingPosition, _playerCharacter.Position) <= 1024d;
            if (isWithinSoulStoneRange == false && ressurectionState == ressurectionStates.PreventRessurection) ressurectionState = ressurectionStates.None;

            if (_playerCharacter.HealthState == Shared.Entities.Dynamic.DynamicEntityHealthState.Dead)
            {
                if (!isWithinSoulStoneRange || ressurectionState == ressurectionStates.PreventRessurection) return;
                else
                {
                    if (ressurectionState != ressurectionStates.PendingRequest)
                    {
                        ressurectionState = ressurectionStates.PendingRequest;
                        //Show resurect Box
                        _guiManager.MessageBox("Do you want to be resurrected ?", "SoulStone", new string[] { "Yes", "No" }, ResurectionRequest);
                    }
                    return;
                }
            }

            //Auto Regen Health if in range of our own soulStone = 32 blocks distances, and a soulstone is placed
            if (isWithinSoulStoneRange && _playerCharacter.BindedSoulStone != null &&
                _playerCharacter.Health.CurrentAsPercent < 1.0f)
            {
                var soulsStoneHealingAmount = _healthSoulStoneGainPerSecond * timeSpent.ElapsedGameTimeInS_LD;
                _playerCharacter.Health.CurrentValue += soulsStoneHealingAmount;
            }

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
        private void ResurectionRequest(string action)
        {
            switch (action.ToLower())
            {
                case "yes":
                    DeactivateDeadState();
                    ressurectionState = ressurectionStates.None;
                    break;
                case "no":
                    ressurectionState = ressurectionStates.PreventRessurection;
                    break;
                default:
                    break;
            }
        }

        private float _healthSoulStoneGainPerSecond = 5.0f;
        private float _healthDamagePerFallMeter = 5.0f;
        private float _healthDamageDrowningPerSecond = 15.0f;
        private void Health_ValueChanged(object sender, Shared.Entities.Events.EnergyChangedEventArgs e)
        {
            if (e.EnergyChanged.CurrentAsPercent <= 0 && _playerCharacter.HealthState != Shared.Entities.Dynamic.DynamicEntityHealthState.Dead)
            {
                ActivateDeadState();
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
            _playerCharacter.HealthState = Shared.Entities.Dynamic.DynamicEntityHealthState.Dead;
            _postEffectComponent.ActivateEffect("Dead");

            //Change player model to ghost ==> Will be automatic by the DynamicEntityManager that is listening to the HealthState change event

            //Change backGround music
            //Add graveyard object at death location

            DisplacementMode = Shared.Entities.EntityDisplacementModes.Flying;
        }

        private void DeactivateDeadState()
        {
            _playerCharacter.Health.CurrentValue = 10; //This will trigger 
            _playerCharacter.HealthState = Shared.Entities.Dynamic.DynamicEntityHealthState.Normal;
            _postEffectComponent.DeactivateEffect();

            //Change player model to Normal ==> Will be automatic by the DynamicEntityManager that is listening to the HealthState change event

            //Change backGround music
            //Add graveyard object at death location

            DisplacementMode = Shared.Entities.EntityDisplacementModes.Walking;
        }
        #endregion

    }
}
