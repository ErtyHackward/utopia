﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Settings;

namespace Utopia.Entities.Managers
{
    public partial class PlayerEntityManager
    {
        #region Private Methods
        //Get the entity being picked up
        private void GetSelectedEntity()
        {
            bool newpicking;
            EntityPickingManager.Update(); //UPdate picking data

            if (MousepickDisabled || _inputsManager.MouseManager.MouseCapture)
            {
                Vector3D pickingPointInLine = _worldPosition + _entityEyeOffset;
                newpicking = RefreshPicking(ref pickingPointInLine, _entityRotations.LookAt, 1);
            }
            else
            {
                Vector3D mouseWorldPosition;
                Vector3D mouseLookAtPosition;
                _inputsManager.MouseManager.UnprojectMouseCursor(_cameraManager.ActiveCamera, out mouseWorldPosition, out mouseLookAtPosition);
                newpicking = RefreshPicking(ref mouseWorldPosition, mouseLookAtPosition.AsVector3(), 2);
            }

            if (newpicking)
            {
                //A new Block has been pickedup
                if (Player.EntityState.IsEntityPicked == false)
                {
                    _pickingRenderer.SetPickedBlock(ref Player.EntityState.PickedBlockPosition, GameSystemSettings.Current.Settings.CubesProfile[PickedCube.Cube.Id].YBlockOffset);
                }
                else
                {
                    if (_cameraManager.ActiveCamera.CameraType == S33M3CoreComponents.Cameras.CameraType.ThirdPerson && _pickedUpEntity.Entity == Player)
                        return;
                    _pickingRenderer.SetPickedEntity(_pickedUpEntity);
                }
            }
        }

        //Will return true if a new Item has been picked up !
        private bool RefreshPicking(ref Vector3D pickingWorldPosition, Vector3 pickingLookAt, int rounding)
        {
            Player.EntityState.IsBlockPicked = false;

            //Check the Ray against all entity.
            Ray pickingRay = new Ray(pickingWorldPosition.AsVector3(), pickingLookAt);
            if (EntityPickingManager.CheckEntityPicking(ref pickingRay, out _pickedUpEntity))
            {
                _pickedUpEntityPosition = _pickedUpEntity.Entity.Position;
                Player.EntityState.PickedEntityPosition = _pickedUpEntity.Entity.Position;
                Player.EntityState.PickedEntityLink = _pickedUpEntity.Entity.GetLink();
                Player.EntityState.IsEntityPicked = true;
                Player.EntityState.IsBlockPicked = false;
                return true;
            }

            //Sample 500 points in the view direction vector
            for (int ptNbr = 0; ptNbr < 500; ptNbr++)
            {
                pickingWorldPosition += new Vector3D(pickingLookAt * 0.02f);

                //Check if a block is picked up !
                if (_cubesHolder.isPickable(ref pickingWorldPosition, out PickedCube))
                {
                    Player.EntityState.PickedBlockPosition = PickedCube.Position;

                    bool newPlacechanged = false;

                    //Find the potential new block place, by rolling back !
                    while (ptNbr > 0)
                    {
                        pickingWorldPosition -= new Vector3D(pickingLookAt * 0.02f);

                        if (_cubesHolder.isPickable(ref pickingWorldPosition, out NewCube) == false)
                        {
                            Player.EntityState.NewBlockPosition = NewCube.Position;
                            newPlacechanged = true;
                            break;
                        }
                        ptNbr--;
                    }

                    Player.EntityState.IsEntityPicked = false;
                    Player.EntityState.IsBlockPicked = true;
                    if (PickedCube.Position == Player.EntityState.PickedBlockPosition)
                    {
                        if (!newPlacechanged) return false;
                    }
                    break;
                }
            }

            return Player.EntityState.IsBlockPicked; //Return true if a new block or Entity has been picked up !
        }

        private void ComputeBlockBoundingBox(ref Vector3I blockPlace, out BoundingBox blockBoundingBox)
        {
            blockBoundingBox = new BoundingBox(new Vector3(blockPlace.X, blockPlace.Y, blockPlace.Z), new Vector3(blockPlace.X + 1, blockPlace.Y + 1, blockPlace.Z + 1));
        }
        #endregion

    }
}