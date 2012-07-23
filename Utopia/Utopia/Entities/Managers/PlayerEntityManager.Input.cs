using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Action;
using Utopia.Shared.Entities;
using SharpDX;
using S33M3CoreComponents.Maths;

namespace Utopia.Entities.Managers
{
    //Handle all Input related stuff for player
    public partial class PlayerEntityManager
    {
        #region Private Variables
        #endregion

        #region Public Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        /// <summary>
        /// Handle Player Actions - Movement and rotation input are not handled here
        /// </summary>
        private void inputHandler()
        {
            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Mode, CatchExclusiveAction))
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
                if ((Player.EntityState.IsBlockPicked || Player.EntityState.IsEntityPicked) && Player.Equipment.LeftTool != null)
                {
                    //sends the client server event that does tool.use on server
                    Player.LeftToolUse(ToolUseMode.LeftMouse);

                    //client invocation to keep the client inventory in synch
                    Player.Equipment.LeftTool.Use(Player, ToolUseMode.LeftMouse);
                }
            }

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Use_Right, CatchExclusiveAction))
            {
                if ((Player.EntityState.IsBlockPicked || Player.EntityState.IsEntityPicked) && Player.Equipment.LeftTool != null)
                {
                    //Avoid the player to add a block where he is located !            
                    BoundingBox playerPotentialNewBlock;
                    ComputeBlockBoundingBox(ref Player.EntityState.NewBlockPosition, out playerPotentialNewBlock);

                    if (!MBoundingBox.Intersects(ref VisualEntity.WorldBBox, ref playerPotentialNewBlock))
                    {
                        //sends the client server event that does tool.use on server
                        Player.LeftToolUse(ToolUseMode.RightMouse);

                        //client invocation to keep the client inventory in synch
                        Player.Equipment.LeftTool.Use(Player, ToolUseMode.RightMouse);
                    }
                }
            }

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.EntityUse, CatchExclusiveAction))
            {
                //TODO implement use 'picked' entity (picked here means entity is in world having cursor over it, not in your hand or pocket) 
                //like opening a chest or a door  
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
