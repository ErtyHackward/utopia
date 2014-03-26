using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.GUI;
using Utopia.Particules;
using Utopia.Shared.Net.Web.Responses;
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

        private ressurectionStates _ressurectionState = ressurectionStates.None;
        private Vector3D _playerSpawnLocation = default(Vector3D);
        private bool _isWithinSoulStoneRange;

        public Vector3D PlayerSpawnLocation
        {
            get { return _playerSpawnLocation; }
            set { _playerSpawnLocation = value; }
        }

        private void EnergyFTSUpdate(GameTime timeSpent)
        {
            if (_playerCharacter.BindedSoulStone == null && _playerSpawnLocation == default(Vector3D))
            {
                //Player not binded to a soulstone, remember player spawn location for ressurection only.
                _playerSpawnLocation = _playerCharacter.Position;
            }

            Vector3D BindingPosition = _playerCharacter.BindedSoulStone != null ? _playerCharacter.BindedSoulStone.Position : _playerSpawnLocation;

            _isWithinSoulStoneRange = Vector3D.DistanceSquared(BindingPosition, _playerCharacter.Position) <= 1024d;
            if (_isWithinSoulStoneRange == false && _ressurectionState == ressurectionStates.PreventRessurection) _ressurectionState = ressurectionStates.None;

            if (_playerCharacter.HealthState == Shared.Entities.Dynamic.DynamicEntityHealthState.Dead)
            {
                if (!_isWithinSoulStoneRange || _ressurectionState == ressurectionStates.PreventRessurection) return;
                else
                {
                    if (_ressurectionState != ressurectionStates.PendingRequest)
                    {
                        _ressurectionState = ressurectionStates.PendingRequest;
                        //Show resurect Box
                        _guiManager.MessageBox("Do you want to be resurrected ?", "SoulStone", new string[] { "Yes", "No" }, ResurectionRequest);
                    }
                    return;
                }
            }

            //Auto Regen Stamina
            var staminaRegenForRunning = _staminaRegenAmountPerSecond * timeSpent.ElapsedGameTimeInS_LD;
            _playerCharacter.Stamina.CurrentValue += staminaRegenForRunning;

            if (_playerCharacter.Oxygen.CurrentValue <= 0)
            {
                _playerCharacter.HealthState = Shared.Entities.Dynamic.DynamicEntityHealthState.Drowning;
                DrowningDamage(timeSpent);
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

        /// <summary>
        /// Will be called +/- second
        /// </summary>
        private void energyUpdateTimer_OnTimerRaised(float elapsedTimeInS)
        {
            //This is updated here, every second to avoid to send to server 40 times per second the updated amount of life
            //Auto Regen Health if in range of our own soulStone = 32 blocks distances, and a soulstone is placed
            if (_isWithinSoulStoneRange && _playerCharacter.BindedSoulStone != null &&
                _playerCharacter.Health.CurrentAsPercent < 1.0f)
            {
                var soulsStoneHealingAmount = _healthSoulStoneGainPerSecond * elapsedTimeInS;
                _playerCharacter.HealthImpact(soulsStoneHealingAmount);
            }
        }

        #region Stamina Energy managment
        private float _staminaRegenAmountPerSecond = 2.5f;
        private float _staminaAmountForRunningPerSecond = 15;
        private float _staminaAmountPerJump = 5;

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
                    _ressurectionState = ressurectionStates.None;
                    break;
                case "no":
                    _ressurectionState = ressurectionStates.PreventRessurection;
                    break;
                default:
                    break;
            }
        }

        private float _healthSoulStoneGainPerSecond = 5.0f;
        private float _healthDamagePerFallMeter = 5.0f;
        private float _healthDamageDrowningPerSecond = 15.0f;

        private void PlayerEntityManager_OnLanding(double fallHeight, TerraCubeWithPosition landedCube)
        {
            //The first 5 meter are "free", no damage computed
            if (fallHeight <= 5.0) return;
            var damageComputed = (fallHeight - 5) * _healthDamagePerFallMeter;
            _playerCharacter.HealthImpact(-(float)damageComputed);
        }

        private void DrowningDamage(GameTime timeSpent)
        {
            var healthLost = _healthDamageDrowningPerSecond * timeSpent.ElapsedGameTimeInS_LD;
            _playerCharacter.HealthImpact(-healthLost);
        }

        void _playerCharacter_HealthChanged(object sender, Shared.Entities.Events.EntityHealthChangeEventArgs e)
        {
            if (e.Change < -2)
            {
                //Only if first person mode !
                if (_cameraManager.ActiveCamera.CameraType == S33M3CoreComponents.Cameras.CameraType.FirstPerson)
                {
                    //Damage indicator in case of First Person damage received
                    UtopiaParticuleEngine.AddDynamicEntityParticules(_cameraManager.ActiveCamera.WorldPosition.Value, _entityRotations.LookAt, UtopiaParticuleEngine.DynamicEntityParticuleType.Blood);
                }

                if (e.Change < -20)
                {
                    _soundEngine.StartPlay2D("Hurt", 1.0f);
                }
                else
                {
                    _soundEngine.StartPlay2D("Hurt", 0.3f);
                }
            }
        }

        private void playerCharacter_HealthStateChanged(object sender, Shared.Entities.Events.EntityHealthStateChangeEventArgs e)
        {
            switch (e.NewState)
            {
                case Utopia.Shared.Entities.Dynamic.DynamicEntityHealthState.Normal:
                    if (e.PreviousState == Shared.Entities.Dynamic.DynamicEntityHealthState.Dead)
                    {
                        //var restore = _playerCharacter.Health.CurrentValue < 0 ? _playerCharacter.Health.CurrentValue : 0;
                        //_playerCharacter.HealthImpact(restore + 10);
                        _postEffectComponent.DeactivateEffect();
                        _worldPosition = new Vector3D(_playerCharacter.Position.X, _physicSimu.GroundBelowEntity, _playerCharacter.Position.Z);
                        _fallMaxHeight = double.MinValue;
                        _playerCharacter.DisplacementMode = Shared.Entities.EntityDisplacementModes.Walking;
                    }
                    break;
                case Utopia.Shared.Entities.Dynamic.DynamicEntityHealthState.Drowning:
                    break;
                case Utopia.Shared.Entities.Dynamic.DynamicEntityHealthState.Dead:
                    _soundEngine.StartPlay2D("Dying");
                    _postEffectComponent.ActivateEffect("Dead");
                    break;
                default:
                    break;
            }
        }

        private void DeactivateDeadState()
        {
            _playerCharacter.HealthState = Shared.Entities.Dynamic.DynamicEntityHealthState.Normal;
        }
        #endregion

    }
}
