using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Planets.Terran.Cube;
using S33M3Engines.Struct;
using SharpDX;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.Threading;
using Utopia.Planets.Terran.World;
using S33M3Engines.D3D;
using SharpDX.Direct3D11;
using UtopiaContent.ModelComp;
using S33M3Engines.Maths;
using SharpDX.Direct3D;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Landscaping;

namespace Utopia.Planets.Terran.Chunk
{
    public class TerraChunk : baseChunk
    {
        #region Private Variables
        Dictionary<string, int> _solidCubeVerticeDico; // Dictionnary used in the mesh creation, to avoid to recreate a vertex that has already been used create for another cube.

        //List are use instead of standard array because it's not possible to know the number of vertices/indices that will be produced at cubes creation time.
        //After vertex/index buffer creation those collections are cleared.
        List<VertexCubeSolid> _solidCubeVertices;      // Collection use to collect the vertices at the solid cube creation time
        List<ushort> _solidCubeIndices;                // Collection use to collect the indices at the solid cube creation time
        List<VertexCubeLiquid> _liquidCubeVertices;    // Collection use to collect the vertices at the liquid cube creation time
        List<ushort> _liquidCubeIndices;               // Collection use to collect the indices at the liquid cube creation time
        #endregion

        #region public Properties
        //Graphical chunk components Exposed VB and IB ==> Called a lot, so direct acces without property bounding
        public VertexBuffer<VertexCubeSolid> SolidCubeVB;   //Solid cube vertex Buffer
        public IndexBuffer<ushort> SolidCubeIB;             //Solid cube index buffer

        public VertexBuffer<VertexCubeLiquid> LiquidCubeVB; //Liquid cube vertex Buffer
        public IndexBuffer<ushort> LiquidCubeIB;            //Liquid cube index Buffer
        #endregion

        public TerraChunk(Game game, Range<int> CubeRange, LandScape Landscape, TerraWorld World)
            : base(game, CubeRange, Landscape, World)
        {
        }

        #region private methods
        object _Lock_DrawChunksSolidFaces = new object();       //Multithread Locker
        object _Lock_DrawChunksSeeThrough1Faces = new object(); //Multithread Locker
        #endregion

        #region public methods

        #region ChunkLandscape ==> Vertex & Index Buffer !

        //Creation of the cubes meshes, Face type by face type
        public override void CreateCubeMeshes()
        {
            //Instanciate the various collection objects that wil be used during the cube mesh creations
            _solidCubeVerticeDico = new Dictionary<string, int>();
            _solidCubeVertices = new List<VertexCubeSolid>();
            _solidCubeIndices = new List<ushort>();
            _liquidCubeVertices = new List<VertexCubeLiquid>();
            _liquidCubeIndices = new List<ushort>();

            GenerateCubesFace(CubeFace.Front);
            GenerateCubesFace(CubeFace.Back);
            GenerateCubesFace(CubeFace.Bottom);
            GenerateCubesFace(CubeFace.Top);
            GenerateCubesFace(CubeFace.Left);
            GenerateCubesFace(CubeFace.Right);
        }

        private void GenerateCubesFace(CubeFace cubeFace)
        {
            TerraCube currentCube, neightborCube;
            RenderCubeProfile cubeProfile;

            ByteVector4 cubePosiInChunk;
            Location3<int> cubePosiInWorld;
            int XWorld, YWorld, ZWorld;
            int cubeIndex, neightborCubeIndex;

            for (int X = 0; X < LandscapeBuilder.Chunksize; X++)
            {
                XWorld = (X + _cubeRange.Min.X);
                    
                for (int Z = 0; Z < LandscapeBuilder.Chunksize; Z++)
                {
                    ZWorld = (Z + _cubeRange.Min.Z);

                    //by moving it there an reordering the loops, we avoid doing this operations 128 times for each chunk
                    int offset = MathHelper.Mod(XWorld, LandscapeBuilder.Worldsize.X)
                            + MathHelper.Mod(ZWorld, LandscapeBuilder.Worldsize.Z) * LandscapeBuilder.Worldsize.X;

                    
                    for (int Y = 0; Y < _cubeRange.Max.Y; Y++)
                    {

                        //_cubeRange in fact identify the chunk, the chunk position in the world being _cubeRange.Min
                        YWorld = (Y + _cubeRange.Min.Y);
                    
                        //Inlinning Fct for speed !
                        //==> Could use this instead of the next line ==> cubeIndex = _landscape.Index(XWorld, YWorld, ZWorld);
                        //The "problem" being that everytime this fonction is called, it is creation a copy of the parameter variables, this fct is being called 
                        //A lot of time, to easy the GAC work, it's better to inline the fct here.
                        cubeIndex =  offset + YWorld * LandscapeBuilder.Worldsize.X * LandscapeBuilder.Worldsize.Z;

                        //_landscape.Cubes[] is the BIG table containing all terraCube in the visible world.
                        //For speed access, I use an array with only one dimension, thus the table index must be computed from the X, Y, Z position of the terracube.
                        //Computing myself this index, is faster than using an array defined as [x,y,z]
                        // ? Am I an Air Cube ? ==> Default Value, not needed to render !
                        if (_landscape.Cubes[cubeIndex].Id == CubeId.Air || _landscape.Cubes[cubeIndex].Id == CubeId.Error) continue;

                        //Terra Cube contain only the data that are variables, and could be different between 2 cube.
                        currentCube = _landscape.Cubes[cubeIndex];
                        //The Cube profile contain the value that are fixe for a block type.
                        cubeProfile = RenderCubeProfile.CubesProfile[currentCube.Id];

                        cubePosiInWorld = new Location3<int>(XWorld, YWorld, ZWorld);
                        cubePosiInChunk = new ByteVector4(X, Y, Z);

                        IdxRelativeMove neightRelativeMove;
                        int relativeMoveFrom;

                        //Check to see if the face needs to be generated or not !
                        //Border Chunk test ! ==> Don't generate faces that are "border" chunks
                        //BorderChunk value is true if the chunk is at the border of the visible world.
                        switch (cubeFace)
                        {
                            case CubeFace.Back:
                                if (BorderChunk && (ZWorld - 1 < _terraWorld.WorldRange.Min.Z)) continue;
                                neightRelativeMove = IdxRelativeMove.Z_Minus1;
                                relativeMoveFrom = ZWorld;
                                break;
                            case CubeFace.Front:
                                if (BorderChunk && (ZWorld + 1 >= _terraWorld.WorldRange.Max.Z)) continue;
                                neightRelativeMove = IdxRelativeMove.Z_Plus1;
                                relativeMoveFrom = ZWorld;
                                break;
                            case CubeFace.Bottom:
                                if (YWorld - 1 < 0) continue;
                                neightRelativeMove = IdxRelativeMove.Y_Minus1;
                                relativeMoveFrom = YWorld;
                                break;
                            case CubeFace.Top:
                                if (YWorld + 1 >= _terraWorld.WorldRange.Max.Y) continue;
                                neightRelativeMove = IdxRelativeMove.Y_Plus1;
                                relativeMoveFrom = YWorld;
                                break;
                            case CubeFace.Left:
                                if (BorderChunk && (XWorld - 1 < _terraWorld.WorldRange.Min.X)) continue;
                                neightRelativeMove = IdxRelativeMove.X_Minus1;
                                relativeMoveFrom = XWorld;
                                break;
                            case CubeFace.Right:
                                if (BorderChunk && (XWorld + 1 >= _terraWorld.WorldRange.Max.X)) continue;
                                neightRelativeMove = IdxRelativeMove.X_Plus1;
                                relativeMoveFrom = XWorld;
                                break;
                            default:
                                neightRelativeMove = IdxRelativeMove.Z_Minus1;
                                relativeMoveFrom = ZWorld;
                                throw new NullReferenceException();
                        }

                        //Custom test to see if the face can be generated (Between cube checks)
                        //_landscape.FastIndex is another method of computing index of the big table but faster !
                        //The constraint is that it only compute index of a cube neightbor
                        neightborCubeIndex = _landscape.FastIndex(cubeIndex, relativeMoveFrom, neightRelativeMove); // MathHelper.Mod(XWorld, TerraWorld.Worldsize.X) + MathHelper.Mod(ZWorld - 1, TerraWorld.Worldsize.Z) * TerraWorld.Worldsize.X + YWorld * TerraWorld.Worldsize.X * TerraWorld.Worldsize.Z;
                        neightborCube = _landscape.Cubes[neightborCubeIndex];

                        //It is using a delegate in order to give the possibility for Plugging to replace the fonction call.
                        //Be default the fonction called here is : TerraCube.FaceGenerationCheck or TerraCube.WaterFaceGenerationCheck
                        if (!cubeProfile.CanGenerateCubeFace(ref currentCube, ref cubePosiInWorld, cubeFace, ref neightborCube)) continue;

                        switch (cubeProfile.CubeFamilly)
                        {
                            case enuCubeFamilly.Solid:
                                //Other delegate.
                                //Default linked to : CubeMeshFactory.GenSolidCubeFace;
                                cubeProfile.CreateSolidCubeMesh(ref currentCube, cubeFace, ref cubePosiInChunk, ref cubePosiInWorld, ref _solidCubeVertices, ref _solidCubeIndices, ref _solidCubeVerticeDico);
                                break;
                            case enuCubeFamilly.Liquid:
                                //Default linked to : CubeMeshFactory.GenLiquidCubeFace;
                                cubeProfile.CreateLiquidCubeMesh(ref currentCube, cubeFace, ref cubePosiInChunk, ref cubePosiInWorld, ref _liquidCubeVertices, ref _liquidCubeIndices, ref _solidCubeVerticeDico);
                                break;
                            case enuCubeFamilly.Other:
                                break;
                        }

                    }
                }
            }

        }

        //Thread Entry;
        public object SendCubeMeshesToBuffers_Threaded(object stateInfo)
        {
            SendCubeMeshesToBuffers();
            _threadStatus = ThreadStatus.Idle; //Thread Work finished
            return null;
        }

        public void SendCubeMeshesToBuffers()
        {
            SendSolidCubeMeshToGraphicCard();
            SendLiquidCubeMeshToGraphicCard();

            base.Ready2Draw = true;

            State = ChunkState.DisplayInSyncWithMeshes;
        }

        //Solid Cube
        //Create the VBuffer + IBuffer from the List, then clear the list
        //The Buffers are pushed to the graphic card. (SetData());
        private void SendSolidCubeMeshToGraphicCard()
        {
            lock (_Lock_DrawChunksSolidFaces)
            {
                if (_solidCubeVertices.Count == 0)
                {
                    if (SolidCubeVB != null) SolidCubeVB.Dispose();
                    SolidCubeVB = null;
                    return;
                }

                if (SolidCubeVB == null)
                {
                    SolidCubeVB = new VertexBuffer<VertexCubeSolid>(_game, _solidCubeVertices.Count, VertexCubeSolid.VertexDeclaration, PrimitiveTopology.TriangleList, ResourceUsage.Default, 10);
                }
                SolidCubeVB.SetData(_solidCubeVertices.ToArray());
                _solidCubeVertices.Clear();

                if (SolidCubeIB == null)
                {
                    SolidCubeIB = new IndexBuffer<ushort>(_game, _solidCubeIndices.Count, SharpDX.DXGI.Format.R16_UInt);
                }
                SolidCubeIB.SetData(_solidCubeIndices.ToArray());
                _solidCubeIndices.Clear();

                _solidCubeVerticeDico.Clear();
            }
        }

        //Liquid Cube
        //Create the VBuffer + IBuffer from the List, then clear the list
        //The Buffers are pushed to the graphic card. (SetData());
        private void SendLiquidCubeMeshToGraphicCard()
        {
            lock (_Lock_DrawChunksSeeThrough1Faces)
            {
                if (_liquidCubeVertices.Count == 0)
                {
                    if (LiquidCubeVB != null) LiquidCubeVB.Dispose();
                    LiquidCubeVB = null;
                    return;
                }

                if (LiquidCubeVB == null)
                {
                    LiquidCubeVB = new VertexBuffer<VertexCubeLiquid>(_game, _liquidCubeVertices.Count, VertexCubeLiquid.VertexDeclaration, PrimitiveTopology.TriangleList, ResourceUsage.Default, 10);
                }
                LiquidCubeVB.SetData(_liquidCubeVertices.ToArray());
                _liquidCubeVertices.Clear();

                if (LiquidCubeIB == null)
                {
                    LiquidCubeIB = new IndexBuffer<ushort>(_game, _liquidCubeIndices.Count, SharpDX.DXGI.Format.R16_UInt);
                }
                LiquidCubeIB.SetData(_liquidCubeIndices.ToArray());
                _liquidCubeIndices.Clear();
            }
        }

        //Ask the Graphical card to Draw the solid faces
        public override void DrawSolidFaces()
        {
            lock (_Lock_DrawChunksSolidFaces)
            {
                if (SolidCubeVB != null)
                {
                    SolidCubeVB.SetToDevice(0);
                    SolidCubeIB.SetToDevice(0);
                    _game.D3dEngine.Context.DrawIndexed(SolidCubeIB.IndicesCount, 0, 0);
                }
            }
        }

        //Ask the Graphical card to Draw the liquid faces
        public override void DrawLiquidFaces()
        {
            lock (_Lock_DrawChunksSeeThrough1Faces)
            {
                if (LiquidCubeVB != null)
                {
                    LiquidCubeVB.SetToDevice(0);
                    LiquidCubeIB.SetToDevice(0);
                    _game.D3dEngine.Context.DrawIndexed(LiquidCubeIB.IndicesCount, 0, 0);
                }
            }
        }

        #endregion

        #endregion

        #region IDisposable Members

        public override void Dispose()
        {
            if (SolidCubeIB != null) SolidCubeIB.Dispose();
            if (SolidCubeVB != null) SolidCubeVB.Dispose();
            if (LiquidCubeVB != null) LiquidCubeVB.Dispose();
            if (LiquidCubeIB != null) LiquidCubeIB.Dispose();
            base.Dispose();
        }

        #endregion
    }
}
