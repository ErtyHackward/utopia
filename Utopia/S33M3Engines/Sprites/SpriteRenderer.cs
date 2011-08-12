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

namespace S33M3Engines.Sprites
{
    public class SpriteRenderer
    {
        public static int MaxBatchSize = 1000;

        private HLSLSprites _effect;
        private HLSLSprites _effectInstanced;
        private SamplerState _spriteSampler;
        private Game _game;

        private int _rasterStateId, _blendStateId, _depthStateId;

        private IndexBuffer<short> _iBuffer;
        private VertexBuffer<VertexSprite> _vBuffer;
        private InstancedVertexBuffer<VertexSprite, VertexSpriteInstanced> _vBufferInstanced;

        private VertexSpriteInstanced[] _textDrawData = new VertexSpriteInstanced[MaxBatchSize];

        public enum FilterMode
        {
            DontSet = 0,
            Linear = 1,
            Point = 2
        };

        public void Initialize(Game game)
        {
            _game = game;
            _effect = new HLSLSprites(game, @"D3D\Effects\Basics\Sprites.hlsl", VertexSprite.VertexDeclaration);
            _effectInstanced = new HLSLSprites(game, @"D3D\Effects\Basics\Sprites.hlsl", VertexSpriteInstanced.VertexDeclaration, new D3D.Effects.EntryPoints() { VertexShader_EntryPoint = "SpriteInstancedVS", PixelShader_EntryPoint = "SpritePS" });


            _spriteSampler = new SamplerState(_game.GraphicDevice,
                                                        new SamplerStateDescription()
                                                        {
                                                            AddressU = TextureAddressMode.Clamp,
                                                            AddressV = TextureAddressMode.Clamp,
                                                            AddressW = TextureAddressMode.Clamp,
                                                            Filter = Filter.MinMagMipPoint,
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

            _vBuffer = new VertexBuffer<VertexSprite>(game, vertices.Length, VertexSprite.VertexDeclaration, PrimitiveTopology.TriangleList, ResourceUsage.Immutable);
            _vBuffer.SetData(vertices);

            _vBufferInstanced = new InstancedVertexBuffer<VertexSprite, VertexSpriteInstanced>(game, VertexSpriteInstanced.VertexDeclaration, PrimitiveTopology.TriangleList);
            _vBufferInstanced.SetFixedData(vertices);

            // Create the instance data buffer
            // TO DO

            // Create the index buffer
            short[] indices = { 0, 1, 2, 3, 0, 2 };
            _iBuffer = new IndexBuffer<short>(game, indices.Length, SharpDX.DXGI.Format.R16_UInt);
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

            //Send index buffer to Device
            _iBuffer.SetToDevice(0);

            // Set the states
            StatesRepository.ApplyStates(_rasterStateId, _blendStateId, _depthStateId);

            //Change the Sampler Filter Mode ==> Need external Sampler for it ! At this moment it is forced inside the shader !
        }

        public void Render(SpriteTexture spriteTexture, ref Matrix transform, Vector4 color, RectangleF sourceRect = default(RectangleF), bool sourceRectInTextCoord = true)
        {
            _vBuffer.SetToDevice(0); // Set the Vertex buffer

            //Set Par Batch Constant
            _effect.Begin();

            _effect.CBPerDraw.Values.ViewportSize = new Vector2(_game.ActivCamera.Viewport.Width, _game.ActivCamera.Viewport.Height);
            if (sourceRectInTextCoord) _effect.CBPerDraw.Values.TextureSize = new Vector2(spriteTexture.TextureDescr.Width, spriteTexture.TextureDescr.Height);
            else _effect.CBPerDraw.Values.TextureSize = new Vector2(1, 1);
            
            _effect.CBPerDraw.IsDirty = true;
            
            // Set per-instance data
            _effect.CBPerInstance.Values.Transform = Matrix.Transpose(transform);
            _effect.CBPerInstance.Values.Color = color;
            if (sourceRect == default(RectangleF))
            {
                if (sourceRectInTextCoord) _effect.CBPerInstance.Values.SourceRect = new RectangleF(0, 0, spriteTexture.TextureDescr.Width, spriteTexture.TextureDescr.Height);
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

            _game.D3dEngine.Context.DrawIndexed(6, 0, 0);
        }

        public void RenderBatch(SpriteTexture spriteTexture, VertexSpriteInstanced[] drawData, bool sourceRectInTextCoord = true)
        {
            RenderBatch(spriteTexture, drawData, drawData.Length, sourceRectInTextCoord);
        }

        public void RenderBatch(SpriteTexture spriteTexture, VertexSpriteInstanced[] drawData, int numSprites, bool sourceRectInTextCoord = true)
        {
            //Set Par Batch Constant
            _effectInstanced.Begin();
            _effectInstanced.CBPerDraw.Values.ViewportSize = new Vector2(_game.ActivCamera.Viewport.Width, _game.ActivCamera.Viewport.Height);
            if(sourceRectInTextCoord)_effectInstanced.CBPerDraw.Values.TextureSize = new Vector2(spriteTexture.TextureDescr.Width, spriteTexture.TextureDescr.Height);
            else _effectInstanced.CBPerDraw.Values.TextureSize = new Vector2(1, 1);
            _effectInstanced.CBPerDraw.IsDirty = true;
            _effectInstanced.SpriteTexture.Value = spriteTexture.Texture;
            _effectInstanced.SpriteTexture.IsDirty = true;
            _effectInstanced.Apply();

            //// Make sure the draw rects are all valid
            //for (int i = drawOffset; i < drawData.Length - drawOffset; ++i)
            //{
            //    Vector4 drawRect = drawData[i].SourceRect;
            //    if (drawRect.X < 0 || drawRect.X >= spriteTexture.TextureDescr.Width ||
            //        drawRect.Y >= 0 && drawRect.Y < spriteTexture.TextureDescr.Height ||
            //        drawRect.Z > 0 && drawRect.X + drawRect.Z <= spriteTexture.TextureDescr.Width ||
            //        drawRect.W > 0 && drawRect.Y + drawRect.W <= spriteTexture.TextureDescr.Height)
            //    {
            //        Console.WriteLine("ERREUR Rectangle source en dehors texture !!!!");
            //    }
            //}

            int numSpritesToDraw = Math.Min(numSprites, MaxBatchSize);

            // Copy the Data inside the vertex buffer
            _vBufferInstanced.SetInstancedData(drawData);
            _vBufferInstanced.SetToDevice(0);

            _game.D3dEngine.Context.DrawIndexedInstanced(6, numSpritesToDraw, 0, 0, 0);
            // If there's any left to be rendered, do it recursively
            if (numSprites > numSpritesToDraw)
                RenderBatch(spriteTexture, drawData, numSprites - numSpritesToDraw);

        }

        public void RenderText(SpriteFont font, string text, Matrix transform, Color4 color)
        {
            int length = text.Length;
            Matrix textTransform = Matrix.Identity;

            int numCharsToDraw = Math.Min(length, MaxBatchSize);
            int currentDraw = 0;
            for (int i = 0; i < numCharsToDraw; ++i)
            {
                char character = text[i];
                if (character == ' ')
                    textTransform.M41 += font.SpaceWidth;
                else if (character == '\n')
                {
                    textTransform.M42 += font.CharHeight;
                    textTransform.M41 = 0;
                }
                else
                {
                    S33M3Engines.Sprites.SpriteFont.CharDesc desc = font.CharDescriptors[character];

                    _textDrawData[currentDraw].Tranform = textTransform * transform;
                    _textDrawData[currentDraw].Color = color;
                    _textDrawData[currentDraw].SourceRect.X = desc.X;
                    _textDrawData[currentDraw].SourceRect.Y = desc.Y;
                    _textDrawData[currentDraw].SourceRect.Z = desc.Width;
                    _textDrawData[currentDraw].SourceRect.W = desc.Height;
                    currentDraw++;

                    textTransform.M41 += desc.Width + 1;
                }
            }

            // Submit a batch
            RenderBatch(font.SpriteTexture, _textDrawData, currentDraw);

            if (length > numCharsToDraw)
                RenderText(font, text + numCharsToDraw, textTransform, color);
        }

        public void End()
        {
        }

        public void Dispose()
        {
            _effect.Dispose();
            _effectInstanced.Dispose();
            _iBuffer.Dispose();
            _vBuffer.Dispose();
            _vBufferInstanced.Dispose();
        }

    }
}
