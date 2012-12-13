using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_DXEngine.Main;
using S33M3CoreComponents.Sprites2D;
using S33M3CoreComponents.Sprites3D.Interfaces;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3Resources.Effects.Sprites;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Sprites3D.Processors
{
    public class CubeColorProc : BaseComponent, ISprite3DProcessor
    {
        #region Private Variables
        private List<VertexCubeColor> _spritesCollection;
        private VertexBuffer<VertexCubeColor> _vb;
        private bool _isCollectionDirty;
        private HLSLCubeColorParticule _effect;
        private Include _sharedCBIncludeHandler;
        private iCBuffer _frameSharedCB;
        #endregion

        #region Public Properties
        #endregion
        public CubeColorProc(Include sharedCBIncludeHandler, iCBuffer frameSharedCB)
        {
            _sharedCBIncludeHandler = sharedCBIncludeHandler;
            _frameSharedCB = frameSharedCB;
        }

        #region Public Methods
        public void Init(DeviceContext context, ResourceUsage usage = ResourceUsage.Dynamic)
        {
            _effect = ToDispose(new HLSLCubeColorParticule(context.Device, @"Effects\Sprites\PointSpriteColor3DBillBoard.hlsl", VertexCubeColor.VertexDeclaration, _frameSharedCB, _sharedCBIncludeHandler));

            _spritesCollection = new List<VertexCubeColor>();
            _vb = new VertexBuffer<VertexCubeColor>(context.Device, 16, VertexCubeColor.VertexDeclaration, PrimitiveTopology.PointList, "VB Sprite3DColorBillBoardProcessor", usage, 10);
            _isCollectionDirty = false;
        }

        public void Begin()
        {
            _spritesCollection.Clear(); //Free buffer;
        }

        public void SetData(DeviceContext context)
        {
            if (_isCollectionDirty)
            {
                _vb.SetData(context, _spritesCollection.ToArray());
                _isCollectionDirty = false;
            }
        }

        public void Set2DeviceAndDraw(DeviceContext context)
        {
            if (_vb.VertexCount == 0) return;

            //Set Effect Constant Buffer
            _effect.Begin(context);
            _effect.Apply(context);

            _vb.SetToDevice(context, 0);
            context.Draw(_vb.VertexCount, 0);
        }

        public void Draw(ref Vector4 position, ref ByteColor color, ref ByteColor ambiantColor, ref Matrix tranform)
        {
            _spritesCollection.Add(new VertexCubeColor(ref position, ref color, ref ambiantColor, ref tranform));
            _isCollectionDirty = true;
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
