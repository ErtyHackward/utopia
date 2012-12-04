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
    public class Sprite3DBillBoardProc : BaseComponent, ISprite3DProcessor
    {
        #region Private Variables
        private List<VertexPointSprite3D> _spritesCollection;
        private VertexBuffer<VertexPointSprite3D> _vb;
        private bool _isCollectionDirty;
        private HLSLPointSprite3DBillBoard _effect;
        private ShaderResourceView _texture;
        private SamplerState _spriteSampler;
        private Include _sharedCBIncludeHandler;
        private iCBuffer _frameSharedCB;
        #endregion

        #region Public Properties
        #endregion
        public Sprite3DBillBoardProc(ShaderResourceView texture, SamplerState SpriteSampler, Include sharedCBIncludeHandler, iCBuffer frameSharedCB)
        {
            _texture = texture;
            _spriteSampler = SpriteSampler;
            _sharedCBIncludeHandler = sharedCBIncludeHandler;
            _frameSharedCB = frameSharedCB;
        }

        #region Public Methods
        public void Init(DeviceContext context, ResourceUsage usage = ResourceUsage.Dynamic)
        {
            _effect = ToDispose(new HLSLPointSprite3DBillBoard(context.Device, @"Effects\Sprites\PointSprite3DBillBoard.hlsl", VertexPointSprite3D.VertexDeclaration, _frameSharedCB, _sharedCBIncludeHandler));

            //Set the Texture
            _effect.DiffuseTexture.Value = _texture;
            _effect.SamplerDiffuse.Value = _spriteSampler;

            _spritesCollection = new List<VertexPointSprite3D>();
            _vb = new VertexBuffer<VertexPointSprite3D>(context.Device, 16, VertexPointSprite3D.VertexDeclaration, PrimitiveTopology.PointList, "VB Sprite3DBillBoardProcessor", usage, 10);
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

        public void Draw(ref Vector3 worldPosition, ref Vector2 size, ref ByteColor color, int textureArrayIndex = 0)
        {
            _spritesCollection.Add(new VertexPointSprite3D(new Vector4(worldPosition.X, worldPosition.Y, worldPosition.Z, textureArrayIndex), color, size));
            _isCollectionDirty = true;
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
