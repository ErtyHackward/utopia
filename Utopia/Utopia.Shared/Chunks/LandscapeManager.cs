using System;
using S33M3CoreComponents.Physics.Verlet;
using SharpDX;
using Utopia.Shared.Configuration;
using Utopia.Shared.Interfaces;
using S33M3Resources.Structs;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.World;
using S33M3CoreComponents.Maths;
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Base class for chunk landscape management
    /// </summary>
    /// <typeparam name="TChunk"></typeparam>
    public abstract class LandscapeManager<TChunk> : ILandscapeManager 
        where TChunk : AbstractChunk
    {
        protected WorldParameters _wp;

        /// <summary>
        /// Gets chunk from a global position
        /// </summary>
        /// <param name="globalPosition"></param>
        /// <returns></returns>
        public TChunk GetChunkFromBlock(Vector3D globalPosition)
        {
            return GetChunk(BlockHelper.EntityToChunkPosition(globalPosition));
        }
        
        public TChunk GetChunkFromBlock(Vector3I blockPosition)
        {
            return GetChunk(BlockHelper.BlockToChunkPosition(blockPosition));
        }

        protected LandscapeManager(WorldParameters wp)
        {
            _wp = wp;
        }

        /// <summary>
        /// Gets the chunk at position specified
        /// </summary>
        /// <param name="position">chunk position</param>
        /// <returns></returns>
        public abstract TChunk GetChunk(Vector3I position);

        IAbstractChunk ILandscapeManager.GetChunk(Vector3I position)
        {
            return GetChunk(position);
        }

        IAbstractChunk ILandscapeManager.GetChunkFromBlock(Vector3I blockPosition)
        {
            return GetChunkFromBlock(blockPosition);
        }

        /// <summary>
        /// Gets landscape cursor
        /// </summary>
        /// <param name="blockPosition">global block position</param>
        /// <returns></returns>
        public abstract ILandscapeCursor GetCursor(Vector3I blockPosition);
        
        public ILandscapeCursor GetCursor(Vector3D entityPosition)
        {
            return GetCursor(new Vector3I((int)Math.Floor(entityPosition.X), (int)entityPosition.Y, (int)Math.Floor(entityPosition.Z)));
        }

        public abstract TerraCube GetCubeAt(Vector3I vector3I);

        public bool IsSolidToPlayer(ref BoundingBox bb, bool withCubeOffSetAccount, out TerraCubeWithPosition collidingcube)
        {
            //Get ground surface 4 blocks below the Bounding box
            int Xmin = MathHelper.Floor(bb.Minimum.X);
            int Zmin = MathHelper.Floor(bb.Minimum.Z);
            int Ymin = MathHelper.Floor(bb.Minimum.Y);
            int Xmax = MathHelper.Floor(bb.Maximum.X);
            int Zmax = MathHelper.Floor(bb.Maximum.Z);
            int Ymax = MathHelper.Floor(bb.Maximum.Y);

            for (var x = Xmin; x <= Xmax; x++)
            {
                for (var z = Zmin; z <= Zmax; z++)
                {
                    for (var y = Ymin; y <= Ymax; y++)
                    {
                        var cube = GetCubeAt(new Vector3I(x, y, z));
                        var profile = _wp.Configuration.BlockProfiles[cube.Id];
                        
                        if (!profile.IsSolidToEntity) 
                            continue;

                        collidingcube.Cube = cube;
                        collidingcube.Position = new Vector3I(x, y, z);
                        collidingcube.BlockProfile = profile;

                        //Block with Offset case
                        if (withCubeOffSetAccount && profile.YBlockOffset > 0.0f)
                        {
                            //If my "Feet" are below the height of the offseted cube, then colliding true
                            FloatAsInt FeetLevel = bb.Minimum.Y;
                            FloatAsInt OffsetedCubeLevel = 1;// profile.YBlockOffset;
                            OffsetedCubeLevel -= profile.YBlockOffset;

                            //if (bb.Minimum.Y < (y + (1-profile.YBlockOffset)))
                            if (FeetLevel < (y + OffsetedCubeLevel))
                            {
                                return true;
                            }

                            //check the head when On a Offseted Block
                            cube = GetCubeAt(new Vector3I(x, MathHelper.Floor(bb.Maximum.Y + profile.YBlockOffset), z));
                            profile = _wp.Configuration.BlockProfiles[cube.Id];
                            if (profile.IsSolidToEntity) 
                                return true;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            collidingcube = new TerraCubeWithPosition();
            return false;
        }
        
        /// <summary>
        /// "Simple" collision detection check against landscape, send back the cube being collided
        /// </summary>
        /// <param name="localEntityBoundingBox"></param>
        /// <param name="newPosition2Evaluate"></param>
        public byte IsCollidingWithTerrain(ref BoundingBox localEntityBoundingBox, ref Vector3D newPosition2Evaluate)
        {
            TerraCubeWithPosition _collidingCube;

            BoundingBox boundingBox2Evaluate = new BoundingBox(localEntityBoundingBox.Minimum + newPosition2Evaluate.AsVector3(), localEntityBoundingBox.Maximum + newPosition2Evaluate.AsVector3());
            if (IsSolidToPlayer(ref boundingBox2Evaluate, true, out _collidingCube))
            {
                return _collidingCube.Cube.Id;
            }

            return WorldConfiguration.CubeId.Air;
        }

        /// <summary>
        /// Validate player move against surrounding landscape, if move not possible, it will be "rollbacked"
        /// It's used by the physic engine
        /// </summary>
        /// <param name="physicSimu"></param>
        /// <param name="localEntityBoundingBox"></param>
        /// <param name="newPosition2Evaluate"></param>
        public void IsCollidingWithTerrain(VerletSimulator physicSimu, ref BoundingBox localEntityBoundingBox, ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition)
        {
            Vector3D newPositionWithColliding = previousPosition;
            TerraCubeWithPosition _collidingCube;

            //Create a Bounding box with my new suggested position, taking only the X that has been changed !
            //X Testing =====================================================
            newPositionWithColliding.X = newPosition2Evaluate.X;
            BoundingBox boundingBox2Evaluate = new BoundingBox(localEntityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), localEntityBoundingBox.Maximum + newPositionWithColliding.AsVector3());

            //If my new X position, make me placed "inside" a block, then invalid the new position
            if (IsSolidToPlayer(ref boundingBox2Evaluate, true, out _collidingCube))
            {
                //logger.Debug("ModelCollisionDetection X detected tested {0}, assigned (= previous) {1}", newPositionWithColliding.X, previousPosition.X);

                newPositionWithColliding.X = previousPosition.X;
                if (_collidingCube.BlockProfile.YBlockOffset > 0 || physicSimu.OnOffsettedBlock > 0)
                {
                    float offsetValue = (float)((1 - _collidingCube.BlockProfile.YBlockOffset));
                    if (physicSimu.OnOffsettedBlock > 0) offsetValue -= (1 - physicSimu.OnOffsettedBlock);
                    if (offsetValue <= 0.5)
                    {
                        physicSimu.OffsetBlockHitted = offsetValue;
                    }
                }
            }

            //Z Testing =========================================================
            newPositionWithColliding.Z = newPosition2Evaluate.Z;
            boundingBox2Evaluate = new BoundingBox(localEntityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), localEntityBoundingBox.Maximum + newPositionWithColliding.AsVector3());

            //If my new Z position, make me placed "inside" a block, then invalid the new position
            if (IsSolidToPlayer(ref boundingBox2Evaluate, true, out _collidingCube))
            {
                //logger.Debug("ModelCollisionDetection Z detected tested {0}, assigned (= previous) {1}", newPositionWithColliding.Z, previousPosition.Z);

                newPositionWithColliding.Z = previousPosition.Z;
                if (_collidingCube.BlockProfile.YBlockOffset > 0 || physicSimu.OnOffsettedBlock > 0)
                {
                    float offsetValue = (float)((1 - _collidingCube.BlockProfile.YBlockOffset));
                    if (physicSimu.OnOffsettedBlock > 0) offsetValue -= (1 - physicSimu.OnOffsettedBlock);
                    if (offsetValue <= 0.5)
                    {
                        physicSimu.OffsetBlockHitted = offsetValue;
                    }
                }

            }

            //Y Testing ======================================================
            newPositionWithColliding.Y = newPosition2Evaluate.Y;
            boundingBox2Evaluate = new BoundingBox(localEntityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), localEntityBoundingBox.Maximum + newPositionWithColliding.AsVector3());

            //If my new Y position, make me placed "inside" a block, then invalid the new position
            if (IsSolidToPlayer(ref boundingBox2Evaluate, true, out _collidingCube))
            {
                //If was Jummping "before" entering inside the cube
                if (previousPosition.Y >= newPositionWithColliding.Y)
                {
                    //If the movement between 2 Y is too large, use the GroundBelowEntity value
                    if (Math.Abs(newPositionWithColliding.Y - previousPosition.Y) > 1 || physicSimu.isInContactWithLadder)
                    {
                        previousPosition.Y = physicSimu.GroundBelowEntity;
                    }
                    else
                    {
                        //Raise Up until the Ground, next the previous position
                        if (_collidingCube.BlockProfile.YBlockOffset > 0)
                        {
                            previousPosition.Y = MathHelper.Floor(previousPosition.Y + 1) - _collidingCube.BlockProfile.YBlockOffset;
                        }
                        else
                        {
                            previousPosition.Y = MathHelper.Floor(previousPosition.Y);
                        }
                    }

                    physicSimu.OffsetBlockHitted = 0;
                    physicSimu.OnGround = true; // On ground ==> Activite the force that will counter the gravity !!
                }

                //logger.Debug("ModelCollisionDetection Y detected tested {0}, assigned (= previous) {1}", newPositionWithColliding.Y, previousPosition.Y);

                newPositionWithColliding.Y = previousPosition.Y;
            }
            else
            {
                //No collision with Y, is the block below me solid to entity ?
                boundingBox2Evaluate.Minimum.Y -= 0.01f;
                if (IsSolidToPlayer(ref boundingBox2Evaluate, true, out _collidingCube))
                {
                    physicSimu.OnGround = true; // On ground ==> Activite the force that will counter the gravity !!
                }
            }
            
            //Check to see if new destination is not blocking me
            boundingBox2Evaluate = new BoundingBox(localEntityBoundingBox.Minimum + newPositionWithColliding.AsVector3(), localEntityBoundingBox.Maximum + newPositionWithColliding.AsVector3());
            if (IsSolidToPlayer(ref boundingBox2Evaluate, true, out _collidingCube))
            {
                //logger.Debug("STUCK tested {0}, assigned {1}, last Good position tested : {2}", newPositionWithColliding, previousPosition, memo);
                newPositionWithColliding = previousPosition;
                newPositionWithColliding.Y += 0.1;
            }

            newPosition2Evaluate = newPositionWithColliding;
        }

        public Vector3D GetHighestPoint(Vector3D vector2)
        {
            var chunk = GetChunkFromBlock(vector2);

            var cx = (int)vector2.X % AbstractChunk.ChunkSize.X;
            var cy = (int)vector2.Y % AbstractChunk.ChunkSize.Y;
            var cz = (int)vector2.Z % AbstractChunk.ChunkSize.Z;

            if (cx < 0) cx = AbstractChunk.ChunkSize.X + cx;
            if (cy < 0) cy = AbstractChunk.ChunkSize.Y + cy;
            if (cz < 0) cz = AbstractChunk.ChunkSize.Z + cz;

            int y;

            for (y = AbstractChunk.ChunkSize.Y-1; y >= 0; y--)
            {
                if (chunk.BlockData.GetBlock(new Vector3I(cx, y, cz)) != WorldConfiguration.CubeId.Air)
                    break;
            }

            return new Vector3D(vector2.X, y + 1, vector2.Z);
        }
    }
}
