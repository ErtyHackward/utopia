using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.Cameras;
using SharpDX;
using S33M3Engines.Maths;
using Utopia.Planets.Terran;
using Utopia.Planets.Terran.World;
using Utopia.Planets.Terran.Cube;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.Struct;
using S33M3Engines.InputHandler;
using System.Windows.Forms;
using UtopiaContent.ModelComp;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.D3D.Effects.Basics;
using Utopia.Shared.Structs;
using Utopia.Shared.Landscaping;
using Utopia.Shared;
using Utopia.Settings;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;
using S33M3Engines.Shared.Sprites;
using S33M3Engines;
using S33M3Engines.WorldFocus;

namespace Utopia.Entities.Living
{
    public class Player : LivingEntity, ILivingEntity
    {
        #region private/public variables
        string _name;
        Location3<int> _pickedBlock, _previousPickedBlock, _newCubePlace;
        bool _isBlockPicked;

        int _buildingCubeIndex;
        RenderCubeProfile _buildingCube;

        //Bloc Cursor Variables
        BoundingBox _playerSelectedBox, _playerPotentialNewBlock;
        BoundingBox3D _blocCursor;
        HLSLVertexPositionColor _cursorEffect;
        Color _cursorColor = Color.Red; //Color.FromNonPremultiplied(30,30,30, 255);
        WorldFocusManager _worldFocusManager;
        #endregion

        #region public properties

        public string Name
        {
            get { return _name; }
        }

        public PlayerInventory Inventory { get; private set; }
        
        #endregion

        public Player(D3DEngine d3dEngine, CameraManager camManager, WorldFocusManager worldFocusManager ,string Name, ICamera camera, InputHandlerManager inputHandler, DVector3 startUpWorldPosition, Vector3 size, float walkingSpeed, float flyingSpeed, float headRotationSpeed)
            : base(d3dEngine, camManager, inputHandler, startUpWorldPosition, size, walkingSpeed, flyingSpeed, headRotationSpeed)
        {
            _worldFocusManager = worldFocusManager;
            _name = Name;
            Inventory = new PlayerInventory();
            Pickaxe tool = new Pickaxe();
            tool.AllowedSlots = InventorySlot.Bags;
            tool.Icon = new SpriteTexture(_d3dEngine.Device, @"Textures\pickaxe-icon.png", new Vector2(0, 0));

            Armor ring = new Armor();
            ring.AllowedSlots = InventorySlot.Bags | InventorySlot.LeftRing; //FIXME slot system is ko
            ring.Icon = new SpriteTexture(_d3dEngine.Device, @"Textures\ring-icon.png", new Vector2(0, 0));


            Inventory.bag.Items = new List<Item>();
            Inventory.bag.Items.Add(tool);
            Inventory.bag.Items.Add(ring);

        }

        #region private methods
        private void GetSelectedBlock()
        {
            _isBlockPicked = false;

            _previousPickedBlock = _pickedBlock;

            Vector3 pickingPointInLine = (WorldPosition.Value + _entityEyeOffset).AsVector3();
            //Sample 500 points in the view direction vector
            for (int ptNbr = 0; ptNbr < 500; ptNbr++)
            {
                pickingPointInLine += LookAt * 0.02f;

                if (TerraWorld.Landscape.isPickable(ref pickingPointInLine))
                {

                    _pickedBlock.X = MathHelper.Fastfloor(pickingPointInLine.X);
                    _pickedBlock.Y = MathHelper.Fastfloor(pickingPointInLine.Y);
                    _pickedBlock.Z = MathHelper.Fastfloor(pickingPointInLine.Z);

                    //Find the face picked up !
                    float FaceDistance;
                    Ray newRay = new Ray((WorldPosition.Value + _entityEyeOffset).AsVector3(), LookAt);
                    BoundingBox bBox = new SharpDX.BoundingBox(new Vector3(_pickedBlock.X, _pickedBlock.Y, _pickedBlock.Z), new Vector3(_pickedBlock.X + 1, _pickedBlock.Y + 1, _pickedBlock.Z + 1));
                    newRay.Intersects(ref bBox, out FaceDistance);

                    Vector3 CollisionPoint = ((WorldPosition.Value + _entityEyeOffset).AsVector3()) + (LookAt * FaceDistance);
                    MVector3.Round(ref CollisionPoint, 5);
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

            //Create the bounding box around the cube !
            if (_previousPickedBlock != _pickedBlock && _isBlockPicked)
            {
                _playerSelectedBox = new BoundingBox(new Vector3(_pickedBlock.X - 0.002f, _pickedBlock.Y - 0.002f, _pickedBlock.Z - 0.002f), new Vector3(_pickedBlock.X + 1.002f, _pickedBlock.Y + 1.002f, _pickedBlock.Z + 1.002f));
                _blocCursor.Update(ref _playerSelectedBox);
            }
        }

        bool KeyMBuffer, LButtonBuffer, RButtonBuffer, WheelForward, WheelBackWard;
        private void InputHandler(bool bufferMode)
        {
            if ((_inputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.Move.Mode)) || KeyMBuffer)
            {
                if (bufferMode) { KeyMBuffer = true; return; } else KeyMBuffer = false;

                if (Mode == LivingEntityMode.FreeFirstPerson)
                {
                    Mode = LivingEntityMode.WalkingFirstPerson;
                    MoveSpeed = WalkingSpeed;
                }
                else
                {
                    Mode = LivingEntityMode.FreeFirstPerson;
                    MoveSpeed = FlyingSpeed;
                }

                //GameConsole.Write(Name + " : Mode change : " + Mode.ToString());
            }

            if ((_inputHandler.PrevKeyboardState.IsKeyUp(Keys.LButton) && _inputHandler.CurKeyboardState.IsKeyDown(Keys.LButton)) || LButtonBuffer)
            {
                if (bufferMode) { LButtonBuffer = true; return; } else LButtonBuffer = false;

                if (_isBlockPicked)
                {
                    if (_inputHandler.CurKeyboardState.IsKeyDown(Keys.LShiftKey))
                    {
                        //EntityImpact.TnT(ref _pickedBlock, CubeType.Air);
                    }
                    else
                    {
                        EntityImpact.ReplaceBlock(ref _pickedBlock, CubeId.Air);
                    }
                }
            }

            //Did I use the scrollWheel
            if (((_inputHandler.CurMouseState.ScrollWheelTicks - _inputHandler.PrevMouseState.ScrollWheelTicks) != 0) || WheelForward || WheelBackWard)
            {
                if (bufferMode)
                {
                    if (_inputHandler.CurMouseState.ScrollWheelTicks > _inputHandler.PrevMouseState.ScrollWheelTicks) WheelForward = true;
                    else WheelBackWard = true;
                    return;
                }

                if (_inputHandler.CurMouseState.ScrollWheelTicks > _inputHandler.PrevMouseState.ScrollWheelTicks || WheelForward)
                {
                    _buildingCubeIndex++;
                    if (_buildingCubeIndex >= RenderCubeProfile.CubesProfile.Length) _buildingCubeIndex = 1;

                    _buildingCube = RenderCubeProfile.CubesProfile[_buildingCubeIndex];
                }
                else
                {
                    _buildingCubeIndex--;
                    if (_buildingCubeIndex <= 0) _buildingCubeIndex = RenderCubeProfile.CubesProfile.Length - 1;
                    _buildingCube = RenderCubeProfile.CubesProfile[_buildingCubeIndex];
                }

                WheelForward = false;
                WheelBackWard = false;
            }

            if ((_inputHandler.PrevKeyboardState.IsKeyUp(Keys.RButton) && _inputHandler.CurKeyboardState.IsKeyDown(Keys.RButton)) || RButtonBuffer)
            {
                if (bufferMode) { RButtonBuffer = true; return; } else RButtonBuffer = false;

                if (_isBlockPicked)
                {
                    if (!MBoundingBox.Intersects(ref _boundingBox, ref _playerPotentialNewBlock) && _playerPotentialNewBlock.Maximum.Y <= LandscapeBuilder.Worldsize.Y - 2)
                    {
                        EntityImpact.ReplaceBlock(ref _newCubePlace, _buildingCube.Id);
                    }
                }
            }
        }
        #endregion

        #region public methods

        public override void LoadContent()
        {
            _buildingCubeIndex = 1;
            _buildingCube = RenderCubeProfile.CubesProfile[_buildingCubeIndex];

            _cursorEffect = new HLSLVertexPositionColor(_d3dEngine, @"D3D/Effects/Basics/VertexPositionColor.hlsl", VertexPositionColor.VertexDeclaration);
            _blocCursor = new BoundingBox3D(_d3dEngine, _worldFocusManager, new Vector3(1.004f, 1.004f, 1.004f), _cursorEffect, _cursorColor);
        }

        public override void UnloadContent()
        {
            if (_cursorEffect != null) _cursorEffect.Dispose();
            if (_blocCursor != null) _blocCursor.Dispose();
        }

        public int ind;
        public override void Update(ref GameTime TimeSpend)
        {
            base.Update(ref TimeSpend);

            //Block Picking !?
            if (TerraWorld != null)
            {
                GetSelectedBlock();
                ind = TerraWorld.Landscape.Index(_pickedBlock.X, _pickedBlock.Y, _pickedBlock.Z);

                //Handle Specific User Keyboard/Mouse Action !
                InputHandler(false);

                //Head Under Water ??
                RefreshHeadUnderWater();
            }
        }

        public override void DrawDepth2()
        {
            if (_isBlockPicked)
                _blocCursor.Draw(_camManager.ActiveCamera, _worldFocusManager.WorldFocus);

            base.DrawDepth2();
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            //Handle Specific User Keyboard/Mouse Action !
            InputHandler(true);

            base.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        public override string GetInfo()
        {
            return string.Concat("<IPerson : Player (", _name, ")> X : ", (WorldPosition.ActualValue.X - LandscapeBuilder.WorldStartUpX).ToString("0.0"), " Y : ", WorldPosition.ActualValue.Y.ToString("0.0"), " Z : ", (WorldPosition.ActualValue.Z - LandscapeBuilder.WorldStartUpZ).ToString("0.0"), " Block Focus : ", _isBlockPicked ? _pickedBlock.ToString() : "None", " Block Add : ", _isBlockPicked ? _newCubePlace.ToString() : "None", " CubeType : ", _buildingCube.Name);
        }
        #endregion

    }
}
