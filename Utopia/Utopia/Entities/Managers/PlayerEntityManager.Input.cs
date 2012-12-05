using S33M3CoreComponents.Inputs.Actions;
using Utopia.Action;
using Utopia.Shared.Entities;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Particules.Interfaces;
using S33M3CoreComponents.Particules;
using S33M3Resources.Structs;
using Utopia.Shared.GameDXStates;
using S33M3DXEngine.Textures;
using Utopia.Shared.Settings;
using SharpDX.Direct3D11;
using S33M3DXEngine.RenderStates;

namespace Utopia.Entities.Managers
{

    //Handle all Input related stuff for player
    public partial class PlayerEntityManager
    {
        private ShaderResourceView _particules;
        private IEmitter _testEmitter;

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
            else
            {
                if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Use_Right, CatchExclusiveAction))
                {
                    if (_particules == null)
                    {
                        ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, _d3DEngine.ImmediateContext, ClientSettings.TexturePack + @"Particules/", @"*.png", FilterFlags.Point, "ArrayTexture_Particules", out _particules);
                        ToDispose(_particules);

                        //Testing a Particule generator
                        _testEmitter = new Emitter(this._cameraManager.ActiveCamera.WorldPosition.Value,
                                                           new Vector3(0, 2, 0),
                                                           new Vector2(5, 5),
                                                           5.0f,
                                                           Emitter.ParticuleGenerationMode.Manual,
                                                           new Vector3(2, 1, 2),
                                                           new Vector3D(0, -9.8, 0),
                                                           RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_MinMagMipLinear),
                                                           _particules,
                                                           DXStates.Rasters.Default,
                                                           DXStates.Blenders.Enabled,
                                                           DXStates.DepthStencils.DepthEnabled);


                        _particuleEngine.AddEmitter(_testEmitter);

                    }

                    _testEmitter.EmmitParticule(10);


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
