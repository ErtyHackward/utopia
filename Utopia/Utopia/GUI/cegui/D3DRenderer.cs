//#region LGPL License
///*************************************************************************
//    Crazy Eddie's GUI System (http://crayzedsgui.sourceforge.net)
//    Copyright (C)2004 Paul D Turner (crayzed@users.sourceforge.net)

//    C# Port developed by Chris McGuirk (leedgitar@latenitegames.com)
//    Compatible with the Axiom 3D Engine (http://axiomengine.sf.net)

//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.

//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.

//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//*************************************************************************/
//#endregion LGPL License

//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using SharpDX.Direct3D11;
//using S33M3Engines.Struct.Vertex;
//using S33M3Engines.Buffers;
//using S33M3Engines.D3D;
//using SharpDX.Direct3D;
//using S33M3Engines.D3D.Effects.Basics;
//using S33M3Engines.StatesManager;


//namespace CeGui.Renderers.SharpDX
//{

//    /// <summary>Renders GUIs using managed DirectX 2.0 on DirectX 9.0c</summary>
//    public class D3DRenderer : CeGui.Renderer
//    {

//        /// <summary>The size of a single vertex batch</summary>
//        /// <remarks>
//        ///   The optimal size is entirely dependent on the generation of video card used
//        ///   and should not be hardcoded for ideal performance. However, there isn't
//        ///   really that much to gain besides a huge increase in complexity, so we'll use
//        ///   a well-working default size.
//        /// </remarks>
//        public const int VertexBatchSize = 4096;

//        /// <summary>Number of vertex batches in vertex buffer</summary>
//        /// <remarks>
//        ///   Because modern video hardware has the freedom to render multiple frames ahead
//        ///   it is not a good idea to lock and overwrite the same vertex buffer repeatedly.
//        ///   Instead of always locking the vertex buffer in discard mode, we accumulate
//        ///   vertices until the end of the vertex buffer is reached. Only then a single
//        ///   lock in discard mode is done.
//        /// </remarks>
//        public const int VertexBatchCount = 4;



//        /// <summary>Initializes the D3D9 renderer</summary>
//        /// <param name="d3dDevice">Direct3D device that will be used for rendering</param>
//        /// <param name="maxQuads">
//        ///   Maximum number of quads that the Renderer will be able to render per frame
//        /// </param>
//        public D3DRenderer(Game game,  int maxQuads)
//        {
//            this.game = game;
//            this.d3dDevice = game.GraphicDevice;

//            this.maxVertices = maxQuads * 6; // vertices for two triangles in triangle list mode
//            this.vertices = new VertexSprite[this.maxVertices];
//            this.textures = new List<D3DTexture>();

//            ClearRenderList();

//            createVertexBuffer();
//            Initialize();
//        }

//        //copy paste from spriterenderer
//        public void Initialize()
//        {
//            _effect = new HLSLSprites(game, @"D3D\Effects\Basics\Sprites.hlsl", VertexSprite.VertexDeclaration);
         

//            _spriteSampler = new SamplerState(game.GraphicDevice,
//                                                        new SamplerStateDescription()
//                                                        {
//                                                            AddressU = TextureAddressMode.Clamp,
//                                                            AddressV = TextureAddressMode.Clamp,
//                                                            AddressW = TextureAddressMode.Clamp,
//                                                            Filter = Filter.MinMagMipPoint,
//                                                            MaximumLod = float.MaxValue,
//                                                            MinimumLod = 0
//                                                        });

//            _effect.SpriteSampler.Value = _spriteSampler;
       
         
//            _rasterStateId = StatesRepository.AddRasterStates(new RasterizerStateDescription()
//            {
//                IsAntialiasedLineEnabled = false,
//                CullMode = CullMode.None,
//                DepthBias = 0,
//                DepthBiasClamp = 1.0f,
//                IsDepthClipEnabled = false,
//                FillMode = FillMode.Solid,
//                IsFrontCounterClockwise = false,
//                IsMultisampleEnabled = true,
//                IsScissorEnabled = false,
//                SlopeScaledDepthBias = 0,
//            });

//            BlendStateDescription BlendDescr = new BlendStateDescription();
//            BlendDescr.IndependentBlendEnable = false;
//            BlendDescr.AlphaToCoverageEnable = false;
//            for (int i = 0; i < 8; i++)
//            {
//                BlendDescr.RenderTarget[i].IsBlendEnabled = true;
//                BlendDescr.RenderTarget[i].BlendOperation = BlendOperation.Add;
//                BlendDescr.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
//                BlendDescr.RenderTarget[i].DestinationBlend = BlendOption.InverseSourceAlpha;
//                BlendDescr.RenderTarget[i].DestinationAlphaBlend = BlendOption.One;
//                BlendDescr.RenderTarget[i].SourceBlend = BlendOption.One;
//                BlendDescr.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
//                BlendDescr.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
//            }
//            _blendStateId = StatesRepository.AddBlendStates(BlendDescr);

//            _depthStateId = StatesRepository.AddDepthStencilStates(new DepthStencilStateDescription()
//            {
//                IsDepthEnabled = false,
//                DepthComparison = Comparison.Less,
//                DepthWriteMask = DepthWriteMask.All,
//                IsStencilEnabled = false,
//                BackFace = new DepthStencilOperationDescription() { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep },
//                FrontFace = new DepthStencilOperationDescription() { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep }
//            });

//        }


//        /// <summary>Creates the vertex buffer for the renderer</summary>
//        private void createVertexBuffer()
//        {

//            this.vertexBuffer = new VertexBuffer<VertexSprite>(game, vertices.Length, VertexSprite.VertexDeclaration, PrimitiveTopology.TriangleList, ResourceUsage.Immutable);
            
//            /*
//            // Create a new vertex buffer for persistent objects
//            this.vertexBuffer = new VertexBuffer(
//              typeof(CustomVertex.TransformedColoredTextured),
//              this.maxVertices,
//              this.d3dDevice,
//              Usage.Dynamic | Usage.WriteOnly,
//              CustomVertex.TransformedColoredTextured.Format,
//              Pool.Default
//            );*/

           

//        }

//        /// <summary>
//        ///   Direct3D support method that must be called prior to a Reset() call
//        ///   on the Direct3D device
//        /// </summary>
//        public void PreD3DReset()
//        {

//            this.vertexBuffer.Dispose();
//            this.vertexBuffer = null;

//            foreach (D3DTexture texture in this.textures)
//                texture.PreD3DReset();

//        }

//        /// <summary>
//        ///   Direct3D support method that must be called after a Reset() call on the
//        ///   Direct3D device
//        /// </summary>
//        public void PostD3DReset()
//        {

//            createVertexBuffer();

//            // Perform post-reset operations on all textures
//            foreach (D3DTexture texture in this.textures)
//                texture.PostD3DReset();

//            // Update display size not needed because we query it on-the-fly

//            // Now we've come back, we MUST ensure a full redraw is done since the
//            // textures in the stored quads will have been invalidated.
//            GuiSystem.Instance.SignalRedraw();

//        }

//        /// <summary>Add a quad to the rendering queue</summary>
//        /// <remarks>
//        ///   All clipping and other adjustments should have been made prior to calling this
//        /// </remarks>
//        /// <summary>Add a quad to the rendering queue (or render immediately)</summary>
//        /// <param name="destRect">Coordinates at which to draw the quad, in pixels</param>
//        /// <param name="z">Z coordinate at which to draw the quad</param>
//        /// <param name="texture">Texture containing the bitmap to draw onto the quad</param>
//        /// <param name="textureRect">
//        ///   Region within the texture to be drawn onto the quad, in texture coordinates
//        /// </param>
//        /// <param name="colors">Vertex colors for each of the 4 corners</param>
//        /// <param name="quadSplitMode">Where to split the quad into 2 triangles</param>
//        public override void AddQuad(
//          Rect destRect, float z, Texture texture, Rect textureRect,
//          ColourRect colors, QuadSplitMode quadSplitMode
//        )
//        {
//            Texture2D d3dTexture = (texture as D3DTexture).D3DTexture2D;

//            // Is this a quad we should render directly?
//            if (!IsQueueingEnabled)
//            {

//                // Generate the required vertices
//               VertexSprite[] tempVertices =
//                  new VertexSprite[6];

//                generateQuadVertices(
//                  tempVertices, 0,
//                  destRect, z, textureRect, colors, quadSplitMode
//                );

//                // Now all that's left to do is to send this to the graphics card
//                d3dDevice.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
//                d3dDevice.SetTexture(0, d3dTexture);
//                d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleList, 2, tempVertices);

//                // Persistent vertices are to be extended
//            }
//            else
//            {

//                // Make sure the vertices for this quad will fit into the vertex buffer
//                int remainingSpace = this.maxVertices - this.currentOperation.EndVertex;
//                if (remainingSpace < 6)
//                    throw new ApplicationException("Too many quads. Try increasing maxQuads.");

//                // If the texture changed since the last call, begin a new rendering operation
//                if (this.currentOperation.Texture != d3dTexture)
//                {
//                    this.currentOperation = new RenderOperation(
//                      this.currentOperation.EndVertex, d3dTexture
//                    );
//                    this.operations.Add(this.currentOperation);
//                }

//                // Initialize the quad and append the vertices to our vertex list
//                generateQuadVertices(
//                  this.vertices, this.currentOperation.EndVertex,
//                  destRect, z, textureRect, colors, quadSplitMode
//                );
//                this.currentOperation.EndVertex += 6;

//                // Remember that the vertex buffer needs to be updated
//                this.vertexBufferUpToDate = false;
//            }

//        }

//        /// <summary>Perform final rendering for all quads that have been queued</summary>
//        /// <remarks>
//        ///   The contents of the rendering queue is retained and can be rendered again as required.
//        ///   If the contents is not required call <see cref="ClearRenderList"/>
//        /// </remarks>
//        public override void DoRender()
//        {
//            VertexFormats vertexFormat = CustomVertex.TransformedColoredTextured.Format;
//            int stride = CustomVertex.TransformedColoredTextured.StrideSize;

//            updateVertexBuffer();

//            // Select our vertex buffer into the device
//            this.d3dDevice.SetStreamSource(0, this.vertexBuffer, 0, stride);
//            this.d3dDevice.VertexFormat = vertexFormat;

//            int firstOperation = 1;
//            int lastOperation = 0;

//            int startVertex = 0;
//            int endVertex = this.operations[lastOperation].EndVertex;

//            do
//            {

//                // Pick as many operations as we can execute in this batch
//                if (endVertex == this.operations[lastOperation].EndVertex)
//                {

//                    firstOperation = lastOperation + 1;
//                    if (firstOperation >= this.operations.Count)
//                        break;

//                    startVertex = this.operations[firstOperation].StartVertex;

//                    // Until the end is reached
//                    while (lastOperation < this.operations.Count - 1)
//                    {
//                        if (this.operations[lastOperation].EndVertex - startVertex < VertexBatchSize)
//                            ++lastOperation;
//                        else
//                            break;
//                    }
//                }

//                // Determine the number of vertices we need to copy into the vertex buffer
//                endVertex = this.operations[lastOperation].EndVertex;
//                if ((endVertex - startVertex) > VertexBatchSize)
//                    endVertex = startVertex + (VertexBatchSize - VertexBatchSize % 3);

//                for (int index = firstOperation; index <= lastOperation; ++index)
//                {
//                    RenderOperation operation = this.operations[index];

//                    int drawStart = System.Math.Max(operation.StartVertex, startVertex);
//                    int drawCount = System.Math.Min(operation.EndVertex, endVertex) - drawStart;

//                    this.d3dDevice.SetTexture(0, operation.Texture);
//                    this.d3dDevice.DrawPrimitives(PrimitiveType.TriangleList, drawStart, drawCount / 3);
//                }

//                startVertex = endVertex;

//            } while (endVertex < this.currentOperation.EndVertex);

//        }

//        /// <summary>Clears all queued quads from the render queue</summary>
//        public override void ClearRenderList()
//        {
//            this.operations = new System.Collections.Generic.List<RenderOperation>();

//            // Ensure there always is a currentOperation available. This saves us from
//            // checking whether the current operation is null each time vertices are added.
//            this.currentOperation = new RenderOperation(0, null);
//            this.operations.Add(this.currentOperation);
//        }


//        /// <summary>Generates the vertices for a single quad in the internal vertex array</summary>
//        /// <param name="target">Array into which to write the quad</param>
//        /// <param name="startIndex">Start index within the internal vertex array</param>
//        /// <param name="destRect">Rectangle at which the quad will be drawn</param>
//        /// <param name="z">Desired z coordinate of the vertices</param>
//        /// <param name="textureRect">Texture coordinates of the texture region to use</param>
//        /// <param name="colors">Vertex colors at the 4 corners of the quad</param>
//        /// <param name="quadSplitMode">How to divide the quad into triangles</param>
//        private void generateQuadVertices(
//         VertexSprite[] target, int startIndex,
//          Rect destRect, float z, Rect textureRect, ColourRect colors, QuadSplitMode quadSplitMode
//        )
//        {
//            // Adjust the screen coordinates to hit the pixel centers. This is explained in detail in
//            // the DirectX documentation. If we wouldn't do this, the image might appear blurred
//            // (with texture filtering turned on) or a few pixels might become displaced because of
//            // rounding errors (with texture filtering turned off).
//            destRect.Top = (float)(int)destRect.Top - 0.5f;
//            destRect.Left = (float)(int)destRect.Left - 0.5f;
//            destRect.Right = (float)(int)destRect.Right - 0.5f;
//            destRect.Bottom = (float)(int)destRect.Bottom - 0.5f;

//            // We shamelessly ignore quadSplitMode. What effect should it have on the image anyway?

//            // First triangle
//            target[startIndex + 0].Position.X = destRect.Left;
//            target[startIndex + 0].Position.Y = destRect.Top;
//           // target[startIndex + 0].Position.Z = z;
//            //OOPS no color in vertexsprite target[startIndex + 0]. Color = colors.topLeft.ToARGB();
//            target[startIndex + 0].TextureCoordinate.X = textureRect.Left;
//            target[startIndex + 0].TextureCoordinate.Y = textureRect.Top;

//            target[startIndex + 1].Position.X = destRect.Left;
//            target[startIndex + 1].Position.Y = destRect.Bottom;
//            //target[startIndex + 1].Position.Z = z;
//            //target[startIndex + 1].Color = colors.bottomLeft.ToARGB();
//            target[startIndex + 1].TextureCoordinate.X = textureRect.Left;
//            target[startIndex + 1].TextureCoordinate.Y = textureRect.Bottom;

//            target[startIndex + 2].Position.X = destRect.Right;
//            target[startIndex + 2].Position.Y = destRect.Bottom;
//            //target[startIndex + 2].Z = z;
//            //target[startIndex + 2].Color = colors.bottomRight.ToARGB();
//            target[startIndex + 2].TextureCoordinate.X = textureRect.Right;
//            target[startIndex + 2].TextureCoordinate.Y = textureRect.Bottom;

//            // Second triangle
//            target[startIndex + 3] = target[startIndex + 0];

//            target[startIndex + 4] = target[startIndex + 2];

//            target[startIndex + 5].Position.X = destRect.Right;
//            target[startIndex + 5].Position.Y = destRect.Top;
//            //target[startIndex + 5].Z = z;
//            //target[startIndex + 5].Color = colors.topRight.ToARGB();
//            target[startIndex + 5].Position.X = textureRect.Right;
//            target[startIndex + 5].Position.Y = textureRect.Top;

//        }

//        /// <summary>Writes the current vertex list into the vertex buffer</summary>
//        /// <remarks>
//        ///   <para>
//        ///     Normally, updating the vertex buffer can be a costly operation because AGP
//        ///     memory needs to the locked and written into. Also, if you lock a vertex buffer,
//        ///     Direct3D would have to wait until the graphics card is finished drawing the
//        ///     current contents of the vertex buffer before granting access to modify it.
//        ///   </para>
//        ///   <para>
//        ///     Locking with the 'Discard' flag actually tells Direct3D to create a new
//        ///     vertex buffer that will be available for writing immediately. The old vertex
//        ///     buffer is deleted as soon as the graphics card has finished drawing its vertices.
//        ///   </para>
//        /// </remarks>
//        private void updateVertexBuffer()
//        {
//            if (this.vertexBufferUpToDate)
//                return;

//            // Put the vertex batch into the vertex buffer
//            this.vertexBuffer.SetData(
//              this.vertices, 0, Microsoft.DirectX.Direct3D.LockFlags.Discard
//            );

//            // The vertex buffer now is up-to-date once again
//            this.vertexBufferUpToDate = true;
//        }

//        /// <summary>Creates a 'null' Texture object</summary>
//        /// <returns>
//        ///   A newly created Texture object. The returned Texture object has no size
//        ///   or imagery associated with it, and is generally of little or no use.
//        /// </returns>
//        public override Texture CreateTexture()
//        {
//            D3DTexture newTexture = new D3DTexture(this);
//            this.textures.Add(newTexture);

//            return newTexture;
//        }

//        /// <summary>
//        ///   Create a Texture object with the given pixel dimensions as specified by
//        ///   <paramref name="size"/>
//        /// </summary>
//        /// <remarks>
//        ///   Textures are always created with a size that is a power of 2. If you specify a
//        ///   size that is not a power of two, the final	sizef will be rounded up. So if you
//        ///   specify a size of 1024, the texture will be (1024 x 1024), however, if you
//        ///   specify a size of 1025, the texture will be (2048 x 2048). You can check the
//        ///   ultimate size by querying the texture after creation.
//        /// </remarks>
//        /// <param name="size">
//        ///   Float value that specifies the size to use for the width and height when creating
//        ///   the new texture
//        /// </param>
//        /// <returns>
//        ///   A newly created Texture object. The initial contents of the texture memory are
//        ///   undefined / random
//        /// </returns>
//        public override Texture CreateTexture(float size)
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        /// <summary>
//        ///   Create a <see cref="Texture"/> object using the given image file name.
//        /// </summary>
//        /// <remarks>
//        ///   Textures are always created with a size that is a power of 2. If the file
//        ///   you specify is of a size that is not a power of two, the final size will be
//        ///   rounded up. Additionally, textures are always square, so the ultimate size
//        ///   is governed by the larger of the width and height of the specified file. You
//        ///   can check the ultimate sizes by querying the texture after creation.
//        /// </remarks>
//        /// <param name="filename">
//        ///   The path and filename of the image file to use when creating the texture
//        /// </param>
//        /// <param name="resourceGroup">
//        ///   Resource group identifier to be passed to the resource provider when loading
//        ///   the texture file
//        /// </param>
//        /// <returns>
//        ///   A newly created Texture object. The initial contents of the texture memory is
//        ///   the requested image file
//        /// </returns>
//        public override Texture CreateTexture(string filename, string resourceGroup)
//        {
//            D3DTexture newTexture = new D3DTexture(this);
//            this.textures.Add(newTexture);

//            newTexture.LoadFromFile(filename);

//            return newTexture;
//        }

//        /// <summary>Destroy all texture objects</summary>
//        public override void DestroyAllTextures()
//        {
//            foreach (D3DTexture texture in this.textures)
//                texture.Dispose();

//            this.textures.Clear();
//        }

//        /// <summary>Destroy the given Texture object</summary>
//        /// <param name="texture">Reference to the texture to be destroyed</param>
//        public override void DestroyTexture(Texture texture)
//        {
//            if (texture != null)
//                ((IDisposable)texture).Dispose();

//        }

//        /// <summary>Return the current width of the display in pixels</summary>
//        /// <value>Float value equal to the current width of the display in pixels.</value>
//        public override float Width
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        /// <summary>Return the current height of the display in pixels</summary>
//        /// <value>Float value equal to the current height of the display in pixels</value>
//        public override float Height
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        /// <summary>Return the size of the display in pixels</summary>
//        /// <value>A size object containing the dimensions of the current display</value>
//        public override SizeF Size
//        {
//            get
//            {
//                return new SizeF(game.ViewPort.Width, game.ViewPort.Height);
//            }
//        }

//        /// <summary>Return a <see cref="Rect"/> describing the screen</summary>
//        /// <value>
//        ///   A Rect object that describes the screen area. Typically, the top-left
//        ///   values are always 0, and the size of the area described is equal to
//        ///   the screen resolution.
//        /// </value>
//        public override Rect Rect
//        {
//            get
//            {
//                return new Rect(
//                  0, 0,
//                  game.ViewPort.Width, game.ViewPort.Height
//                );
//            }
//        }

//        /// <summary>Return the maximum texture size available</summary>
//        /// <value>
//        ///   Size of the maximum supported texture in pixels (textures are always
//        ///   assumed to be square)
//        /// </value>
//        public override int MaxTextureSize
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        /// <summary>Return the horizontal display resolution dpi</summary>
//        /// <value>Horizontal resolution of the display in dpi</value>
//        public override int HorizontalScreenDPI
//        {
//            get { return 96; }
//        }

//        /// <summary>Return the vertical display resolution dpi</summary>
//        /// <value>Vertical resolution of the display in dpi</value>
//        public override int VerticalScreenDPI
//        {
//            get { return 96; }
//        }

//        /// <summary>The Direct3D device used by the renderer</summary>
//        public Device D3DDevice
//        {
//            get { return d3dDevice; }
//        }

//        /// <summary>Queueable rendering operation</summary>
//        private class RenderOperation
//        {

//            /// <summary>Constructs a textured RenderOperation</summary>
//            /// <param name="startVertex">Starting vertex of the RenderOperation</param>
//            /// <param name="texture">The texture to be selected when rendering</param>
//            public RenderOperation(int startVertex, Texture2D texture)
//            {
//                this.StartVertex = startVertex;
//                this.EndVertex = startVertex;
//                this.Texture = texture;
//            }

//            /// <summary>First vertex to draw</summary>
//            public int StartVertex;
//            /// <summary>Vertex after the last vertex to draw</summary>
//            public int EndVertex;
//            /// <summary>Texture to use. Can be null</summary>
//            public Texture2D Texture;

//        }

//        /// <summary>The Direct3D device we're using for rendering</summary>
//        private Device d3dDevice;
//        /// <summary>Primary vertex buffer used for caching primitives</summary>
//        private VertexBuffer<VertexSprite> vertexBuffer;
//        /// <summary>List of all textures this renderer has created</summary>
//        private List<D3DTexture> textures;

//        /// <summary>All vertex batches enqueued for rendering so far</summary>
//        private System.Collections.Generic.List<RenderOperation> operations;
//        /// <summary>Cached reference to the current RenderOperation</summary>
//        private RenderOperation currentOperation;
//        /// <summary>Array used to cache geometry for the vertex buffer</summary>
//        private VertexSprite[] vertices;
//        /// <summary>Maximum number of vertices allowed in the vertex array</summary>
//        private int maxVertices;
//        /// <summary>Whether the vertex buffer is up to date with the vertices array</summary>
//        private bool vertexBufferUpToDate;
//        private Game game;

//        private HLSLSprites _effect;
//        private SamplerState _spriteSampler;
//        private int _rasterStateId, _blendStateId, _depthStateId;

//    }

//} // namespace CeGui.Renderers.Direct3D9
