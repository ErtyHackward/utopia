using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using S33M3DXEngine.Main;
using Utopia.Shared.Settings;
using Utopia.Shared.Entities;
using Utopia.Shared.Configuration;

namespace Utopia.Entities.Managers
{
    public partial class PlayerEntityManager
    {
        #region Private Methods
        private void PhysicOnEntity(EntityDisplacementModes mode, ref GameTime timeSpent)
        {
            if (_stopMovedAction && _groundCubeProgile.SlidingValue == 0)
            {
                _stopMovedAction = false;
            }

            switch (mode)
            {
                case EntityDisplacementModes.Dead:
                    _physicSimu.Friction = 0f;
                    PhysicSimulation(ref timeSpent);
                    break;
                case EntityDisplacementModes.God:
                case EntityDisplacementModes.Flying:
                    _physicSimu.Friction = 0f;
#if !DEBUG
                    PhysicSimulation(ref timeSpent);    //Apply physic constraint on new compute location
#endif
                    break;
                case EntityDisplacementModes.Swiming:
                    _physicSimu.Friction = 0.3f;
                    PhysicSimulation(ref timeSpent);    //Apply physic constraint on new compute location
                    break;
                case EntityDisplacementModes.Walking:

                    if (_physicSimu.isInContactWithLadder)
                    {
                        if (!_physicSimu.OnGround)
                        {
                            _physicSimu.AirFriction = 0.25f;
                            _physicSimu.Friction = 0.25f;
                        }
                        else
                        {
                            _physicSimu.AirFriction = 0;
                            if (_stopMovedAction == false)
                            {
                                _physicSimu.Friction = _groundCubeProgile.Friction; //0.25f;
                            }
                            else
                            {
                                //I did stop to move, but I'm on a sliding block => Will slide a little before ending movement
                                _physicSimu.Friction = _groundCubeProgile.SlidingValue;
                            }
                        }

                        PhysicSimulation(ref timeSpent); //Apply physic constraint on new compute location
                        break;
                    } 
                    
                    if (_physicSimu.OnGround)
                    {
                        _physicSimu.AirFriction = 0.0f;
                        if (_stopMovedAction == false)
                        {
                            _physicSimu.Friction = _groundCubeProgile.Friction; //0.25f;
                        }
                        else
                        {
                            //I did stop to move, but I'm on a sliding block => Will slide a little before ending movement
                            _physicSimu.Friction = _groundCubeProgile.SlidingValue;
                        }
                    }
                    else
                    {
                        _physicSimu.Friction = 0.0f;
                        _physicSimu.AirFriction = 0.07f;
                    }
                    PhysicSimulation(ref timeSpent);    //Apply physic constraint on new compute location
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Apply physic constraint on the new player location (Collisions tests, ...)
        /// </summary>
        /// <param name="timeSpent"></param>
        private void PhysicSimulation(ref GameTime timeSpent)
        {
            Vector3I GroundDirection = new Vector3I(0, -1, 0);
            Vector3D newWorldPosition;
            double BlockOffset;

            _cubesHolder.GetNextSolidBlockToPlayer(ref VisualVoxelEntity.WorldBBox, ref GroundDirection, out _groundCube);
            //Half cube below me ??
            _groundCubeProgile = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[_groundCube.Cube.Id];
            BlockOffset = _groundCubeProgile.YBlockOffset;
            _physicSimu.GroundBelowEntity = _groundCube.Position.Y + (1 - BlockOffset);
            _physicSimu.OnOffsettedBlock = (float)BlockOffset;

            _physicSimu.Simulate(timeSpent.ElapsedGameTimeInS_LD, out newWorldPosition);
            _worldPosition = newWorldPosition;
        }
        #endregion

    }
}
