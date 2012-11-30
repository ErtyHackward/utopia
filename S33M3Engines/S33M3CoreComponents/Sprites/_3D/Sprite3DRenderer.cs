using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_DXEngine.Main;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Effects.Sprites;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Sprites._3D
{
    /// <summary>
    /// Class that will handle rendering of sprite in 3 dimension
    /// Those rendering sprite can be billboard type
    /// </summary>
    public class Sprite3DRenderer : BaseComponent
    {
        public enum Sprite3dBufferType
        {
            Sprite3D,
            Sprite3DWithTexCoord
        }

        public enum SpriteRenderingType
        {
            Billboard = 0,
            BillboardOnLookAt = 1
        }

        #region Private Variables
        private SpriteTexture _texture;
        private HLSLPointSprite3D _effect;
        private SamplerState _spriteSampler;
        private SpriteFont _font;
        private int _rasterStateId;
        private int _blendStateId; 
        private int _depthStateId;
        private Sprite3dBufferType _sprite3DBufferType;

        private ISprite3DBuffer _processor;
        #endregion

        #region Public Properties
        #endregion

        public Sprite3DRenderer(DeviceContext context,
                                SpriteTexture texture,
                                Sprite3dBufferType SpriteProcessorType,
                                SamplerState SpriteSampler,
                                int RasterStateId,
                                int BlendStateId,
                                int DepthStateId
                                )
        {
            _spriteSampler = SpriteSampler;
            _rasterStateId = RasterStateId;
            _blendStateId = BlendStateId;
            _depthStateId = DepthStateId;
            _texture = texture;

            _sprite3DBufferType = SpriteProcessorType;

            Initialize(context);
        }

        public Sprite3DRenderer(DeviceContext context,
                                SpriteFont font,
                                Sprite3dBufferType SpriteProcessorType,
                                SamplerState SpriteSampler,
                                int RasterStateId,
                                int BlendStateId,
                                int DepthStateId
                                )
            :this(context, font.SpriteTexture, SpriteProcessorType, SpriteSampler, RasterStateId, BlendStateId, DepthStateId)
        {
            _font = font;
        }

        #region Public Methods
        public void Begin(bool ApplyRenderStates = true)
        {
            _processor.Begin();   //Clear the accumulated buffered data
            if(ApplyRenderStates) SetRenderStates();
        }

        public void ReplayLast(DeviceContext context, 
                               ICamera camera,  
                               bool ApplyRenderStates = true)
        {
            if (ApplyRenderStates) SetRenderStates();
            End(context, camera);
        }

        public void End(DeviceContext context, 
                        ICamera camera)
        {
            _processor.SetData(context); //Send the accumulated buffer to the GC ==> "Only" if Collection are "dirty"
            _effect.Begin(context);
            _effect.CBPerFrameLocal.Values.WorldViewProjection = Matrix.Transpose(camera.ViewProjection3D);
            _effect.CBPerFrameLocal.Values.CameraWorldPosition = camera.WorldPosition.Value.AsVector3();
            _effect.CBPerFrameLocal.Values.LookAt = camera.LookAt.Value;
            _effect.CBPerFrameLocal.IsDirty = true;
            _effect.Apply(context);

            _processor.Set2DeviceAndDraw(context);
        }

        public void Draw(ref Vector3 worldPosition, ref Vector2 size, ref ByteColor color, SpriteRenderingType spriterenderingType, int textureArrayIndex = 0)
        {
            _processor.Draw(ref worldPosition, ref size, ref color, spriterenderingType, textureArrayIndex);
        }

        public void DrawText(string text, ref Vector3 worldPosition, float scaling, ref ByteColor color,ICamera camera, int textureArrayIndex = 0, bool multilineSupport = false, bool XCentered = true)
        {
            _processor.DrawText(text, _font, _texture, ref worldPosition, scaling, ref color, camera, textureArrayIndex, XCentered, multilineSupport);
        }

        #endregion

        #region Private Methods
        private void Initialize(DeviceContext context)
        {
            switch (_sprite3DBufferType)
            {
                case Sprite3dBufferType.Sprite3D:
                    _processor = ToDispose(new Sprite3DBuffer());
                    _effect = ToDispose(new HLSLPointSprite3D(context.Device, @"Effects\Sprites\PointSprite3D.hlsl", VertexPointSprite3D.VertexDeclaration));
                    break;
                case Sprite3dBufferType.Sprite3DWithTexCoord:
                    _processor = new Sprite3DWithTexCoordBuffer(_texture.Width, _texture.Height);
                    _effect = ToDispose(new HLSLPointSprite3D(context.Device, @"Effects\Sprites\PointSprite3DTexCoord.hlsl", VertexPointSprite3DTexCoord.VertexDeclaration));
                    break;
                default:
                    break;
            }

            _effect.DiffuseTexture.Value = _texture.Texture;
            _effect.SamplerDiffuse.Value = _spriteSampler;

            _processor.Init(context, ResourceUsage.Dynamic);
        }

        private void SetRenderStates()
        {
            RenderStatesRepo.ApplyStates(_rasterStateId, _blendStateId, _depthStateId);
        }
        #endregion
    }
}
