using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines.D3D;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.Buffers;
using S33M3Engines.StatesManager;
using SharpDX.Direct3D;
using RectangleF = System.Drawing.RectangleF;
using S33M3Engines.Shared.Sprites;
using Utopia.Shared.Structs;

namespace S33M3Engines.Sprites
{
    public class SpriteRenderer : IDisposable
    {
        public static int MaxBatchSize = 1000;

        private HLSLSprites _effect;
        private HLSLSprites _effectInstanced;
        private SamplerState _spriteSampler;
        private D3DEngine _d3dEngine;

        private int _rasterStateId, _blendStateId, _depthStateId;

        private IndexBuffer<short> _iBuffer;
        private VertexBuffer<VertexSprite> _vBuffer;
        private InstancedVertexBuffer<VertexSprite, VertexSpriteInstanced> _vBufferInstanced;

        private VertexSpriteInstanced[] _textDrawData = new VertexSpriteInstanced[MaxBatchSize];

        private int _accumulatedSprites;
        private SpriteTexture _currentSpriteTexture;
        private VertexSpriteInstanced[] _spriteAccumulator = new VertexSpriteInstanced[MaxBatchSize];

        private int _drawCalls = 0;

        public enum FilterMode
        {
            DontSet = 0,
            Linear = 1,
            Point = 2
        };

        public void Initialize(D3DEngine d3dEngine)
        {
            _d3dEngine = d3dEngine;
            _effect = new HLSLSprites(_d3dEngine, @"D3D\Effects\Basics\Sprites.hlsl", VertexSprite.VertexDeclaration);
            _effectInstanced = new HLSLSprites(_d3dEngine, @"D3D\Effects\Basics\Sprites.hlsl", VertexSpriteInstanced.VertexDeclaration, new D3D.Effects.EntryPoints() { VertexShader_EntryPoint = "SpriteInstancedVS", PixelShader_EntryPoint = "SpritePS" });


            _spriteSampler = new SamplerState(_d3dEngine.Device,
                                                        new SamplerStateDescription()
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
                                          new VertexSprite(new Vector2(0.0f, 1.0f), new Vector2(0.0f, 1.0f)),
                                      };

            _vBuffer = new VertexBuffer<VertexSprite>(_d3dEngine, vertices.Length, VertexSprite.VertexDeclaration, PrimitiveTopology.TriangleList, "SpriteRenderer_vBuffer", ResourceUsage.Immutable);
            _vBuffer.SetData(vertices);

            _vBufferInstanced = new InstancedVertexBuffer<VertexSprite, VertexSpriteInstanced>(_d3dEngine, VertexSpriteInstanced.VertexDeclaration, PrimitiveTopology.TriangleList);
            _vBufferInstanced.SetFixedData(vertices);

            // Create the instance data buffer
            // TO DO

            // Create the index buffer
            short[] indices = { 0, 1, 2, 3, 0, 2 };
            _iBuffer = new IndexBuffer<short>(_d3dEngine, indices.Length, SharpDX.DXGI.Format.R16_UInt, "SpriteRenderer_iBuffer");
            _iBuffer.SetData(indices);

            // Create our constant buffers
            // ???

            //Create the states
            //Rasters.Default

            _rasterStateId = StatesRepository.AddRasterStates(new RasterizerStateDescription()
            {
                IsAntialiasedLineEnabled = false,
                CullMode = CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = 1.0f,
                IsDepthClipEnabled = false,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0,
            });

            BlendStateDescription BlendDescr = new BlendStateDescription();
            BlendDescr.IndependentBlendEnable = false;
            BlendDescr.AlphaToCoverageEnable = false;
            for (int i = 0; i < 8; i++)
            {
                BlendDescr.RenderTarget[i].IsBlendEnabled = true;
                BlendDescr.RenderTarget[i].BlendOperation = BlendOperation.Add;
                BlendDescr.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                BlendDescr.RenderTarget[i].DestinationBlend = BlendOption.InverseSourceAlpha;
                BlendDescr.RenderTarget[i].DestinationAlphaBlend = BlendOption.One;
                BlendDescr.RenderTarget[i].SourceBlend = BlendOption.One;
                BlendDescr.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
                BlendDescr.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            }
            _blendStateId = StatesRepository.AddBlendStates(BlendDescr);

            _depthStateId = StatesRepository.AddDepthStencilStates(new DepthStencilStateDescription()
            {
                IsDepthEnabled = false,
                DepthComparison = Comparison.Less,
                DepthWriteMask = DepthWriteMask.All,
                IsStencilEnabled = false,
                BackFace = new DepthStencilOperationDescription() { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep },
                FrontFace = new DepthStencilOperationDescription() { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep }
            });

        }

        public void Begin(FilterMode filterMode = FilterMode.DontSet)
        {
            _drawCalls = 0;
            _accumulatedSprites = 0;
            _currentSpriteTexture = null;

            //Send index buffer to Device
            _iBuffer.SetToDevice(0);

            // Set the states
            StatesRepository.ApplyStates(_rasterStateId, _blendStateId, _depthStateId);

            //Change the Sampler Filter Mode ==> Need external Sampler for it ! At this moment it is forced inside the shader !
        }

        public void Render(SpriteTexture spriteTexture, Rectangle destRect, Rectangle srcRect, Color color)
        {
            Matrix transform = Matrix.Scaling((float)destRect.Width / srcRect.Width, (float)destRect.Height / srcRect.Height, 0) *
                               Matrix.Translation(destRect.Left, destRect.Top, 0);

            System.Drawing.RectangleF src = new System.Drawing.RectangleF(srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height);

            Render(spriteTexture, ref transform, new Color4(color.ToVector4()), src);
        }

        public void Render(SpriteTexture spriteTexture, Rectangle destRect, Color color)
        {
            Matrix transform = Matrix.Translation(destRect.Left, destRect.Top, 0);
            Render(spriteTexture, ref transform, new Color4(color.ToVector4()));
        }

        public void Render(SpriteTexture spriteTexture, ref Matrix transform, Color4 color,  RectangleF sourceRect = default(RectangleF), bool sourceRectInTextCoord = true)
        {
            _drawCalls++;
            _vBuffer.SetToDevice(0); // Set the Vertex buffer

            //Set Par Batch Constant
            _effect.Begin();

            _effect.CBPerDraw.Values.ViewportSize = new Vector2(_d3dEngine.ViewPort.Width, _d3dEngine.ViewPort.Height);
            if (sourceRectInTextCoord) _effect.CBPerDraw.Values.TextureSize = new Vector2(spriteTexture.Width, spriteTexture.Height);
            else _effect.CBPerDraw.Values.TextureSize = new Vector2(1, 1);
            
            _effect.CBPerDraw.IsDirty = true;
            
            // Set per-instance data
            _effect.CBPerInstance.Values.Transform = Matrix.Transpose(transform);
            _effect.CBPerInstance.Values.TextureArrayIndex = 0;
            _effect.CBPerInstance.Values.Color = color;
            if (sourceRect == default(RectangleF))
            {
                if (sourceRectInTextCoord) _effect.CBPerInstance.Values.SourceRect = new RectangleF(0, 0, spriteTexture.Width, spriteTexture.Height);
                else _effect.CBPerInstance.Values.SourceRect = new RectangleF(0, 0, 1, 1);
            }
            else
            {
                _effect.CBPerInstance.Values.SourceRect = sourceRect;
            }
            _effect.CBPerInstance.IsDirty = true;

            _effect.SpriteTexture.Value = spriteTexture.Texture;
            _effect.SpriteTexture.IsDirty = true;

            _effect.Apply(); //Set Shader to the device

            _d3dEngine.Context.DrawIndexed(6, 0, 0);
        }

        public void Render(SpriteTexture spriteTexture, 
                           ref Matrix transform, 
                           Color4 color,
                           Vector2 viewportSize,
                           RectangleF sourceRect = default(RectangleF), 
                           bool sourceRectInTextCoord = true)
        {
            _drawCalls++;
            _vBuffer.SetToDevice(0); // Set the Vertex buffer

            //Set Par Batch Constant
            _effect.Begin();

            _effect.CBPerDraw.Values.ViewportSize = viewportSize;
            if (sourceRectInTextCoord) _effect.CBPerDraw.Values.TextureSize = new Vector2(spriteTexture.Width, spriteTexture.Height);
            else _effect.CBPerDraw.Values.TextureSize = new Vector2(1, 1);

            _effect.CBPerDraw.IsDirty = true;

            // Set per-instance data
            _effect.CBPerInstance.Values.Transform = Matrix.Transpose(transform);
            _effect.CBPerInstance.Values.TextureArrayIndex = 0;
            _effect.CBPerInstance.Values.Color = color;
            if (sourceRect == default(RectangleF))
            {
                if (sourceRectInTextCoord) _effect.CBPerInstance.Values.SourceRect = new RectangleF(0, 0, spriteTexture.Width, spriteTexture.Height);
                else _effect.CBPerInstance.Values.SourceRect = new RectangleF(0, 0, 1, 1);
            }
            else
            {
                _effect.CBPerInstance.Values.SourceRect = sourceRect;
            }
            _effect.CBPerInstance.IsDirty = true;

            _effect.SpriteTexture.Value = spriteTexture.Texture;
            _effect.SpriteTexture.IsDirty = true;

            _effect.Apply(); //Set Shader to the device

            _d3dEngine.Context.DrawIndexed(6, 0, 0);
        }


        /// <summary>
        /// Will be used as an accumulator, will create a draw call at texture change
        /// </summary>
        /// <param name="spriteTexture"></param>
        /// <param name="numSprites"></param>
        /// <param name="sourceRectInTextCoord"></param>
        public void RenderBatch(SpriteTexture spriteTexture, Rectangle destRect, Rectangle srcRect, Color color, bool sourceRectInTextCoord = true, int textureArrayIndex=0)
        {
            Matrix transform = Matrix.Scaling((float)destRect.Width / srcRect.Width, (float)destRect.Height / srcRect.Height, 0) *
                               Matrix.Translation(destRect.Left, destRect.Top, 0);
            VertexSpriteInstanced newSpriteInstace = new VertexSpriteInstanced()
                                        {
                                            Tranform = transform,
                                            SourceRect = new RectangleF(srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height),
                                            Color = new ByteColor(color),
                                            TextureArrayIndex = textureArrayIndex
                                        };

            //Texture change ==> Flush the accomulator by creation a batched draw call
            if(_currentSpriteTexture != null && spriteTexture.GetHashCode() != _currentSpriteTexture.GetHashCode())
            {
                flushAccumulatedSprite();
            }

            _currentSpriteTexture = spriteTexture;
            _spriteAccumulator[_accumulatedSprites] = newSpriteInstace;
            _accumulatedSprites++;

        }

        private void flushAccumulatedSprite()
        {
            if (_accumulatedSprites > 0)
            {
                RenderBatch(_currentSpriteTexture, _spriteAccumulator, _accumulatedSprites);
            }
            _accumulatedSprites = 0;
        }



        public void RenderBatch(SpriteTexture spriteTexture, VertexSpriteInstanced[] drawData, bool sourceRectInTextCoord = true)
        {
            RenderBatch(spriteTexture, drawData, drawData.Length, sourceRectInTextCoord);
        }

        /// <summary>
        /// Will issue a draw call with batched drawData
        /// </summary>
        /// <param name="spriteTexture"></param>
        /// <param name="drawData"></param>
        /// <param name="numSprites"></param>
        /// <param name="sourceRectInTextCoord"></param>
        public void RenderBatch(SpriteTexture spriteTexture, VertexSpriteInstanced[] drawData, int numSprites, bool sourceRectInTextCoord = true)
        {
            _drawCalls++;
            //Set Par Batch Constant
            _effectInstanced.Begin();

            _effectInstanced.CBPerDraw.Values.ViewportSize = new Vector2(_d3dEngine.ViewPort.Width, _d3dEngine.ViewPort.Height);
            if(sourceRectInTextCoord)_effectInstanced.CBPerDraw.Values.TextureSize = new Vector2(spriteTexture.Width, spriteTexture.Height);
            else _effectInstanced.CBPerDraw.Values.TextureSize = new Vector2(1, 1);
            _effectInstanced.CBPerDraw.IsDirty = true;
            _effectInstanced.SpriteTexture.Value = spriteTexture.Texture;
            _effectInstanced.SpriteTexture.IsDirty = true;
            _effectInstanced.Apply();

            //// Make sure the draw rects are all valid
            //for (int i = drawOffset; i < drawData.Length - drawOffset; ++i)
            //{
            //    Vector4 drawRect = drawData[i].SourceRect;
            //    if (drawRect.X < 0 || drawRect.X >= spriteTexture.Width ||
            //        drawRect.Y >= 0 && drawRect.Y < spriteTexture.Height ||
            //        drawRect.Z > 0 && drawRect.X + drawRect.Z <= spriteTexture.Width ||
            //        drawRect.W > 0 && drawRect.Y + drawRect.W <= spriteTexture.Height)
            //    {
            //        Console.WriteLine("ERREUR Rectangle source en dehors texture !!!!");
            //    }
            //}

            int numSpritesToDraw = Math.Min(numSprites, MaxBatchSize);

            // Copy the Data inside the vertex buffer
            _vBufferInstanced.SetInstancedData(drawData);
            _vBufferInstanced.SetToDevice(0);

            _d3dEngine.Context.DrawIndexedInstanced(6, numSpritesToDraw, 0, 0, 0);
            // If there's any left to be rendered, do it recursively
            if (numSprites > numSpritesToDraw)
                RenderBatch(spriteTexture, drawData, numSprites - numSpritesToDraw);

        }

        public void RenderText(SpriteFont spriteFont, string text, Vector2 pos, Color color)
        {
            Matrix transform = Matrix.Translation(pos.X, pos.Y, 0);
            RenderText(spriteFont, text, transform, new ByteColor(color));//TODO color vs color4
        }

        public void RenderText(SpriteFont font, string text, Matrix transform, ByteColor color, float lineDefaultOffset = -1)
        {
            flushAccumulatedSprite();

            int length = text.Length;
            Matrix textTransform = Matrix.Identity;

            int numCharsToDraw = Math.Min(length, MaxBatchSize);
            int currentDraw = 0;

            if(lineDefaultOffset == -1) lineDefaultOffset = transform.M41;

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
                    S33M3Engines.Sprites.SpriteFont.CharDesc desc = font.CharDescriptors[character];

                    _textDrawData[currentDraw].Tranform = textTransform * transform;
                    _textDrawData[currentDraw].Color = color;
                    _textDrawData[currentDraw].SourceRect.X = desc.X;
                    _textDrawData[currentDraw].SourceRect.Y = desc.Y;
                    _textDrawData[currentDraw].SourceRect.Width = desc.Width;
                    _textDrawData[currentDraw].SourceRect.Height = desc.Height;
                    currentDraw++;

                    textTransform.M41 += desc.Width + 1;
                }
            }

            // Submit a batch
            RenderBatch(font.SpriteTexture, _textDrawData, currentDraw);

            if (length > numCharsToDraw)
                RenderText(font, text.Substring(numCharsToDraw - 1), textTransform * transform, color, lineDefaultOffset);
        }

        public void End()
        {
            System.Diagnostics.Debug.WriteLine("Sprite renderer: " + _drawCalls);
            flushAccumulatedSprite();
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
