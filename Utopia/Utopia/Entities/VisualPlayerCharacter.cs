using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities;
using S33M3Engines;
using SharpDX;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Action;
using Utopia.InputManager;
using Utopia.Shared.Chunks;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks.Entities.Concrete;
using S33M3Engines.D3D;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Structs;
using S33M3Engines.Maths;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Cubes;
using S33M3Engines.Cameras;
using S33M3Engines.WorldFocus;

namespace Utopia.Entities
{
    public class VisualPlayerCharacter : VisualSpecialCharacterEntity
    {
        #region Private Variables
        private SingleArrayChunkContainer _cubesHolder;

        private bool _isBlockPicked;
        private Location3<int> _pickedBlock, _previousPickedBlock, _newCubePlace;
        private BoundingBox _playerSelectedBox, _playerPotentialNewBlock;
        private TerraCube _pickedCube;

        private int _headCubeIndex;
        private TerraCube _headCube;

        private ActionsManager _actions;
        #endregion

        #region Public Variables/Properties
        public readonly PlayerCharacter PlayerCharacter;
        #endregion

        public VisualPlayerCharacter(D3DEngine engine,
                                     CameraManager cameraManager,
                                     WorldFocusManager worldFocusManager,
                                     ActionsManager actions,
                                     InputsManager inputsManager,
                                     SingleArrayChunkContainer cubesHolder,
                                     VoxelMeshFactory voxelMeshFactory,
                                     PlayerCharacter entity, 
                                     VoxelEntity voxelEntity)
            : base(engine, cameraManager, worldFocusManager, actions, inputsManager, cubesHolder, voxelMeshFactory, voxelEntity, entity)
        {
            _actions = actions;
            _cubesHolder = cubesHolder;
            PlayerCharacter = entity;
        }

        #region Private Methods
        private void GetSelectedBlock()
        {
            _isBlockPicked = false;

            _previousPickedBlock = _pickedBlock;

            DVector3 pickingPointInLine = WorldPosition.Value + EntityEyeOffset;
            //Sample 500 points in the view direction vector
            for (int ptNbr = 0; ptNbr < 500; ptNbr++)
            {
                pickingPointInLine += LookAt * 0.02;

                if (_cubesHolder.isPickable(ref pickingPointInLine, out _pickedCube))
                {
                    _pickedBlock.X = MathHelper.Fastfloor(pickingPointInLine.X);
                    _pickedBlock.Y = MathHelper.Fastfloor(pickingPointInLine.Y);
                    _pickedBlock.Z = MathHelper.Fastfloor(pickingPointInLine.Z);

                    //Find the face picked up !
                    float FaceDistance;
                    Ray newRay = new Ray((WorldPosition.Value + EntityEyeOffset).AsVector3(), LookAt.AsVector3());
                    BoundingBox bBox = new SharpDX.BoundingBox(new Vector3(_pickedBlock.X, _pickedBlock.Y, _pickedBlock.Z), new Vector3(_pickedBlock.X + 1, _pickedBlock.Y + 1, _pickedBlock.Z + 1));
                    newRay.Intersects(ref bBox, out FaceDistance);

                    DVector3 CollisionPoint = WorldPosition.Value + EntityEyeOffset + (LookAt * FaceDistance);
                    MVector3.Round(ref CollisionPoint, 4);

                    _newCubePlace = new Location3<int>(_pickedBlock.X, _pickedBlock.Y, _pickedBlock.Z);
                    if (CollisionPoint.X == _pickedBlock.X) _newCubePlace.X--;
                    else
                        if (CollisionPoint.X == _pickedBlock.X + 1) _newCubePlace.X++;
                        else
                            if (CollisionPoint.Y == _pickedBlock.Y) _newCubePlace.Y--;
                            else
                                if (CollisionPoint.Y == _pickedBlock.Y + 1) _newCubePlace.Y++;
                                else
                                    if (CollisionPoint.Z == _pickedBlock.Z) _newCubePlace.Z--;
                                    else
                                        if (CollisionPoint.Z == _pickedBlock.Z + 1) _newCubePlace.Z++;


                    _playerPotentialNewBlock = new BoundingBox(new Vector3(_newCubePlace.X, _newCubePlace.Y, _newCubePlace.Z), new Vector3(_newCubePlace.X + 1, _newCubePlace.Y + 1, _newCubePlace.Z + 1));
                    _isBlockPicked = true;

                    break;
                }
            }

            ////Create the bounding box around the cube !
            //if (_previousPickedBlock != _pickedBlock && _isBlockPicked)
            //{
            //    _playerSelectedBox = new BoundingBox(new Vector3(_pickedBlock.X - 0.002f, _pickedBlock.Y - 0.002f, _pickedBlock.Z - 0.002f), new Vector3(_pickedBlock.X + 1.002f, _pickedBlock.Y + 1.002f, _pickedBlock.Z + 1.002f));
            //    _blocCursor.Update(ref _playerSelectedBox);
            //}
        }

        private void inputHandler()
        {
           
            if (_actions.isTriggered(Actions.Block_Add))
            {
                //Enable Single block impact ==> For Testing purpose, shoul dbe removed ==============================================
                if (_isBlockPicked)
                {
                    EntityImpact.ReplaceBlock(ref _pickedBlock, CubeId.Air);
                }
                //Enable Single block impact ==> For Testing purpose, shoul dbe removed ==============================================
            }

            if (_actions.isTriggered(Actions.Block_Remove))
            {

                //Avoid the player to add a block where he is located !
                if (_isBlockPicked)
                {
                    if (!MBoundingBox.Intersects(ref BoundingBox, ref _playerPotentialNewBlock) && _playerPotentialNewBlock.Maximum.Y <= AbstractChunk.ChunkSize.Y - 2)
                    {
                        EntityImpact.ReplaceBlock(ref _newCubePlace, CubeId.Gravel);
                    }
                }
                //Enable Single block impact ==> For Testing purpose, shoul dbe removed ==============================================
            }

            //Did I use the scrollWheel
            //if (_actions.isTriggered(Actions.Block_SelectNext))
            //{
            //    _buildingCubeIndex++;
            //    if (_buildingCubeIndex >= VisualCubeProfile.CubesProfile.Length) _buildingCubeIndex = 1;

            //    _buildingCube = VisualCubeProfile.CubesProfile[_buildingCubeIndex];
            //}

            //if (_actions.isTriggered(Actions.Block_SelectPrevious))
            //{
            //    _buildingCubeIndex--;
            //    if (_buildingCubeIndex <= 0) _buildingCubeIndex = VisualCubeProfile.CubesProfile.Length - 1;
            //    _buildingCube = VisualCubeProfile.CubesProfile[_buildingCubeIndex];
            //}
        }

        private void CheckHeadUnderWater()
        {
            if (_cubesHolder.IndexSafe(MathHelper.Fastfloor(CameraWorldPosition.X), MathHelper.Fastfloor(CameraWorldPosition.Y), MathHelper.Fastfloor(CameraWorldPosition.Z), out _headCubeIndex))
            {
                //Get the cube at the camera position !
                _headCube = _cubesHolder.Cubes[_headCubeIndex];
                if (_headCube.Id == CubeId.Water || _headCube.Id == CubeId.WaterSource)
                {
                    //TODO Take into account the Offseting in case of Offseted Water !
                    HeadInsideWater = true;
                }
                else
                {
                    HeadInsideWater = false;
                }
            }
        }
        #endregion

        #region Public Methods
        public override void Draw()
        {
            base.Draw();
        }

        public override void Update(ref GameTime timeSpent)
        {
            if (IsPlayerConstroled)
            {
                inputHandler();

                //Block Picking !?
                GetSelectedBlock();

                CheckHeadUnderWater();
            }

            base.Update(ref timeSpent);
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            base.Interpolation(ref interpolationHd, ref interpolationLd);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion
    }
}
