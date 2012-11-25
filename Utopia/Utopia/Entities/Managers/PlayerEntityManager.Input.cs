using S33M3CoreComponents.Inputs.Actions;
using Utopia.Action;
using Utopia.Shared.Entities;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;

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
                if (Player.DisplacementMode == EntityDisplacementModes.Flying)
                {
                    DisplacementMode = EntityDisplacementModes.Walking;
                }
                else
                {
                    DisplacementMode = EntityDisplacementModes.Flying;
                }
            }

            if (!HasMouseFocus) return; //the editor(s) can acquire the mouseFocus

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Use_Left, CatchExclusiveAction))
            {
                if ((Player.EntityState.IsBlockPicked || Player.EntityState.IsEntityPicked) && Player.Equipment.RightTool != null)
                {
                    //sends the client server event that does tool.use on server
                    Player.ToolUse();

                    //client invocation to keep the client inventory in synch => This way we don't have to wait for the server back event. (This event will be dropped)
                    Player.Equipment.RightTool.Use(Player);
                }
            }

            //if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Use_Right, CatchExclusiveAction))
            //{
            //    if ((Player.EntityState.IsBlockPicked || Player.EntityState.IsEntityPicked) && Player.Equipment.RightTool != null)
            //    {
            //        //Avoid the player to add a block where he is located !            
            //        BoundingBox playerPotentialNewBlock;
            //        ComputeBlockBoundingBox(ref Player.EntityState.NewBlockPosition, out playerPotentialNewBlock);

            //        if(! VisualVoxelEntity.WorldBBox.Intersects(ref playerPotentialNewBlock))
            //        {
            //            //sends the client server event that does tool.use on server
            //            Player.RightToolUse(ToolUseMode.RightMouse);

            //            //client invocation to keep the client inventory in synch
            //            Player.Equipment.RightTool.Use(Player, ToolUseMode.RightMouse);
            //        }
            //    }
            //}

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
                        entity = link.ResolveStatic(_landscapeManager);
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
                        // send use message to the server
                        Player.EntityUse();
                        
                        if (!link.IsDynamic)
                        {
                            if (entity is IUsableEntity)
                            {
                                var usableEntity = entity as IUsableEntity;
                                usableEntity.Use();
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
