using S33M3CoreComponents.Inputs.Actions;
using Utopia.Action;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete.System;
using Utopia.Shared.Entities.Interfaces;
using S33M3CoreComponents.Cameras;
using Utopia.Shared.Entities.Dynamic;

namespace Utopia.Entities.Managers
{
    //Handle all Input related stuff for player
    public partial class PlayerEntityManager
    {
        private bool _isAutoRepeatedEvent;
        private bool IsRestrictedMode
        {
            get
            {
                return _playerCharacter.HealthState == DynamicEntityHealthState.Dead;
            }
        }

        #region Private Methods
        /// <summary>
        /// Handle Player Actions - Movement and rotation input are not handled here
        /// </summary>
        private void inputHandler()
        {
            if (!IsRestrictedMode && _inputsManager.ActionsManager.isTriggered(Actions.Move_Mode, CatchExclusiveAction))
            {
                if (!Player.CanFly)
                {
                    logger.Warn("User want to fly but he can't!");
                    return;
                }

                if (Player.DisplacementMode == EntityDisplacementModes.Flying)
                {
                    _playerCharacter.DisplacementMode = EntityDisplacementModes.Walking;
                }
                else
                {
                    _playerCharacter.DisplacementMode = EntityDisplacementModes.Flying;
                }
            }

            if (!HasMouseFocus) 
                return; //the editor(s) can acquire the mouseFocus

            if (!IsRestrictedMode && _inputsManager.ActionsManager.isTriggered(UtopiaActions.DropMode, CatchExclusiveAction))
            {
                // switch the drop mode if possible
                var tool = PlayerCharacter.Equipment.RightTool;
                if (tool is ITool)
                {
                    PutMode = !PutMode;
                }
            }

            if (!IsRestrictedMode && _inputsManager.ActionsManager.isTriggered(UtopiaActions.UseLeft, out _isAutoRepeatedEvent, CatchExclusiveAction))
            {
                if (Player.EntityState.IsBlockPicked || Player.EntityState.IsEntityPicked)
                {
                    var item = PlayerCharacter.Equipment.RightTool;

                    if (item == null)
                    {
                        if (!_isAutoRepeatedEvent)
                            HandleHandUse();
                    }
                    else
                    {
                        var tool = item as ITool;

                        if (_isAutoRepeatedEvent)
                        {
                            // don't repeat put actions
                            if (_putMode || tool == null)
                                return;

                            if (!tool.RepeatedActionsAllowed)
                                return;
                        }

                        if (_putMode || tool == null)
                        {
                            PlayerCharacter.PutUse();
                        }
                        else
                        {
                            PlayerCharacter.ToolUse();
                        }
                    }
                }
            }

            if (_cameraManager.ActiveBaseCamera.CameraType == CameraType.ThirdPerson)
            {
                if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.RightDown, CatchExclusiveAction))
                {
                    _inputsManager.MouseManager.MouseCapture = true;
                }
                else
                {
                    _inputsManager.MouseManager.MouseCapture = false;
                }
            }

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.EntityUse, CatchExclusiveAction))
            {
                if (IsRestrictedMode)
                {
                    if (Player.EntityState.IsEntityPicked && !Player.EntityState.PickedEntityLink.IsDynamic)
                    {
                        var entity = Player.EntityState.PickedEntityLink.ResolveStatic(_factory.LandscapeManager);
                        var soulStone = entity as SoulStone;
                        if (soulStone != null)
                        {
                            if (soulStone.DynamicEntityOwnerID == Player.DynamicId)
                            {
                                if (_ressurectionState == ressurectionStates.PreventRessurection)
                                    _ressurectionState = ressurectionStates.None;
                            }
                        }
                    }
                }
                else
                {
                    // using 'picked' entity (picked here means entity is in world having cursor over it, not in your hand or pocket) 
                    // like opening a chest or a door  

                    HandleHandUse();
                }
            }

            if (!IsRestrictedMode && _inputsManager.ActionsManager.isTriggered(UtopiaActions.EntityThrow, CatchExclusiveAction))
            {
                //TODO unequip left item and throw it on the ground, (version 0 = place it at newCubeplace, animation later)                
                // and next, throw the right tool if left tool is already thrown
            }

        }

        private bool HandleHandUse()
        {
            if (!Player.EntityState.IsEntityPicked || Player.EntityState.PickedEntityLink.IsDynamic)
                return false;

            var entity = Player.EntityState.PickedEntityLink.ResolveStatic(_factory.LandscapeManager);

            if (entity.RequiresLock)
            {
                if (_lockedEntity != null)
                {
                    logger.Warn("Unable to lock two items at once");
                    return false;
                }

                _lockedEntity = entity;
                _itemMessageTranslator.RequestLock(_lockedEntity);
            }
            else
            {
                // hand tool use
                PlayerCharacter.HandUse();
            }
            return true;
        }

        #endregion        
    }
}
