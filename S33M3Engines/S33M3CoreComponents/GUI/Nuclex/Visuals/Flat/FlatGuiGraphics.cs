#region CPL License
/*
Nuclex Framework
Copyright (C) 2002-2010 Nuclex Development Labs

This library is free software; you can redistribute it and/or
modify it under the terms of the IBM Common Public License as
published by the IBM Corporation; either version 1.0 of the
License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
IBM Common Public License for more details.

You should have received a copy of the IBM Common Public
License along with this library
*/
#endregion

using System;
using SharpDX;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Xml;
using System.Xml.Schema;

using Texture2D = SharpDX.Direct3D11.Texture2D;
using Rectangle = SharpDX.Rectangle;
using RasterizerState = SharpDX.Direct3D11.RasterizerState;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using S33M3DXEngine;
using S33M3CoreComponents.Sprites2D;
using S33M3CoreComponents.Cameras.Interfaces;


namespace S33M3CoreComponents.GUI.Nuclex.Visuals.Flat
{

    /// <summary>Graphics interface for the traditional flat GUI visualizer</summary>
    /// <remarks>
    ///   This class is analog to System.Drawing.Graphics, but contains specialized
    ///   methods that allow the FlatControlRenderers to draw controls from
    ///   high-level elements which are controlled by loadable XML themes.
    /// </remarks>
    public partial class FlatGuiGraphics : IFlatGuiGraphics, IDisposable
    {

        /// <summary>Width of the caret used for text input</summary>
        const float CaretWidth = 1.0f;

        #region struct Frame

        /// <summary>Frame that can be drawn by the GUI painter</summary>
        public class Frame
        {

            /// <summary>Modes in which text can be horizontally aligned</summary>
            public enum HorizontalTextAlignment
            {
                /// <summary>The text's base offset is placed at the left of the frame</summary>
                /// <remarks>
                ///   The base offset is normally identical to the text's leftmost pixel.
                ///   However, a glyph may have some eccentrics like an arc that extends to
                ///   the left over the letter's actual starting position.
                /// </remarks>
                Left,
                /// <summary>
                ///   The text's ending offset is placed at the right of the frame
                /// </summary>
                /// <remarks>
                ///   The ending offset is normally identical to the text's rightmost pixel.
                ///   However, a glyph may have some eccentrics like an arc that extends to
                ///   the right over the last letter's actual ending position.
                /// </remarks>
                Right,
                /// <summary>The text is centered horizontally in the frame</summary>
                Center

            }

            /// <summary>Modes in which text can be vertically aligned</summary>
            public enum VerticalTextAlignment
            {
                /// <summary>The text's baseline is placed at the top of the frame</summary>
                Top,
                /// <summary>The text's baseline is placed at the bottom of the frame</summary>
                Bottom,
                /// <summary>The text's baseline is centered vertically in the frame</summary>
                Center
            }

            /// <summary>Defines a picture region drawn into a frame</summary>
            public struct Region
            {
                /// <summary>Identification string for the region</summary>
                /// <remarks>
                ///   Used to associate regions with specific behavior
                /// </remarks>
                public string Id;
                /// <summary>Texture the picture region is taken from</summary>
                public SpriteTexture Texture;
                /// <summary>Area within the texture containing the picture region</summary>
                public Rectangle SourceRegion;
                /// <summary>Location in the frame where the picture region will be drawn</summary>
                public UniRectangle DestinationRegion;
                /// <summary>
                /// Indicates if the texture should be tiled or stretched
                /// </summary>
                public bool Tiled;
            }

            /// <summary>Describes where within the frame text should be drawn</summary>
            public struct Text
            {
                /// <summary>Font to use for drawing the text</summary>
                public SpriteFont Font;
                /// <summary>Offset of the text relative to its specified placement</summary>
                public System.Drawing.Point Offset;
                /// <summary>Horizontal placement of the text within the frame</summary>
                public HorizontalTextAlignment HorizontalPlacement;
                /// <summary>Vertical placement of the text within the frame</summary>
                public VerticalTextAlignment VerticalPlacement;
                /// <summary>Color the text will have</summary>
                public Color4 Color;
            }

            /// <summary>Initializes a new frame</summary>
            /// <param name="regions">Regions needed to be drawn to render the frame</param>
            /// <param name="texts">Location in the frame where text can be drawn</param>
            public Frame(Region[] regions, Text[] texts)
            {
                this.Regions = regions;
                this.Texts = texts;
            }

            /// <summary>Regions that need to be drawn to render the frame</summary>
            public Region[] Regions;

            /// <summary>Locations where text can be drawn into the frame</summary>
            public Text[] Texts;

        }

        #endregion // struct Frame

        #region class ScissorKeeper

        /// <summary>Manages the scissor rectangle for the GUI graphics interface</summary>
        private class ScissorKeeper : IDisposable
        {
            private D3DEngine _d3dEngine;

            /// <summary>Initializes a new scissor manager</summary>
            /// <param name="flatGuiGraphics">
            ///   GUI graphics interface the scissor rectangle will be managed for
            /// </param>
            public ScissorKeeper(D3DEngine d3dEngine, FlatGuiGraphics flatGuiGraphics)
            {
                _d3dEngine = d3dEngine;
                this.flatGuiGraphics = flatGuiGraphics;
            }

            /// <summary>Assigns the scissor rectangle to the graphics device</summary>
            /// <param name="clipRegion">Scissor rectangle that will be assigned</param>
            public void Assign(ref Rectangle clipRegion)
            {
                this.flatGuiGraphics.FlushPendingData();

                try
                {
                    this.oldScissorRectangles = _d3dEngine.ScissorRectangles;
                    _d3dEngine.ScissorRectangles = new SharpDX.Rectangle[] { new SharpDX.Rectangle(clipRegion.Left, clipRegion.Top, clipRegion.Right, clipRegion.Bottom) };
                }
                finally
                {
                    this.flatGuiGraphics.SetScissorMode(true);
                    //I'm now in Scissor Mode => Set the spriteRendering with new state
                }
            }

            /// <summary>Releases the currently assigned scissor rectangle again</summary>
            public void Dispose()
            {
                this.flatGuiGraphics.FlushPendingData();
                _d3dEngine.ScissorRectangles = this.oldScissorRectangles;
                this.flatGuiGraphics.SetScissorMode(false);
            }

            /// <summary>
            ///   GUI graphics interface for which the scissor rectangle is managed
            /// </summary>
            private FlatGuiGraphics flatGuiGraphics;
            /// <summary>
            ///   Scissor rectangle that was previously assigned to the graphics device
            /// </summary>
            private SharpDX.Rectangle[] oldScissorRectangles;

        }

        #endregion // class ScissorKeeper

        private D3DEngine _d3dEngine;
        private string _resourceDirectory;

        public D3DEngine Engine
        {
            get { return _d3dEngine; }
        }

        /// <summary>Initializes a new gui painter</summary>
        /// <param name="contentManager">
        ///   Content manager containing the resources for the GUI. The instance takes
        ///   ownership of the content manager and will dispose it.
        /// </param>
        /// <param name="skinStream">
        ///   Stream from which the skin description will be read
        /// </param>
        public FlatGuiGraphics(D3DEngine d3dEngine, Stream skinStream, string resourceDirectory)
        {
            _d3dEngine = d3dEngine;
            _resourceDirectory = resourceDirectory;

            this.spriteRenderer = new SpriteRenderer(_d3dEngine, true);

            this.openingLocator = new OpeningLocator();
            this.stringBuilder = new StringBuilder(64);
            this.scissorManager = new ScissorKeeper(_d3dEngine, this);

            this.fonts = new Dictionary<string, SpriteFont>();
            this.bitmaps = new Dictionary<string, SpriteTexture>();
            this.frames = new Dictionary<string, Frame>();

            loadSkin(skinStream);
        }

        /// <summary>Immediately releases all resources owned by the instance</summary>
        public void Dispose()
        {
            if (this.spriteRenderer != null)
            {
                this.spriteRenderer.Dispose();
            }

            foreach (var kvp in fonts) kvp.Value.Dispose();
            foreach (var kvp in bitmaps) kvp.Value.Dispose();

        }

        private Vector2 positionText(ref RectangleF bounds, 
                                     string text,
                                     SpriteFont Font,
                                     FlatGuiGraphics.Frame.HorizontalTextAlignment HorizontalPlacement,
                                     FlatGuiGraphics.Frame.VerticalTextAlignment VerticalPlacement)
        {
            Vector2 textSize = Font.MeasureString(text);
            float x, y;

            switch (HorizontalPlacement)
            {
                case Frame.HorizontalTextAlignment.Left:
                    {
                        x = bounds.Left;
                        break;
                    }
                case Frame.HorizontalTextAlignment.Right:
                    {
                        x = bounds.Right - textSize.X;
                        break;
                    }
                case Frame.HorizontalTextAlignment.Center:
                default:
                    {
                        x = (bounds.Width - textSize.X) / 2.0f + bounds.Left;
                        break;
                    }
            }

            switch (VerticalPlacement)
            {
                case Frame.VerticalTextAlignment.Top:
                    {
                        y = bounds.Top;
                        break;
                    }
                case Frame.VerticalTextAlignment.Bottom:
                    {
                        y = bounds.Bottom - Font.LineSpacing;
                        break;
                    }
                case Frame.VerticalTextAlignment.Center:
                default:
                    {
                        y = (bounds.Height - Font.CharHeight) / 2.0f + bounds.Top;
                        break;
                    }
            }

            return new Vector2(floor(x), floor(y));
        }

        /// <summary>
        ///   Positions a string within a frame according to the positioning instructions
        ///   stored in the provided text anchor.
        /// </summary>
        /// <param name="anchor">Text anchor the string will be positioned for</param>
        /// <param name="bounds">Boundaries of the control the string is rendered in</param>
        /// <param name="text">String that will be positioned</param>
        /// <returns>The position of the string within the control</returns>
        private Vector2 positionText(ref Frame.Text anchor, ref RectangleF bounds, string text)
        {
            Vector2 textSize = anchor.Font.MeasureString(text);
            float x, y;

            switch (anchor.HorizontalPlacement)
            {
                case Frame.HorizontalTextAlignment.Left:
                    {
                        x = bounds.Left;
                        break;
                    }
                case Frame.HorizontalTextAlignment.Right:
                    {
                        x = bounds.Right - textSize.X;
                        break;
                    }
                case Frame.HorizontalTextAlignment.Center:
                default:
                    {
                        x = (bounds.Width - textSize.X) / 2.0f + bounds.Left;
                        break;
                    }
            }

            switch (anchor.VerticalPlacement)
            {
                case Frame.VerticalTextAlignment.Top:
                    {
                        y = bounds.Top;
                        break;
                    }
                case Frame.VerticalTextAlignment.Bottom:
                    {
                        y = bounds.Bottom - anchor.Font.LineSpacing;
                        break;
                    }
                case Frame.VerticalTextAlignment.Center:
                default:
                    {
                        y = (bounds.Height - anchor.Font.CharHeight) / 2.0f + bounds.Top;
                        break;
                    }
            }

            return new Vector2(floor(x + anchor.Offset.X), floor(y + anchor.Offset.Y));
        }

        /// <summary>
        ///   Calculates the absolute pixel position of a rectangle in unified coordinates
        /// </summary>
        /// <param name="bounds">Bounds of the drawing area in pixels</param>
        /// <param name="destination">Destination rectangle in unified coordinates</param>
        /// <returns>
        ///   The destination rectangle converted to absolute pixel coordinates
        /// </returns>
        private static Rectangle calculateDestinationRectangle(ref RectangleF bounds, ref UniRectangle destination)
        {
            int x = (int)(bounds.Left + destination.Location.X.Offset);
            x += (int)(bounds.Width * destination.Location.X.Fraction);

            int y = (int)(bounds.Top + destination.Location.Y.Offset);
            y += (int)(bounds.Height * destination.Location.Y.Fraction);

            int width = (int)(destination.Size.X.Offset);
            width += (int)(bounds.Width * destination.Size.X.Fraction);

            int height = (int)(destination.Size.Y.Offset);
            height += (int)(bounds.Height * destination.Size.Y.Fraction);

            return new Rectangle(x, y, width, height);
        }

        /// <summary>Looks up the frame with the specified name</summary>
        /// <param name="frameName">Frame that will be looked up</param>
        /// <returns>The frame with the specified name</returns>
        private Frame lookupFrame(string frameName)
        {

            // Make sure the renderer specified a valid frame name. If someone modifies
            // the skin or uses a skin which does not support all required controls,
            // this will provide the user with a clear error message.
            Frame frame;
            if (!this.frames.TryGetValue(frameName, out frame))
            {
                throw new ArgumentException(
                  "Unknown frame type: '" + frameName + "'", "frameName"
                );
            }

            return frame;

        }

        /// <summary>Removes the fractional part from the floating point value</summary>
        /// <param name="value">Value whose fractional part will be removed</param>
        /// <returns>The floating point value without its fractional part</returns>
        private static float floor(float value)
        {
            return (float)Math.Floor((double)value);
        }

        /// <summary>String builder used for various purposes in this class</summary>
        private StringBuilder stringBuilder;
        /// <summary>Locates openings between letters in strings</summary>
        private OpeningLocator openingLocator;
        /// <summary>Batches GUI elements for faster drawing</summary>
        private SpriteRenderer spriteRenderer;
        /// <summary>Manages the scissor rectangle and its assignment time</summary>
        private ScissorKeeper scissorManager;
        /// <summary>Font styles known to the GUI</summary>
        private Dictionary<string, SpriteFont> fonts;
        /// <summary>Bitmaps containing resources for the GUI</summary>
        private Dictionary<string, SpriteTexture> bitmaps;
        /// <summary>Types of frames the painter can draw</summary>
        private Dictionary<string, Frame> frames;

        ///// <summary>Rasterizer state used for drawing the GUI</summary>
        //private RasterizerState rasterizerState;

        public int DrawCalls { get; private set; }

        public int DrawItems { get; private set; }

    }
}
