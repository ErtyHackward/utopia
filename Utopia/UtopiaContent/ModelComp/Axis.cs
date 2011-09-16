using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Engines.Struct;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.D3D;
using S33M3Engines.StatesManager;
using S33M3Engines.Buffers;
using S33M3Engines.D3D.Effects.Basics;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Shared.Structs;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.GameStates;

namespace UtopiaContent.ModelComp
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Axis : DrawableGameComponent
    {
        float _axisSize;
        D3DEngine _d3dEngine;
        CameraManager _camManager;
        HLSLVertexPositionColor _wrappedEffect;
        //Buffer _vertexBuffer;
        VertexBuffer<VertexPositionColor> _vertexBuffer;
        GameStatesManager _gameStates;
        int _renderRasterId;

        public Axis(D3DEngine d3dEngine, CameraManager camManager ,float AxisSize, GameStatesManager gameStates)
        {
            _gameStates = gameStates;
            _d3dEngine = d3dEngine;
            _camManager = camManager;
            this._axisSize = AxisSize;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void LoadContent()
        {
            _renderRasterId = StatesRepository.AddRasterStates(new RasterizerStateDescription() { CullMode = SharpDX.Direct3D11.CullMode.None, FillMode = FillMode.Solid });

            _wrappedEffect = new HLSLVertexPositionColor(_d3dEngine, @"D3D\Effects\Basics\VertexPositionColor.hlsl", VertexPositionColor.VertexDeclaration);
            //_wrappedEffect.CurrentTechnique = _wrappedEffect.Effect.GetTechniqueByIndex(0); ==> Optional, defaulted to 0 !

            CreateVertexBuffer();

            base.LoadContent();
        }

        private void CreateVertexBuffer()
        {
            // Create the 3 Lines

            List<VertexPositionColor> ptList = new List<VertexPositionColor>();
            ptList.AddRange(new Line3D(new Vector3(0, 0, 0) * _axisSize, new Vector3(1f, 0, 0) * _axisSize, Color.Red).PointsList);
            ptList.AddRange(new Line3D(new Vector3(0, 0, 0) * _axisSize, new Vector3(0, 1f, 0) * _axisSize, Color.Green).PointsList);
            ptList.AddRange(new Line3D(new Vector3(0, 0, 0) * _axisSize, new Vector3(0, 0, 1f) * _axisSize, Color.Blue).PointsList);

            _vertexBuffer = new VertexBuffer<VertexPositionColor>(_d3dEngine, 6, VertexPositionColor.VertexDeclaration, PrimitiveTopology.LineList);
            _vertexBuffer.SetData(ptList.ToArray());
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(ref GameTime TimeSpend)
        {
            base.Update(ref TimeSpend);
        }

        public override void Draw(int Index)
        {
            if (_gameStates.DebugDisplay == 1)
            {
                //Set States.
                StatesRepository.ApplyRaster(_renderRasterId);

                //Set Effect variables
                _wrappedEffect.Begin();
                _wrappedEffect.CBPerDraw.Values.World = Matrix.Transpose(Matrix.Identity);
                _wrappedEffect.CBPerDraw.IsDirty = true;
                _wrappedEffect.CBPerFrame.Values.View = Matrix.Transpose(_camManager.ActiveCamera.View_focused);
                _wrappedEffect.CBPerFrame.Values.Projection = Matrix.Transpose(_camManager.ActiveCamera.Projection3D);
                _wrappedEffect.CBPerFrame.IsDirty = true;
                _wrappedEffect.Apply();

                //Set the vertex buffer to the Graphical Card.
                _vertexBuffer.SetToDevice(0);

                _d3dEngine.Context.Draw(6, 0); //2 Vertex by line ! ==> 6 vertex to draw

            }
        }

        public override void UnloadContent()
        {
            if (_vertexBuffer != null) _vertexBuffer.Dispose();
            _wrappedEffect.Dispose();

            base.UnloadContent();
        }
    }
}
