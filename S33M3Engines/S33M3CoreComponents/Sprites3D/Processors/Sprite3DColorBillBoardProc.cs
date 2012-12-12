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
    public class Sprite3DColorBillBoardProc : BaseComponent, ISprite3DProcessor
    {
        #region Private Variables
        private List<VertexPointSprite3DExt1> _spritesCollection;
        private VertexBuffer<VertexPointSprite3DExt1> _vb;
        private bool _isCollectionDirty;
        private HLSLPointSpriteColor3DBillBoard _effect;
        private Include _sharedCBIncludeHandler;
        private iCBuffer _frameSharedCB;
        #endregion

        #region Public Properties
        #endregion
        public Sprite3DColorBillBoardProc(Include sharedCBIncludeHandler, iCBuffer frameSharedCB)
        {
            _sharedCBIncludeHandler = sharedCBIncludeHandler;
            _frameSharedCB = frameSharedCB;
        }

        #region Public Methods
        public void Init(DeviceContext context, ResourceUsage usage = ResourceUsage.Dynamic)
        {
            _effect = ToDispose(new HLSLPointSpriteColor3DBillBoard(context.Device, @"Effects\Sprites\PointSpriteColor3DBillBoard.hlsl", VertexPointSprite3DExt1.VertexDeclaration, _frameSharedCB, _sharedCBIncludeHandler));

            _spritesCollection = new List<VertexPointSprite3DExt1>();
            _vb = new VertexBuffer<VertexPointSprite3DExt1>(context.Device, 16, VertexPointSprite3DExt1.VertexDeclaration, PrimitiveTopology.PointList, "VB Sprite3DColorBillBoardProcessor", usage, 10);
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

        public void Draw(ref Vector3 worldPosition, ref Vector2 size, ref ByteColor color, ref ByteColor colorReceived)
        {
            _spritesCollection.Add(new VertexPointSprite3DExt1(new Vector4(worldPosition.X, worldPosition.Y, worldPosition.Z, 0), color, colorReceived, size));
            _isCollectionDirty = true;
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
