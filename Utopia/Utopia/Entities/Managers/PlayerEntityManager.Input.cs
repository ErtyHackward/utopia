using S33M3CoreComponents.Inputs.Actions;
using Utopia.Action;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using S33M3CoreComponents.Cameras;

namespace Utopia.Entities.Managers
{
    //Handle all Input related stuff for player
    public partial class PlayerEntityManager
    {
        #region Private Methods
        /// <summary>
        /// Handle Player Actions - Movement and rotation input are not handled here
        /// </summary>
        private void inputHandler()
        {
            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_Mode, CatchExclusiveAction))
            {
                if (Player.DisplacementMode == EntityDisplacementModes.God)
                {
                    DisplacementMode = EntityDisplacementModes.Walking;
                }
                else
                {
                    DisplacementMode = EntityDisplacementModes.God;
                }
            }

            if (!HasMouseFocus) 
                return; //the editor(s) can acquire the mouseFocus

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.DropMode, CatchExclusiveAction))
            {
                // switch the drop mode if possible
                var tool = PlayerCharacter.Equipment.RightTool;
                if (tool != null && tool is ITool)
                {
                    PutMode = !PutMode;
                }
            }

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.UseLeft, CatchExclusiveAction))
            {
                if (Player.EntityState.IsBlockPicked || Player.EntityState.IsEntityPicked)
                {

                    var item = PlayerCharacter.Equipment.RightTool;

                    if (item == null)
                        item = PlayerCharacter.HandTool;

                    if (_putMode || !(item is ITool))
                    {
                        // can't put the hand!
                        if (item == PlayerCharacter.HandTool)
                            return;

                        // send put message to the server
                        PlayerCharacter.PutUse();

                        // client sync
                        item.Put(Player);
                    }
                    else
                    {
                        // raise DynamicEntity.Use event and calls current tool Use method
                        PlayerCharacter.ToolUse();
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
                // using 'picked' entity (picked here means entity is in world having cursor over it, not in your hand or pocket) 
                // like opening a chest or a door  

                if (Player.EntityState.IsEntityPicked)
                {
                    var link = Player.EntityState.PickedEntityLink;

                    IEntity entity = null;

                    if (link.IsDynamic)
                    {
                        //TODO: resolve dynamic entity
                    }
                    else
                    {
                        entity = link.ResolveStatic(_factory.LandscapeManager);
                    }

                    if (entity == null)
                        return;
                    
                    // check if the entity need to be locked

                    if (entity.RequiresLock)
                    {
                        if (_lockedEntity != null)
                        {
                            logger.Warn("Unable to lock two items at once");
                            return;
                        }

                        _lockedEntity = entity;
                        _itemMessageTranslator.RequestLock(_lockedEntity);
                    }
                    else
                    {
                        if (!link.IsDynamic)
                        {
                            if (entity is IUsableEntity)
                            {
                                // send use message to the server
                                PlayerCharacter.EntityUse();

                                var usableEntity = entity as IUsableEntity;
                                usableEntity.Use();
                            }
                            else
                            {
                                // hand use
                                // raise DynamicEntity.Use event that will invoke local tool and send a use-message to the server
                                PlayerCharacter.HandUse();
                            }
                        }
                    }
                }
            }

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.EntityThrow, CatchExclusiveAction))
            {
                //TODO unequip left item and throw it on the ground, (version 0 = place it at newCubeplace, animation later)                
                // and next, throw the right tool if left tool is already thrown
            }

        }
        #endregion        
    }
}
