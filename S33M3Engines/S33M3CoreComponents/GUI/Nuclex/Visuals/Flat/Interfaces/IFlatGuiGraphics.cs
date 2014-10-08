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
using S33M3DXEngine;
using SharpDX;
using System.Collections.Generic;
using SharpDX.Direct3D11;
using Rectangle = SharpDX.Rectangle;
using S33M3CoreComponents.Sprites2D;
using S33M3Resources.Structs;

namespace S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces
{

    /// <summary>Provides drawing methods for GUI controls</summary>
    /// <remarks>
    ///   Analogous to System.Drawing.Graphics, but provides specialized methods for
    ///   drawing a GUI with a dynamic, switchable theme.
    /// </remarks>
    public interface IFlatGuiGraphics
    {

        D3DEngine Engine { get; }

        //<summary>Sets the clipping region for any future drawing commands</summary>
        //<param name="clipRegion">Clipping region that will be set</param>
        //<returns>
        //  An object that will unset the clipping region upon its destruction.
        //</returns>
        //<remarks>
        //  Clipping regions can be stacked, though this is not very typical for
        //  a game GUI and also not recommended practice due to performance constraints.
        //  Unless clipping is implemented in software, setting up a clip region
        //  on current hardware requires the drawing queue to be flushed, negatively
        //  impacting rendering performance (in technical terms, a clipping region
        //  change likely causes 2 more DrawPrimitive() calls from the painter).
        //</remarks>
        IDisposable SetClipRegion(ref RectangleF clipRegion);

        /// <summary>Draws a GUI element onto the drawing buffer</summary>
        /// <param name="frameName">Class of the element to draw</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <remarks>
        ///   <para>
        ///     GUI elements are the basic building blocks of a GUI: 
        ///   </para>
        /// </remarks>
        void DrawElement(string frameName, ref RectangleF bounds, int groupId = 0);

        void DrawElement(string frameName, ref RectangleF controlBounds, ref ByteColor color, int groupId = 0);

        void DrawCustomTexture(SpriteTexture custom, ref RectangleF bounds, int textureArrayIndex = 0, int groupId = 0, ByteColor? color = null);

        void DrawCustomTexture(SpriteTexture customTex, ref Rectangle textureSourceRect, ref RectangleF bounds, float rotation, SamplerState sampler, int groupId = 0, ByteColor? color = null);

        void DrawCustomTexture(SpriteTexture customTex, ref Rectangle textureSourceRect, ref RectangleF bounds, int groupId = 0);

        void DrawCustomTextureTiled(SpriteTexture customTex, ref RectangleF bounds, int textureArrayIndex = 0, int groupId = 0);

        /// <summary>Draws text into the drawing buffer for the specified element</summary>
        /// <param name="frameName">Class of the element for which to draw text</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text that will be drawn</param>
        void DrawString(
            string frameName, 
            int frameFontId, 
            ref RectangleF bounds, 
            string text, 
            ref ByteColor color, 
            bool withMaxWidth, 
            int carretPosition = -1,
            int groupId = 0);

        /// <summary>
        /// Draws text into the drawing buffer using font specified
        /// </summary>
        /// <param name="font"></param>
        /// <param name="bounds"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="withMaxWidth"></param>
        /// <param name="carretPosition"></param>
        void DrawString(
            SpriteFont font, 
            ref RectangleF bounds, 
            string text, 
            ref ByteColor color, 
            bool withMaxWidth, 
            int carretPosition = -1,
            FlatGuiGraphics.Frame.HorizontalTextAlignment horizontalPlacement = FlatGuiGraphics.Frame.HorizontalTextAlignment.Left,
            FlatGuiGraphics.Frame.VerticalTextAlignment verticalPlacement = FlatGuiGraphics.Frame.VerticalTextAlignment.Top, 
            int groupId = 0);

        /// <summary>Draws text into the drawing buffer for the specified element</summary>
        /// <param name="frameName">Class of the element for which to draw text</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text that will be drawn</param>
        void DrawString(
            string frameName, 
            int frameFontId, 
            ref RectangleF bounds, 
            string text, 
            bool withMaxWidth, 
            int carretPosition = -1, 
            int groupId = 0
            );

        /// <summary>Measures the extents of a string in the frame's area</summary>
        /// <param name="frameName">Class of the element whose text will be measured</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text that will be measured</param>
        /// <returns>
        ///   The size and extents of the specified string within the frame
        /// </returns>
        RectangleF MeasureString(string frameName, ref RectangleF bounds, string text);
        RectangleF MeasureString(SpriteFont spriteFont, ref RectangleF bounds, string text);

        /// <summary>
        ///   Locates the closest gap between two letters to the provided position
        /// </summary>
        /// <param name="frameName">Class of the element in which to find the gap</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text in which the closest gap will be found</param>
        /// <param name="position">Position of which to determien the closest gap</param>
        /// <returns>The index of the gap the position is closest to</returns>
        int GetClosestOpening(string frameName, ref RectangleF bounds, string text, ref Vector2 position);

    }
}
