using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Action;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Physics;
using SharpDX;
using S33M3CoreComponents.Maths;
using Utopia.Shared.Entities;
using S33M3Resources.Structs;
using Utopia.Shared.Settings;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Chunks;
using Utopia.Shared.Configuration;

namespace Utopia.Entities.Managers
{
    public partial class PlayerEntityManager
    {
        #region Private Variables
        #endregion

        #region Public Properties
        public double YForceApplying { get; set; }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private void UpdateEntityMovementAndRotation(ref GameTime timeSpent)
        {
            switch (Player.DisplacementMode)
            {
                case EntityDisplacementModes.Flying:
                    _gravityInfluence = 3;  // We will move 6 times faster if flying
                    break;
                case EntityDisplacementModes.Walking:
                    _gravityInfluence = 1;
                    break;
                case EntityDisplacementModes.Swiming:
                    _gravityInfluence = 1f / 2; // We will move 2 times slower when swimming
                    break;
                default:
                    break;
            }

            //Compute the delta following the time elapsed : Speed * Time = Distance (Over the elapsed time).
            _moveDelta = Player.MoveSpeed * _gravityInfluence * timeSpent.ElapsedGameTimeInS_LD;

            //Will computate all associated rotation (Hear/Eye and Body)
            _entityRotations.Update(timeSpent);

            //Will update the world position of the player (its using the new computed rotations)
            EntityMovementsOnInputs(Player.DisplacementMode, ref timeSpent);

            //Physic simulation !
            PhysicOnEntity(Player.DisplacementMode, ref timeSpent);

            //Assign to the PlayerCharacter object the newly computed positions and rotations values
            // Assign Body rotation by slowly rotate the body in the moving direction
            if (Player.Position != _worldPosition)
            {
                Quaternion targetRotation;
                if (_cameraManager.ActiveCamera.CameraType == S33M3CoreComponents.Cameras.CameraType.FirstPerson)
                {
                    // take only y-axis rotation of the head
                    targetRotation = Player.HeadRotation;
                    targetRotation.X = 0;
                    targetRotation.Z = 0;
                    targetRotation.Normalize();
                }
                else
                {
                    //In 3th person camera I cannot use the HeadRotation, as the head is not rotated.
                    targetRotation = _cameraManager.ActiveCamera.Orientation.Value;
                    targetRotation.X = 0;
                    targetRotation.Z = 0;
                    targetRotation.Normalize();
                }

                //rotate from Current body rotation to Y axis head rotation "slowly"
                Player.BodyRotation = Quaternion.Lerp(Player.BodyRotation, targetRotation, (float)Vector3D.Distance(Player.Position, _worldPosition));
            }

            Player.Position = _worldPosition; //Send the newly compute location to the playercharacter dynamicEntity

            if (_cameraManager.ActiveCamera.CameraType == S33M3CoreComponents.Cameras.CameraType.FirstPerson)
            {
                Player.HeadRotation = _entityRotations.EyeOrientation;
            }
            else
            {
                Player.HeadRotation = Player.BodyRotation;
            }
        }

        /// <summary>
        /// Compute new entity position following activated inputs
        /// </summary>
        /// <param name="mode">The current displacement mode</param>
        /// <param name="timeSpent">Elapsed time since last call</param>
        private void EntityMovementsOnInputs(EntityDisplacementModes mode, ref GameTime timeSpent)
        {
            switch (mode)
            {
                case EntityDisplacementModes.Swiming:
                    SwimmingFreeFirstPersonMove(ref timeSpent);
                    break;
                case EntityDisplacementModes.Flying:
                    FreeFirstPersonMove();
                    break;
                case EntityDisplacementModes.Walking:
                    if (_physicSimu.OnGround)
                    {
                        WalkingFirstPersonOnGround(ref timeSpent);
                    }
                    else
                    {
                        WalkingFirstPersonNotOnGround(ref timeSpent);
                    }
                    break;
                default:
                    break;
            }
        }

        private void SwimmingFreeFirstPersonMove(ref GameTime timeSpent)
        {
            float jumpPower;

            //Jump Key inside water
            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Jump, out jumpPower))
                _physicSimu.Impulses.Add(new Impulse(timeSpent) { ForceApplied = new Vector3(0, 11 + (2 * jumpPower), 0) });

            _physicSimu.Impulses.Add(new Impulse(timeSpent) { ForceApplied = _entityRotations.EntityMoveVector * _moveDelta * 20 });            
        }

        private void FreeFirstPersonMove()
        {
            _worldPosition += _entityRotations.EntityMoveVector * _moveDelta;
        }

        private void WalkingFirstPersonOnGround(ref GameTime timeSpent)
        {
            float moveModifier = 1;

            float jumpPower;

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.EndMove_Forward) ||
                _inputsManager.ActionsManager.isTriggered(UtopiaActions.EndMove_Backward) ||
                _inputsManager.ActionsManager.isTriggered(UtopiaActions.EndMove_StrafeLeft) ||
                _inputsManager.ActionsManager.isTriggered(UtopiaActions.EndMove_StrafeRight))
            {
                _stopMovedAction = true;
            }
            else
            {

                //Move 2 time slower if not touching ground
                if (!_physicSimu.OnGround) _moveDelta /= 2f;

                //Do a small "Jump" if hitted a offset wall
                if (OffsetBlockHitted > 0 && _physicSimu.OnGround)
                {
                    //Force of 8 for 0.5 offset
                    //Force of 2 for 0.1 offset
                    _physicSimu.Impulses.Add(new Impulse(timeSpent) { ForceApplied = new Vector3(0, MathHelper.FullLerp(2, 3.8f, 0.1, 0.5, OffsetBlockHitted), 0) });
                    OffsetBlockHitted = 0;
                }

                if (YForceApplying > 0)
                {
                    _physicSimu.Impulses.Add(new Impulse(timeSpent) { ForceApplied = new Vector3(0, (float)YForceApplying, 0) });
                    YForceApplying = 0.0;
                }

                //Jumping
                if ((_physicSimu.OnGround || _physicSimu.PrevPosition == _physicSimu.CurPosition || _physicSimu.AllowJumping) && _inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Jump, out jumpPower))
                {
                    _physicSimu.Impulses.Add(new Impulse(timeSpent) { ForceApplied = new Vector3(0, 7 + (2 * jumpPower), 0) });
                }

                if (_entityRotations.EntityMoveVector != Vector3.Zero) _stopMovedAction = false;
            }

            //Run only if Move forward and run button pressed at the same time.
            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Forward) && (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Run)))
            {
                moveModifier = 1.5f;
            }

            _physicSimu.Impulses.Add(new Impulse(timeSpent) { ForceApplied = _entityRotations.EntityMoveVector * 1.2f * moveModifier });
        }

        private void WalkingFirstPersonNotOnGround(ref GameTime timeSpent)
        {
            float moveModifier = 0.2f;
            float jumpPower;

            YForceApplying = 0.0;

            //Run only if Move forward and run button pressed at the same time.
            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Forward) && (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Run)))
            {
                moveModifier = 0.25f;
            }

            //Jumping
            if (_physicSimu.AllowJumping && _inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Jump, out jumpPower))
            {
                _physicSimu.Impulses.Add(new Impulse(timeSpent) { ForceApplied = new Vector3(0, 7 + (2 * jumpPower), 0) });
            }

            //_physicSimu.PrevPosition -= _entityRotations.EntityMoveVector * _moveDelta * moveModifier;
            _physicSimu.Impulses.Add(new Impulse(timeSpent) { ForceApplied = _entityRotations.EntityMoveVector * moveModifier });
        }


        /// <summary>
        /// Serie of check that needs to be done when the new position is defined
        /// </summary>
        private void CheckAfterNewPosition()
        {
            CheckForEventRaising();
        }

        private void CheckForEventRaising()
        {
            //Landing on ground after falling event
            if (_physicSimu.OnGround == false)
            {
                //New "trigger"
                if (_worldPosition.Y > _fallMaxHeight) _fallMaxHeight = _worldPosition.Y;
            }
            else
            {
                if (_physicSimu.OnGround == true && _fallMaxHeight != int.MinValue)
                {
                    if (_fallMaxHeight - _worldPosition.Y >= 0.01)
                    {
                        var handler = OnLanding;
                        if (handler != null)
                        {
                            handler(_fallMaxHeight - _worldPosition.Y, _groundCube);
                        }
#if DEBUG
                        logger.Debug("OnLandingGround event fired with height value : {0} m, cube type : {1} ", _fallMaxHeight - _worldPosition.Y, _visualWorldParameters.WorldParameters.Configuration.CubeProfiles[_groundCube.Cube.Id].Name);
#endif
                    }
                    _fallMaxHeight = int.MinValue;
                }
            }
        }

        private void CheckHeadUnderWater()
        {
            if (_cubesHolder.IndexSafe(MathHelper.Fastfloor(CameraWorldPosition.X), MathHelper.Fastfloor(CameraWorldPosition.Y), MathHelper.Fastfloor(CameraWorldPosition.Z), out _headCubeIndex))
            {
                //Get the cube at the camera position !
                _headCube = _cubesHolder.Cubes[_headCubeIndex];

                //Get Feet block
                int feetBlockIdx = _cubesHolder.FastIndex(_headCubeIndex, MathHelper.Fastfloor(CameraWorldPosition.Y), SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1);
                TerraCube feetBlock = _cubesHolder.Cubes[feetBlockIdx];
                TerraCube BelowfeetBlock = _cubesHolder.Cubes[_cubesHolder.FastIndex(feetBlockIdx, MathHelper.Fastfloor(CameraWorldPosition.Y) - 1, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1)];

                if (_visualWorldParameters.WorldParameters.Configuration.CubeProfiles[feetBlock.Id].CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid &&
                   (_visualWorldParameters.WorldParameters.Configuration.CubeProfiles[BelowfeetBlock.Id].CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid || _visualWorldParameters.WorldParameters.Configuration.CubeProfiles[_headCube.Id].CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid))
                {
                    if (DisplacementMode == EntityDisplacementModes.Walking)
                    {
                        DisplacementMode = EntityDisplacementModes.Swiming;
                    }
                }
                else
                {
                    if (DisplacementMode == EntityDisplacementModes.Swiming) DisplacementMode = EntityDisplacementModes.Walking;
                }

                //Eyes under water (Used to change view Color)
                CubeProfile headCubeProfile = _visualWorldParameters.WorldParameters.Configuration.CubeProfiles[_headCube.Id];
                if (headCubeProfile.CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid)
                {
                    int AboveHead = _cubesHolder.FastIndex(_headCubeIndex, MathHelper.Fastfloor(CameraWorldPosition.Y), SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1);
                    if (_cubesHolder.Cubes[AboveHead].Id == WorldConfiguration.CubeId.Air)
                    {
                        //Check the offset of the water
                        var Offset = CameraWorldPosition.Y - MathHelper.Fastfloor(CameraWorldPosition.Y);
                        if (Offset >= 1 - _visualWorldParameters.WorldParameters.Configuration.CubeProfiles[_headCube.Id].YBlockOffset)
                        {
                            IsHeadInsideWater = false;
                            return;
                        }
                    }

                    IsHeadInsideWater = true;
                }
                else
                {
                    IsHeadInsideWater = false;
                }
            }
        }

        #endregion

    }
}
