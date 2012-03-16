using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using SharpDX.Direct3D11;
using SharpDX;
using S33M3Resources.Structs.Vertex;
using RectangleF = System.Drawing.RectangleF;
using SharpDX.Direct3D;
using S33M3Resources.Structs;
using S33M3Resources.Effects.Sprites;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine.RenderStates;

namespace S33M3CoreComponents.Sprites
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
        private bool _withDepth;

        private int _rasterStateWithoutScissorId, _blendStateId, _depthStateWithDepthId, _depthStateWithoutDepthId, _rasterStateWithScissorId;

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

        private bool _IsScissorMode;

        public bool IsScissorMode
        {
            get { return _IsScissorMode; }
            set
            {
                if (_IsScissorMode != value)
                {
                    _IsScissorMode = value;
                    SetRenderStates(_IsScissorMode);
                }
            }
        }


        public void Initialize(D3DEngine d3DEngine)
        {
            _d3DEngine = d3DEngine;
            _effect = new HLSLSprites(_d3DEngine.Device, @"Effects\Sprites\Sprites.hlsl", VertexSprite.VertexDeclaration);
            _effectInstanced = new HLSLSprites(_d3DEngine.Device, @"Effects\Sprites\Sprites.hlsl", VertexSpriteInstanced.VertexDeclaration, new EntryPoints { VertexShader_EntryPoint = "SpriteInstancedVS", PixelShader_EntryPoint = "SpritePS" });


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

            _vBuffer = new VertexBuffer<VertexSprite>(_d3DEngine.Device, vertices.Length, VertexSprite.VertexDeclaration, PrimitiveTopology.TriangleList, "SpriteRenderer_vBuffer", ResourceUsage.Immutable);
            _vBuffer.SetData(_d3DEngine.ImmediateContext, vertices);

            _vBufferInstanced = new InstancedVertexBuffer<VertexSprite, VertexSpriteInstanced>(_d3DEngine.Device, VertexSpriteInstanced.VertexDeclaration, PrimitiveTopology.TriangleList);
            _vBufferInstanced.SetFixedData(vertices);

            // Create the instance data buffer
            // TO DO

            // Create the index buffer
            short[] indices = { 0, 1, 2, 3, 0, 2 };
            _iBuffer = new IndexBuffer<short>(_d3DEngine.Device, indices.Length, SharpDX.DXGI.Format.R16_UInt, "SpriteRenderer_iBuffer");
            _iBuffer.SetData(_d3DEngine.ImmediateContext, indices);

            // Create our constant buffers
            // ???

            //Create the states
            //Rasters.Default

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
                blendDescr.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                blendDescr.RenderTarget[i].DestinationBlend = BlendOption.InverseSourceAlpha;
                blendDescr.RenderTarget[i].DestinationAlphaBlend = BlendOption.One;
                blendDescr.RenderTarget[i].SourceBlend = BlendOption.One;
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

        }

        /// <summary>
        /// Begins sprites collecting, call End() to perform actual drawing
        /// </summary>
        /// <param name="withDepth"></param>
        /// <param name="filterMode"></param>
        public void Begin(DeviceContext context, bool withDepth, FilterMode filterMode = FilterMode.DontSet)
        {
            _withDepth = withDepth;
            //Send index buffer to Device
            _iBuffer.SetToDevice(context, 0);

            _spriteBuffer.Clear();

            _currentDepth = 1;

            SetRenderStates(IsScissorMode);
            // Set the states
            //Change the Sampler Filter Mode ==> Need external Sampler for it ! At this moment it is forced inside the shader !
        }

        private void SetRenderStates(bool scissorMode)
        {
            RenderStatesRepo.ApplyStates(scissorMode ? _rasterStateWithScissorId : _rasterStateWithoutScissorId, _blendStateId, _withDepth ? _depthStateWithDepthId : _depthStateWithoutDepthId);
        }

        /// <summary>
        /// Begins sprites collecting, call End() to perform actual drawing
        /// </summary>
        /// <param name="withDepth"></param>
        /// <param name="filterMode"></param>
        public void Restart(DeviceContext context)
        {
            _iBuffer.SetToDevice(context, 0);
            _spriteBuffer.Clear();
        }

        /// <summary>
        /// Apply a style and performs a rendering
        /// </summary>
        public void ReplayLast()
        {
            RenderStatesRepo.ApplyStates(_rasterStateWithoutScissorId, _blendStateId, _withDepth ? _depthStateWithDepthId : _depthStateWithoutDepthId);
            End();
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
                else Render(drawInfo.SpriteTexture, drawInfo.Transform, drawInfo.Color4, drawInfo.SourceRect, true, drawInfo.Depth, drawInfo.TextureArrayIndex);
            }
        }

        /// <summary>
        /// Draws single texture
        /// </summary>
        /// <param name="spriteTexture"></param>
        /// <param name="destRect"></param>
        /// <param name="color"></param>
        public void Draw(SpriteTexture spriteTexture, Rectangle destRect, Color4 color)
        {
            var transform = Matrix.Translation(destRect.Left, destRect.Top, 0);
            Draw(spriteTexture, ref transform, color);
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
        public void Draw(SpriteTexture spriteTexture, Rectangle destRect, Rectangle srcRect, Color4 color, bool sourceRectInTextCoord = true, int textureArrayIndex = -1)
        {
            var transform = Matrix.Scaling((float)destRect.Width / srcRect.Width, (float)destRect.Height / srcRect.Height, 0) *
                   Matrix.Translation(destRect.Left, destRect.Top, 0);

            var src = new RectangleF(srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height);

            if (textureArrayIndex == -1)
                textureArrayIndex = spriteTexture.Index;

            Draw(spriteTexture, ref transform, color, src, sourceRectInTextCoord, textureArrayIndex);
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
        public void Draw(SpriteTexture spriteTexture, ref Matrix transform, Color4 color, RectangleF sourceRect = default(RectangleF), bool sourceRectInTextCoord = true, int textureArrayIndex = -1)
        {
            if (textureArrayIndex == -1)
                textureArrayIndex = spriteTexture.Index;

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

        /// <summary>
        /// Draw some text
        /// </summary>
        /// <param name="spriteFont"></param>
        /// <param name="text"></param>
        /// <param name="pos"></param>
        /// <param name="color"></param>
        /// <param name="maxWidth"></param>
        public void DrawText(SpriteFont spriteFont, string text, Vector2 pos, ByteColor color, float maxWidth = -1, int withCarret = -1)
        {
            var transform = Matrix.Translation(pos.X, pos.Y, 0);

            DrawText(spriteFont, text, transform, color, maxWidth, -1 , withCarret);
        }

        /// <summary>
        /// Draws some text
        /// </summary>
        /// <param name="font">Font use to render text</param>
        /// <param name="text">The text to render</param>
        /// <param name="transform">The text location, in matrix translation format</param>
        /// <param name="color">The font color</param>
        /// <param name="maxWidth">If set, the renderer will add new line to the text, in order to respect the maximum width passed in</param>
        /// <param name="lineDefaultOffset">If specified, when a new line is created, this value is used as offset from left windo border</param>
        public void DrawText(SpriteFont font, string text, Matrix transform, ByteColor color, float maxWidth = -1, float lineDefaultOffset = -1, int withCarret = -1)
        {
            var length = text.Length;
            var textTransform = Matrix.Identity;

            if (length == 0 && withCarret == -1) return;

            var descCarret = font.CharDescriptors['|'];

            withCarret--;

            var numCharsToDraw = length;

            if (lineDefaultOffset == -1) lineDefaultOffset = transform.M41;

            var list = new List<VertexSpriteInstanced>();

            _currentDepth -= 0.001f;

            if (maxWidth == -1)
            {
                if (withCarret == -1)
                {
                    //New character
                    list.Add(new VertexSpriteInstanced
                    {
                        Tranform = textTransform * transform,
                        SourceRect = descCarret,
                        Color = color,
                        Depth = _currentDepth
                    });
                }

                for (int i = 0; i < numCharsToDraw; ++i)
                {
                    char character = text[i];
                    if (character == ' ')
                    {
                        textTransform.M41 += font.SpaceWidth;

                        //Display carret
                        if (i == withCarret)
                        {
                            //New character
                            list.Add(new VertexSpriteInstanced
                            {
                                Tranform = textTransform * transform,
                                SourceRect = descCarret,
                                Color = color,
                                Depth = _currentDepth
                            });
                        }
                    }
                    //next character will be a little bit more right
                    else if (character == '\n')
                    {
                        //next character will be at next line
                        textTransform.M42 += font.CharHeight;
                        textTransform.M41 = 0;
                        transform.M41 = lineDefaultOffset;

                        //Display carret
                        if (i == withCarret)
                        {
                            //New character
                            list.Add(new VertexSpriteInstanced
                            {
                                Tranform = textTransform * transform,
                                SourceRect = descCarret,
                                Color = color,
                                Depth = _currentDepth
                            });
                        }
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

                        //Display carret
                        if (i == withCarret)
                        {
                            //New character
                            textTransform.M41 -= 1;
                            list.Add(new VertexSpriteInstanced
                            {
                                Tranform = textTransform * transform,
                                SourceRect = descCarret,
                                Color = color,
                                Depth = _currentDepth
                            });
                            textTransform.M41 += 1;
                        }

                    }

                }
            }
            else
            {
                if (withCarret == -1)
                {
                    //New character
                    list.Add(new VertexSpriteInstanced
                    {
                        Tranform = textTransform * transform,
                        SourceRect = descCarret,
                        Color = color,
                        Depth = _currentDepth
                    });
                }

                SpriteFont.WordInfo[] infos;
                // measure the string
                font.MeasureString(text, maxWidth, out infos);

                float currentWidth = 0;

                // draw each word
                foreach (var wordInfo in infos)
                {
                    // draw line breaks '\n'
                    if (wordInfo.Width == -1)
                    {
                        textTransform.M41 = 0;                //Reset X Position to 0
                        textTransform.M42 += font.CharHeight; //Y Position
                        transform.M41 = lineDefaultOffset;    //
                        currentWidth = 0;

                        //New character
                        if (wordInfo.IndexStart == withCarret)
                        {
                            list.Add(new VertexSpriteInstanced
                            {
                                Tranform = textTransform * transform,
                                SourceRect = descCarret,
                                Color = color,
                                Depth = _currentDepth
                            });
                        }
                    }
                    else
                    {
                        // Check if we need to start a new line
                        if (currentWidth + wordInfo.Width > maxWidth && wordInfo.Width < maxWidth)
                        {
                            currentWidth = 0;
                            textTransform.M41 = 0;
                            textTransform.M42 += font.CharHeight;
                            transform.M41 = lineDefaultOffset;
                        }

                        for (int i = wordInfo.IndexStart; i < wordInfo.IndexStart + wordInfo.Length; i++)
                        {
                            var desc = font.CharDescriptors[text[i]];

                            list.Add(new VertexSpriteInstanced
                            {
                                Tranform = textTransform * transform,
                                SourceRect = desc,
                                Color = color,
                                Depth = _currentDepth
                            });

                            textTransform.M41 += desc.Width + 1;

                            //Display carret
                            if (i == withCarret)
                            {
                                //New character
                                textTransform.M41 -= 1;
                                list.Add(new VertexSpriteInstanced
                                {
                                    Tranform = textTransform * transform,
                                    SourceRect = descCarret,
                                    Color = color,
                                    Depth = _currentDepth
                                });
                                textTransform.M41 += 1;
                            }
                        }
                        textTransform.M41 += font.SpaceWidth;
                        currentWidth += wordInfo.Width + font.SpaceWidth;
                    }
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
        private void Render(SpriteTexture spriteTexture, Matrix transform, Color4 color, RectangleF sourceRect = default(RectangleF), bool sourceRectInTextCoord = true, float depth = 0, int textureArrayIndex = -1)
        {
            if (textureArrayIndex == -1)
                textureArrayIndex = spriteTexture.Index;

            _vBuffer.SetToDevice(_d3DEngine.ImmediateContext ,0); // Set the Vertex buffer

            //Set Par Batch Constant
            _effect.Begin(_d3DEngine.ImmediateContext);

            _effect.CBPerDraw.Values.ViewportSize = new Vector2(_d3DEngine.ViewPort.Width, _d3DEngine.ViewPort.Height);
            _effect.CBPerDraw.Values.TextureSize = sourceRectInTextCoord ? new Vector2(spriteTexture.Width, spriteTexture.Height) : new Vector2(1, 1);

            _effect.CBPerDraw.IsDirty = true;

            // Set per-instance data
            _effect.CBPerInstance.Values.Transform = Matrix.Transpose(transform);
            _effect.CBPerInstance.Values.TextureArrayIndex = (uint)textureArrayIndex;
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

            _effect.Apply(_d3DEngine.ImmediateContext); //Set Shader to the device

            _d3DEngine.ImmediateContext.DrawIndexed(6, 0, 0);
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
            _effectInstanced.Begin(_d3DEngine.ImmediateContext);
            _effectInstanced.CBPerDraw.Values.ViewportSize = new Vector2(_d3DEngine.ViewPort.Width, _d3DEngine.ViewPort.Height);
            _effectInstanced.CBPerDraw.Values.TextureSize = sourceRectInTextCoord ? new Vector2(spriteTexture.Width, spriteTexture.Height) : new Vector2(1, 1);
            _effectInstanced.CBPerDraw.IsDirty = true;
            _effectInstanced.SpriteTexture.Value = spriteTexture.Texture;
            _effectInstanced.SpriteTexture.IsDirty = true;
            _effectInstanced.Apply(_d3DEngine.ImmediateContext);

            // Copy the Data inside the vertex buffer
            _vBufferInstanced.SetInstancedData(_d3DEngine.ImmediateContext, drawData);
            _vBufferInstanced.SetToDevice(_d3DEngine.ImmediateContext, 0);

            _d3DEngine.ImmediateContext.DrawIndexedInstanced(6, drawData.Length, 0, 0, 0);
        }

        public void Dispose()
        {
            //_spriteSampler.Dispose();
            _effect.Dispose();
            _effectInstanced.Dispose();
            _iBuffer.Dispose();
            _vBuffer.Dispose();
            _vBufferInstanced.Dispose();
            _spriteSampler.Dispose();
        }

    }
}
