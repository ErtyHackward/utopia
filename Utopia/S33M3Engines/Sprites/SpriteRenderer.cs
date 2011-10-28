using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.Buffers;
using S33M3Engines.StatesManager;
using SharpDX.Direct3D;
using RectangleF = System.Drawing.RectangleF;
using S33M3Engines.Shared.Sprites;
using Utopia.Shared.Structs;

namespace S33M3Engines.Sprites
{
    /// <summary>
    /// Provides delayed optimized drawing of the set of sprites.
    /// </summary>
    public class SpriteRenderer : IDisposable
    {
        private HLSLSprites _effect;
        private HLSLSprites _effectInstanced;
        private SamplerState _spriteSampler;
        private D3DEngine _d3DEngine;

        private int _rasterStateId, _blendStateId, _depthStateId;

        private IndexBuffer<short> _iBuffer;
        private VertexBuffer<VertexSprite> _vBuffer;
        private InstancedVertexBuffer<VertexSprite, VertexSpriteInstanced> _vBufferInstanced;

        private float _currentDepth;

        /// <summary>
        /// Provides grouping of draw calls
        /// </summary>
        private readonly SpriteBuffer _spriteBuffer = new SpriteBuffer();
        
        public enum FilterMode
        {
            DontSet = 0,
            Linear = 1,
            Point = 2
        };

        public int DrawCalls
        {
            get { return _spriteBuffer.DrawCalls; }
        }

        public int DrawItems
        {
            get { return _spriteBuffer.TotalItems; }
        }

        public void Initialize(D3DEngine d3DEngine)
        {
            _d3DEngine = d3DEngine;
            _effect = new HLSLSprites(_d3DEngine, @"D3D\Effects\Basics\Sprites.hlsl", VertexSprite.VertexDeclaration);
            _effectInstanced = new HLSLSprites(_d3DEngine, @"D3D\Effects\Basics\Sprites.hlsl", VertexSpriteInstanced.VertexDeclaration, new D3D.Effects.EntryPoints { VertexShader_EntryPoint = "SpriteInstancedVS", PixelShader_EntryPoint = "SpritePS" });


            _spriteSampler = new SamplerState(_d3DEngine.Device,
                                                        new SamplerStateDescription
                                                        {
                                                            AddressU = TextureAddressMode.Clamp,
                                                            AddressV = TextureAddressMode.Clamp,
                                                            AddressW = TextureAddressMode.Clamp,
                                                            Filter = Filter.MinLinearMagMipPoint,
                                                            MaximumLod = float.MaxValue,
                                                            MinimumLod = 0
                                                        });

            _effect.SpriteSampler.Value = _spriteSampler;
            _effectInstanced.SpriteSampler.Value = _spriteSampler;

            // Create the vertex buffer
            VertexSprite[] vertices = { 
                                          new VertexSprite(new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f)),
                                          new VertexSprite(new Vector2(1.0f, 0.0f), new Vector2(1.0f, 0.0f)),
                                          new VertexSprite(new Vector2(1.0f, 1.0f), new Vector2(1.0f, 1.0f)),
                                          new VertexSprite(new Vector2(0.0f, 1.0f), new Vector2(0.0f, 1.0f))
                                      };

            _vBuffer = new VertexBuffer<VertexSprite>(_d3DEngine, vertices.Length, VertexSprite.VertexDeclaration, PrimitiveTopology.TriangleList, "SpriteRenderer_vBuffer", ResourceUsage.Immutable);
            _vBuffer.SetData(vertices);

            _vBufferInstanced = new InstancedVertexBuffer<VertexSprite, VertexSpriteInstanced>(_d3DEngine, VertexSpriteInstanced.VertexDeclaration, PrimitiveTopology.TriangleList);
            _vBufferInstanced.SetFixedData(vertices);

            // Create the instance data buffer
            // TO DO

            // Create the index buffer
            short[] indices = { 0, 1, 2, 3, 0, 2 };
            _iBuffer = new IndexBuffer<short>(_d3DEngine, indices.Length, SharpDX.DXGI.Format.R16_UInt, "SpriteRenderer_iBuffer");
            _iBuffer.SetData(indices);

            // Create our constant buffers
            // ???

            //Create the states
            //Rasters.Default

            _rasterStateId = StatesRepository.AddRasterStates(new RasterizerStateDescription
            {
                IsAntialiasedLineEnabled = false,
                CullMode = CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = 1f,
                IsDepthClipEnabled = false,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0,
            });

            var blendDescr = new BlendStateDescription { IndependentBlendEnable = false, AlphaToCoverageEnable = false };
            for (var i = 0; i < 8; i++)
            {
                blendDescr.RenderTarget[i].IsBlendEnabled = true;
                blendDescr.RenderTarget[i].BlendOperation = BlendOperation.Add;
                blendDescr.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                blendDescr.RenderTarget[i].DestinationBlend = BlendOption.InverseSourceAlpha;
                blendDescr.RenderTarget[i].DestinationAlphaBlend = BlendOption.One;
                blendDescr.RenderTarget[i].SourceBlend = BlendOption.One;
                blendDescr.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
                blendDescr.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            }
            _blendStateId = StatesRepository.AddBlendStates(blendDescr);

            _depthStateId = StatesRepository.AddDepthStencilStates(new DepthStencilStateDescription
            {
                IsDepthEnabled = false,
                DepthComparison = Comparison.LessEqual,
                DepthWriteMask = DepthWriteMask.All,
                IsStencilEnabled = false,
                BackFace = new DepthStencilOperationDescription { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep },
                FrontFace = new DepthStencilOperationDescription { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep }
            });

        }

        /// <summary>
        /// Begins sprites collecting, call End() to perform actual drawing
        /// </summary>
        /// <param name="filterMode"></param>
        public void Begin(FilterMode filterMode = FilterMode.DontSet)
        {
            //Send index buffer to Device
            _iBuffer.SetToDevice(0);

            _spriteBuffer.Clear();

            _currentDepth = 1;

            // Set the states
            StatesRepository.ApplyStates(_rasterStateId, _blendStateId, _depthStateId);

            //Change the Sampler Filter Mode ==> Need external Sampler for it ! At this moment it is forced inside the shader !
        }

        /// <summary>
        /// Flush all sprites to the graphic device
        /// </summary>
        public void End()
        {
            foreach (var drawInfo in _spriteBuffer)
            {
                if (drawInfo.IsGroup)
                {
                    RenderBatch(drawInfo.SpriteTexture, drawInfo.Group);
                }
                else Render(drawInfo.SpriteTexture, drawInfo.Transform, drawInfo.Color4, drawInfo.SourceRect, true, drawInfo.Depth);
            }

            System.Diagnostics.Debug.WriteLine(string.Format("Sprite renderer: {0}/{1}", _spriteBuffer.DrawCalls, _spriteBuffer.TotalItems));
        }

        /// <summary>
        /// Draws single texture
        /// </summary>
        /// <param name="spriteTexture"></param>
        /// <param name="destRect"></param>
        /// <param name="color"></param>
        public void Draw(SpriteTexture spriteTexture, Rectangle destRect, Color color)
        {
            var transform = Matrix.Translation(destRect.Left, destRect.Top, 0);
            Draw(spriteTexture, ref transform, new Color4(color.ToVector4()));
        }

        /// <summary>
        /// Draws single texture
        /// </summary>
        /// <param name="spriteTexture"></param>
        /// <param name="destRect"></param>
        /// <param name="srcRect"></param>
        /// <param name="color"></param>
        /// <param name="sourceRectInTextCoord"></param>
        /// <param name="textureArrayIndex"></param>
        public void Draw(SpriteTexture spriteTexture, Rectangle destRect, Rectangle srcRect, Color color, bool sourceRectInTextCoord = true, int textureArrayIndex = 0)
        {
            var transform = Matrix.Scaling((float)destRect.Width / srcRect.Width, (float)destRect.Height / srcRect.Height, 0) *
                   Matrix.Translation(destRect.Left, destRect.Top, 0);

            var src = new RectangleF(srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height);

            Draw(spriteTexture, ref transform, new Color4(color.ToVector4()), src, sourceRectInTextCoord, textureArrayIndex);
        }
        
        /// <summary>
        /// Draws single texture
        /// </summary>
        /// <param name="spriteTexture"></param>
        /// <param name="transform"></param>
        /// <param name="color"></param>
        /// <param name="sourceRect"></param>
        /// <param name="sourceRectInTextCoord"></param>
        /// <param name="textureArrayIndex"></param>
        public void Draw(SpriteTexture spriteTexture, ref Matrix transform, Color4 color,  RectangleF sourceRect = default(RectangleF), bool sourceRectInTextCoord = true, int textureArrayIndex = 0)
        {
            _currentDepth -= 0.001f;
            _spriteBuffer.Add(spriteTexture, ref transform, color, sourceRect, sourceRectInTextCoord, textureArrayIndex, _currentDepth);
        }

        /// <summary>
        /// Draws sprite batch
        /// </summary>
        /// <param name="spriteTexture"></param>
        /// <param name="drawData"></param>
        /// <param name="sourceRectInTextCoord"></param>
        public void Draw(SpriteTexture spriteTexture, VertexSpriteInstanced[] drawData, bool sourceRectInTextCoord = true)
        {
            Draw(spriteTexture, drawData, drawData.Length, sourceRectInTextCoord);
        }

        /// <summary>
        /// Will issue a draw call with batched drawData
        /// </summary>
        /// <param name="spriteTexture"></param>
        /// <param name="drawData"></param>
        /// <param name="numSprites"></param>
        /// <param name="sourceRectInTextCoord"></param>
        public void Draw(SpriteTexture spriteTexture, VertexSpriteInstanced[] drawData, int numSprites, bool sourceRectInTextCoord = true)
        {
            _spriteBuffer.Add(spriteTexture, drawData);
        }

        //public void Render(SpriteTexture spriteTexture, 
        //                   ref Matrix transform, 
        //                   Color4 color,
        //                   Vector2 viewportSize,
        //                   RectangleF sourceRect = default(RectangleF), 
        //                   bool sourceRectInTextCoord = true)
        //{
        //    _vBuffer.SetToDevice(0); // Set the Vertex buffer

        //    //Set Par Batch Constant
        //    _effect.Begin();

        //    _effect.CBPerDraw.Values.ViewportSize = viewportSize;
        //    _effect.CBPerDraw.Values.TextureSize = sourceRectInTextCoord ? new Vector2(spriteTexture.Width, spriteTexture.Height) : new Vector2(1, 1);

        //    _effect.CBPerDraw.IsDirty = true;

        //    // Set per-instance data
        //    _effect.CBPerInstance.Values.Transform = Matrix.Transpose(transform);
        //    _effect.CBPerInstance.Values.TextureArrayIndex = 0;
        //    _effect.CBPerInstance.Values.Color = color;
        //    if (sourceRect == default(RectangleF))
        //    {
        //        _effect.CBPerInstance.Values.SourceRect = sourceRectInTextCoord ? new RectangleF(0, 0, spriteTexture.Width, spriteTexture.Height) : new RectangleF(0, 0, 1, 1);
        //    }
        //    else
        //    {
        //        _effect.CBPerInstance.Values.SourceRect = sourceRect;
        //    }
        //    _effect.CBPerInstance.IsDirty = true;

        //    _effect.SpriteTexture.Value = spriteTexture.Texture;
        //    _effect.SpriteTexture.IsDirty = true;

        //    _effect.Apply(); //Set Shader to the device

        //    _d3DEngine.Context.DrawIndexed(6, 0, 0);
        //}

        /// <summary>
        /// Draw some text
        /// </summary>
        /// <param name="spriteFont"></param>
        /// <param name="text"></param>
        /// <param name="pos"></param>
        /// <param name="color"></param>
        public void DrawText(SpriteFont spriteFont, string text, Vector2 pos, Color color)
        {
            var transform = Matrix.Translation(pos.X, pos.Y, 0);

            //TODO color vs color4
            DrawText(spriteFont, text, transform, new ByteColor(color));
        }

        /// <summary>
        /// Draws some text
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="transform"></param>
        /// <param name="color"></param>
        /// <param name="lineDefaultOffset"></param>
        public void DrawText(SpriteFont font, string text, Matrix transform, ByteColor color, float lineDefaultOffset = -1)
        {
            var length = text.Length;
            var textTransform = Matrix.Identity;

            var numCharsToDraw = length;

            if(lineDefaultOffset == -1) lineDefaultOffset = transform.M41;

            var list = new List<VertexSpriteInstanced>();

            _currentDepth -= 0.001f;

            for (int i = 0; i < numCharsToDraw; ++i)
            {
                char character = text[i];
                if (character == ' ')
                    //next character will be a little bit more right
                    textTransform.M41 += font.SpaceWidth;
                else if (character == '\n')
                {
                    //next character will be at next line
                    textTransform.M42 += font.CharHeight;
                    textTransform.M41 = 0;
                    transform.M41 = lineDefaultOffset;
                }
                else
                {
                    //New character
                    var desc = font.CharDescriptors[character];

                    list.Add(new VertexSpriteInstanced
                        {
                            Tranform = textTransform * transform,
                            SourceRect = desc,
                            Color = color,
                            Depth = _currentDepth
                        });

                    textTransform.M41 += desc.Width + 1;
                }
            }

            

            // Submit a batch
            Draw(font.SpriteTexture, list.ToArray());

            if (length > numCharsToDraw)
                DrawText(font, text.Substring(numCharsToDraw - 1), textTransform * transform, color, lineDefaultOffset);
        }

        /// <summary>
        /// Performs single texture rendering
        /// </summary>
        /// <param name="spriteTexture"></param>
        /// <param name="transform"></param>
        /// <param name="color"></param>
        /// <param name="sourceRect"></param>
        /// <param name="sourceRectInTextCoord"></param>
        private void Render(SpriteTexture spriteTexture, Matrix transform, Color4 color, RectangleF sourceRect = default(RectangleF), bool sourceRectInTextCoord = true, float depth = 0)
        {
            _vBuffer.SetToDevice(0); // Set the Vertex buffer

            //Set Par Batch Constant
            _effect.Begin();

            _effect.CBPerDraw.Values.ViewportSize = new Vector2(_d3DEngine.ViewPort.Width, _d3DEngine.ViewPort.Height);
            _effect.CBPerDraw.Values.TextureSize = sourceRectInTextCoord ? new Vector2(spriteTexture.Width, spriteTexture.Height) : new Vector2(1, 1);

            _effect.CBPerDraw.IsDirty = true;

            // Set per-instance data
            _effect.CBPerInstance.Values.Transform = Matrix.Transpose(transform);
            _effect.CBPerInstance.Values.TextureArrayIndex = 0;
            _effect.CBPerInstance.Values.Color = color;
            _effect.CBPerInstance.Values.Depth = depth;
            if (sourceRect == default(RectangleF))
            {
                _effect.CBPerInstance.Values.SourceRect = sourceRectInTextCoord ? new RectangleF(0, 0, spriteTexture.Width, spriteTexture.Height) : new RectangleF(0, 0, 1, 1);
            }
            else
            {
                _effect.CBPerInstance.Values.SourceRect = sourceRect;
            }
            _effect.CBPerInstance.IsDirty = true;

            _effect.SpriteTexture.Value = spriteTexture.Texture;
            _effect.SpriteTexture.IsDirty = true;

            _effect.Apply(); //Set Shader to the device

            _d3DEngine.Context.DrawIndexed(6, 0, 0);
        }

        /// <summary>
        /// Performs batch rendering
        /// </summary>
        /// <param name="spriteTexture"></param>
        /// <param name="drawData"></param>
        /// <param name="sourceRectInTextCoord"></param>
        private void RenderBatch(SpriteTexture spriteTexture, VertexSpriteInstanced[] drawData, bool sourceRectInTextCoord = true)
        {
            //Set Par Batch Constant
            _effectInstanced.Begin();

            _effectInstanced.CBPerDraw.Values.ViewportSize = new Vector2(_d3DEngine.ViewPort.Width, _d3DEngine.ViewPort.Height);
            _effectInstanced.CBPerDraw.Values.TextureSize = sourceRectInTextCoord ? new Vector2(spriteTexture.Width, spriteTexture.Height) : new Vector2(1, 1);
            _effectInstanced.CBPerDraw.IsDirty = true;
            _effectInstanced.SpriteTexture.Value = spriteTexture.Texture;
            _effectInstanced.SpriteTexture.IsDirty = true;
            _effectInstanced.Apply();

            // Copy the Data inside the vertex buffer
            _vBufferInstanced.SetInstancedData(drawData);
            _vBufferInstanced.SetToDevice(0);

            _d3DEngine.Context.DrawIndexedInstanced(6, drawData.Length, 0, 0, 0);
        }


        public void Dispose()
        {
            //_spriteSampler.Dispose();
            _effect.Dispose();
            _effectInstanced.Dispose();
            _iBuffer.Dispose();
            _vBuffer.Dispose();
            _vBufferInstanced.Dispose();
        }

    }
}
