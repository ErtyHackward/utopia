using S33M3CoreComponents.Maths;
using S33M3CoreComponents.Sprites2D.Interfaces;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Effects.Sprites;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using SharpDX.Direct3D11;
using Rectangle = SharpDX.Rectangle;

namespace S33M3CoreComponents.Sprites2D
{
    public class SpriteRenderer : BaseComponent, ISpriteRenderer
    {
        public enum TextFontPosition
        {
            RelativeToFontBottom,
            RelativeToFontUp
        }

        #region Private variables
        private D3DEngine _d3DEngine;
        private readonly string _shaderFilePath;
        private SamplerState _spriteSamplerWrap, _spriteSamplerClamp;
        private HLSLSprites2 _effect;
        private int _rasterStateWithoutScissorId, _blendStateId, _depthStateWithDepthId, _depthStateWithoutDepthId, _rasterStateWithScissorId;
        RectangleF _descCarret = default(RectangleF);
        private bool _withDepth;

        private VertexBuffer<VertexSprite2> _vb;
        private IndexBuffer<ushort> _ib;

        private SpriteDrawBuffer _spriteBuffer;
        private bool _isScissorMode;
        private bool _withAlphaClip;
        #endregion

        #region Public variables
        public int BlendStateId
        {
            get { return _blendStateId; }
            set { _blendStateId = value; }
        }

        public SamplerState SpriteSamplerClamp
        {
            get { return _spriteSamplerClamp; }
            set { _spriteSamplerClamp = value; }
        }

        public bool IsScissorMode
        {
            get { return _isScissorMode; }
            set
            {
                if (_isScissorMode != value)
                {
                    _isScissorMode = value;
                    SetRenderStates(_isScissorMode, _d3DEngine.ImmediateContext);
                }
            }
        }

        public int DrawCalls;
        public int SpritesDraw;
        #endregion

        public SpriteRenderer(D3DEngine d3DEngine, bool withAlphaClip = true)
            : this(d3DEngine, @"Effects\Sprites\Sprites2.hlsl", withAlphaClip)
        {
        }

        public SpriteRenderer(D3DEngine d3DEngine, string shaderFilePath, bool withAlphaClip = true)
        {
            _d3DEngine = d3DEngine;
            _shaderFilePath = shaderFilePath;
            _withAlphaClip = withAlphaClip;
            Initialize();
        }

        #region Public methods

        public void Begin(bool withDepth, DeviceContext context)
        {
            DrawCalls = 0;
            SpritesDraw = 0;
            _withDepth = withDepth;
            _spriteBuffer.Reset(withDepth);
            SetRenderStates(false, context);
        }

        public void Restart()
        {
            DrawCalls = 0;
            SpritesDraw = 0;
            _spriteBuffer.Restart(_withDepth);
        }

        public void ReplayLast(DeviceContext context)
        {
            SetRenderStates(false, context);
            End(context);
        }

        public void EndWithCustomProjection(DeviceContext context, ref Matrix Projection2D)
        {
            //The projection stay the same !
            _effect.CBPerDraw.Values.OrthoProjection = Matrix.Transpose(Projection2D);
            _effect.CBPerDraw.IsDirty = true;

            foreach (var spriteGroup in _spriteBuffer.GetAllSpriteGroups())
            {
                _vb.SetData(context, spriteGroup.Vertices.ToArray());
                _ib.SetData(context, spriteGroup.Indices.ToArray());

                _effect.Begin(context);
                _effect.SpriteTexture.Value = spriteGroup.Texture.Texture;
                _effect.CBTextureTransform.Values.TexMatrix = Matrix.Transpose(spriteGroup.TextureMatrix);
                _effect.CBTextureTransform.IsDirty = true;
                _effect.Apply(context);

                _vb.SetToDevice(context, 0);
                _ib.SetToDevice(context, 0);

                context.DrawIndexed(_ib.IndicesCount, 0, 0);
                DrawCalls++;
                SpritesDraw += _ib.IndicesCount / 6;
            }
        }

        public void End(DeviceContext context, bool unbindResource = false)
        {
            //The OrthoProjection stay the same all the time
            _effect.CBPerDraw.Values.OrthoProjection = Matrix.Transpose(_d3DEngine.Projection2D);
            _effect.CBPerDraw.IsDirty = true;

            //ForEach sprite group
            foreach (SpriteDrawInfo spriteGroup in _spriteBuffer.GetAllSpriteGroups())
            {
                _vb.SetData(context, spriteGroup.Vertices.ToArray());
                _ib.SetData(context, spriteGroup.Indices.ToArray());
                
                _effect.Begin(context);
                _effect.SpriteTexture.Value = spriteGroup.Texture.Texture;

                _effect.CBTextureTransform.Values.TexMatrix = Matrix.Transpose(spriteGroup.TextureMatrix);
                _effect.CBTextureTransform.IsDirty = true;

                _effect.SpriteSampler.Value = spriteGroup.TextureSampler;

                _effect.Apply(context);

                _vb.SetToDevice(context, 0);
                _ib.SetToDevice(context, 0);

                context.DrawIndexed(_ib.IndicesCount, 0, 0);
                DrawCalls++;
                SpritesDraw += _ib.IndicesCount / 6;
            }

            //Will unbind the used resource but the shadder (Not needed if resources (textures) are not shares with another components)
            if (unbindResource)
            {
                _effect.UnBindResources(context);
            }
        }

        /// <summary>
        /// Draw Sprite
        /// </summary>
        /// <param name="spriteTexture">The texture that will be used</param>
        /// <param name="destRect">The destination rectangle = drawing translation offset + scaling</param>
        /// <param name="color">Color Modifiers</param>
        /// <param name="textureArrayIndex"> </param>
        public void Draw(SpriteTexture spriteTexture, ref Rectangle destRect, ref ByteColor color, int textureArrayIndex = 0, int drawGroupId = 0)
        {
            Vector2 position = new Vector2(destRect.X, destRect.Y);
            Vector2 size = new Vector2(destRect.Width, destRect.Height);
            Draw(spriteTexture, ref position, ref size, ref color, textureArrayIndex, drawGroupId);
        }

        public void Draw(SpriteTexture spriteTexture, Rectangle destRect, ByteColor color, int textureArrayIndex = 0, int drawGroupId = 0)
        {
            Draw(spriteTexture, ref destRect, ref color, textureArrayIndex, drawGroupId);
        }

        public void Draw(SpriteTexture spriteTexture, ref Vector2 position, ref Vector2 size, ref ByteColor color, int textureArrayIndex = 0, int drawGroupId = 0)
        {
            _spriteBuffer.AddSprite(spriteTexture, _spriteSamplerClamp, ref position, ref size, textureArrayIndex, ref color, drawGroupId);
        }

        public void Draw(SpriteTexture spriteTexture, Vector2 position, Vector2 size, ByteColor color, int textureArrayIndex = 0, int drawGroupId = 0)
        {
            _spriteBuffer.AddSprite(spriteTexture, _spriteSamplerClamp, ref position, ref size, textureArrayIndex, ref color, drawGroupId);
        }

        public void DrawWithWrapping(SpriteTexture spriteTexture, ref Vector2 position, ref Vector2 size, ref ByteColor color, int textureArrayIndex = 0, int drawGroupId = 0)
        {
            _spriteBuffer.AddWrappingSprite(spriteTexture, _spriteSamplerWrap, ref position, ref size, textureArrayIndex, ref color, drawGroupId);
        }

        public void DrawWithWrapping(SpriteTexture spriteTexture, ref Rectangle destRect, ref Rectangle srcRect, ref ByteColor color, int textureArrayIndex = 0, bool sourceRectInTextCoord = true, int drawGroupId = 0)
        {
            Vector2 position = new Vector2(destRect.Left, destRect.Top);
            Vector2 size = new Vector2(destRect.Width, destRect.Height);

            var src = new RectangleF(srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height);

            _spriteBuffer.AddWrappingSprite(spriteTexture, _spriteSamplerWrap, ref position, ref size, ref src, textureArrayIndex, ref color, drawGroupId);
        }

        public void Draw(SpriteTexture spriteTexture, ref Rectangle destRect, ref Rectangle srcRect, ref ByteColor color, int textureArrayIndex = 0, bool sourceRectInTextCoord = true, int drawGroupId = 0)
        {
            Draw(spriteTexture, ref destRect, ref srcRect, ref color, 0.0f, _spriteSamplerClamp, textureArrayIndex, sourceRectInTextCoord, drawGroupId);
        }

        public void Draw(SpriteTexture spriteTexture, ref Rectangle destRect, ref Rectangle srcRect, ref ByteColor color, float textureRotation, SamplerState sampler, int textureArrayIndex = 0, bool sourceRectInTextCoord = true, int drawGroupId = 0)
        {
            Vector2 position = new Vector2(destRect.Left, destRect.Top);
            Vector2 size = new Vector2(destRect.Width, destRect.Height);

            var src = new RectangleF(srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height);

            _spriteBuffer.AddSprite(spriteTexture, sampler, ref position, ref size, ref src, sourceRectInTextCoord, textureArrayIndex, ref color, drawGroupId, textureRotation, float.NaN);
        }

        public void DrawText(SpriteFont spriteFont, string text, ref Vector2 position, ref ByteColor color, float maxWidth = -1, int withCarret = -1, TextFontPosition textFontPosition = TextFontPosition.RelativeToFontUp, int drawGroupId = 0)
        {
            SpriteFont.WordInfo[] infos;
            Vector2 textPosition = position;
            if (textFontPosition == TextFontPosition.RelativeToFontBottom) textPosition.Y -= spriteFont.CharHeight; //remove the char. height

            bool isFirstChar = true;
            int length = text.Length;
            if (length == 0 && withCarret == -1) return;

            if (withCarret > -1)
            {
                _descCarret = spriteFont.CharDescriptors['|'];
            }
            withCarret--;
            _spriteBuffer.AutoDepth -= 0.001f;
            int numCharsToDraw = length;

            //Get the SpriteDrawInfo group for this Font
            SpriteDrawInfo spritesDrawFont = _spriteBuffer.GetSpriteDrawInfo(spriteFont.SpriteTexture, _spriteSamplerClamp, drawGroupId);

            //If Carret at starup position
            if (withCarret == -1)
            {
                spritesDrawFont.AddSprite(ref textPosition, ref _descCarret, true, 0, ref color, _spriteBuffer.AutoDepth); //Add Carret at startUp text Position
            }
            if (maxWidth == -1)
            {
                infos = new[] { new SpriteFont.WordInfo { IndexStart = 0, Length = numCharsToDraw } };
            }
            else
            {
                spriteFont.MeasureStringWords(text, maxWidth, out infos);
            }

            //For each Words
            char previousChar = (char)0;
            float currentLineWidth = 0;
            foreach (SpriteFont.WordInfo info in infos)
            {
                // Check if we need to start a new line
                if (info.Width == -1 ||
                    currentLineWidth + info.Width > maxWidth && info.Width < maxWidth)
                {
                    InsertNewLine(ref textPosition, ref currentLineWidth, spriteFont, position.X);
                    //continue;
                }

                //For Each character in the word
                for (int i = info.IndexStart; i < info.IndexStart + info.Length; i++)
                {
                    char character = text[i];

                    if (character == '\r')
                    {
                        continue;
                    }

                    //Managing Space
                    if (character == ' ')
                    {
                        textPosition.X += spriteFont.SpaceWidth;
                    }
                    else
                    {
                        //Managing New Line
                        if (character == '\n')
                        {
                            InsertNewLine(ref textPosition, ref currentLineWidth, spriteFont, position.X);
                        }
                        else
                        {
                            //Apply Kerning
                            if (!isFirstChar && spriteFont.WithKerning)
                            {
                                int kerningAmount = spriteFont.GetKerning(previousChar, character);
                                textPosition.X += kerningAmount;
                            }

                            //All other characters goes here
                            RectangleF desc = spriteFont.CharDescriptors[character];
                            spritesDrawFont.AddSprite(ref textPosition, ref desc, true, 0, ref color, _spriteBuffer.AutoDepth); //Add Carret at startUp text Position

                            textPosition.X += desc.Width;
                            previousChar = character;
                            isFirstChar = false;
                        }
                    }

                    //Display carret ??
                    if (i == withCarret) spritesDrawFont.AddSprite(ref textPosition, ref _descCarret, true, 0, ref color, _spriteBuffer.AutoDepth); //Add Carret at startUp text Position
                }

                //If in word max mode, then add a space after the word
                if (maxWidth > -1)
                {
                    //Display carret ??
                    if (info.Width > -1) textPosition.X += spriteFont.SpaceWidth;
                    currentLineWidth += info.Width + spriteFont.SpaceWidth;
                }

            }

            if (length > numCharsToDraw) DrawText(spriteFont, text.Substring(numCharsToDraw - 1), ref textPosition, ref color, maxWidth, withCarret);

        }
        #endregion

        #region Private methods
        private void InsertNewLine(ref Vector2 textPosition, ref float currentLineWidth, SpriteFont spriteFont, float xOffset)
        {
            textPosition.Y += spriteFont.CharHeight;
            textPosition.X = xOffset;
            currentLineWidth = 0;
        }

        private void SetRenderStates(bool scissorMode, DeviceContext context)
        {
            RenderStatesRepo.ApplyStates(context, scissorMode ? _rasterStateWithScissorId : _rasterStateWithoutScissorId, _blendStateId, _withDepth ? _depthStateWithDepthId : _depthStateWithoutDepthId);
        }

        private void Initialize()
        {

            _spriteSamplerWrap = ToDispose(new SamplerState(_d3DEngine.Device,
                                            new SamplerStateDescription
                                            {
                                                AddressU = TextureAddressMode.Wrap,
                                                AddressV = TextureAddressMode.Wrap,
                                                AddressW = TextureAddressMode.Wrap,
                                                //Filter = Filter.MinLinearMagMipPoint,
                                                Filter = Filter.MinMagMipPoint, // MinMagMipPoint,
                                                MaximumLod = float.MaxValue,
                                                MinimumLod = 0
                                            }));

            _spriteSamplerClamp = ToDispose(new SamplerState(_d3DEngine.Device,
                            new SamplerStateDescription
                            {
                                AddressU = TextureAddressMode.Clamp,
                                AddressV = TextureAddressMode.Clamp,
                                AddressW = TextureAddressMode.Clamp,
                                Filter = Filter.MinMagMipPoint,
                                MaximumLod = float.MaxValue,
                                MinimumLod = 0
                            }));

            _rasterStateWithoutScissorId = RenderStatesRepo.AddRasterStates(new RasterizerStateDescription
            {
                IsAntialiasedLineEnabled = false,
                CullMode = CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = 1f,
                IsDepthClipEnabled = true,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0,
            });

            _rasterStateWithScissorId = RenderStatesRepo.AddRasterStates(new RasterizerStateDescription
            {
                IsAntialiasedLineEnabled = false,
                CullMode = CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = 1f,
                IsDepthClipEnabled = true,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = true,
                SlopeScaledDepthBias = 0,
            });

            var blendDescr = new BlendStateDescription { IndependentBlendEnable = false, AlphaToCoverageEnable = false };
            for (var i = 0; i < 8; i++)
            {
                blendDescr.RenderTarget[i].IsBlendEnabled = true;
                blendDescr.RenderTarget[i].BlendOperation = BlendOperation.Add;
                blendDescr.RenderTarget[i].AlphaBlendOperation = BlendOperation.Maximum;
                blendDescr.RenderTarget[i].DestinationBlend = BlendOption.InverseSourceAlpha;
                blendDescr.RenderTarget[i].DestinationAlphaBlend = BlendOption.One;
                blendDescr.RenderTarget[i].SourceBlend = BlendOption.SourceAlpha;
                blendDescr.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
                blendDescr.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            }
            _blendStateId = RenderStatesRepo.AddBlendStates(blendDescr);

            _depthStateWithDepthId = RenderStatesRepo.AddDepthStencilStates(new DepthStencilStateDescription
            {
                IsDepthEnabled = true,
                DepthComparison = Comparison.Less,
                DepthWriteMask = DepthWriteMask.All,
                IsStencilEnabled = false,
                BackFace = new DepthStencilOperationDescription { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep },
                FrontFace = new DepthStencilOperationDescription { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep }
            });

            _depthStateWithoutDepthId = RenderStatesRepo.AddDepthStencilStates(new DepthStencilStateDescription
            {
                IsDepthEnabled = false,
                DepthComparison = Comparison.Less,
                DepthWriteMask = DepthWriteMask.All,
                IsStencilEnabled = false,
                BackFace = new DepthStencilOperationDescription { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep },
                FrontFace = new DepthStencilOperationDescription { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep }
            });

            //Create the effect and set the default texture sampler
            _effect = ToDispose(new HLSLSprites2(_d3DEngine.Device, _withAlphaClip, _shaderFilePath));

            //Buffer creation
            _vb = ToDispose(new VertexBuffer<VertexSprite2>(_d3DEngine.Device, 16, SharpDX.Direct3D.PrimitiveTopology.TriangleList, "SpriteRenderer2 VB", ResourceUsage.Dynamic, 20));
            _ib = ToDispose(new IndexBuffer<ushort>(_d3DEngine.Device, 24, "SpriteRenderer2 IB", 20, ResourceUsage.Dynamic));

            //Sprite buffer creation
            _spriteBuffer = new SpriteDrawBuffer();
        }

        #endregion
    }
}
