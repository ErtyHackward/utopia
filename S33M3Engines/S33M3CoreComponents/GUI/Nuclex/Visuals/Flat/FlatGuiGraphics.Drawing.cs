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

using SharpDX;
using Rectangle = SharpDX.Rectangle;
using S33M3CoreComponents.Sprites2D;
using System;
using SharpDX.Direct3D11;
using S33M3Resources.Structs;


namespace S33M3CoreComponents.GUI.Nuclex.Visuals.Flat
{

    partial class FlatGuiGraphics
    {
        private ByteColor _defaultColor = Color.White;

        /// <summary>Needs to be called before the GUI drawing process begins</summary>
        public void BeginDrawing()
        {
            spriteRenderer.Begin(true, _d3dEngine.ImmediateContext);

            DrawCalls = 0;
            DrawItems = 0;
        }

        /// <summary>Needs to be called when the GUI drawing process has ended</summary>
        public void FlushPendingData()
        {
            // flushing all pending draw calls to the device
            spriteRenderer.End(_d3dEngine.ImmediateContext);

            DrawCalls += spriteRenderer.DrawCalls;
            DrawItems += spriteRenderer.SpritesDraw;
        }

        public void ContinueDrawing()
        {
            spriteRenderer.Restart();
        }

        public void SetScissorMode(bool isScissorMode)
        {
            spriteRenderer.IsScissorMode = isScissorMode;
            ContinueDrawing();
        }

        /// <summary>Needs to be called when the GUI drawing process has ended</summary>
        public void EndDrawing()
        {
            // flushing all pending draw calls to the device
            spriteRenderer.End(_d3dEngine.ImmediateContext);

            // update stati
            DrawCalls += spriteRenderer.DrawCalls;
            DrawItems += spriteRenderer.SpritesDraw;
        }

        /// <summary>Sets the clipping region for any future drawing commands</summary>
        /// <param name="clipRegion">Clipping region that will be set</param>
        /// <returns>
        ///   An object that will unset the clipping region upon its destruction.
        /// </returns>
        /// <remarks>
        ///   Clipping regions can be stacked, though this is not very typical for
        ///   a game GUI and also not recommended practice due to performance constraints.
        ///   Unless clipping is implemented in software, setting up a clip region
        ///   on current hardware requires the drawing queue to be flushed, negatively
        ///   impacting rendering performance (in technical terms, a clipping region
        ///   change likely causes 2 more DrawPrimitive() calls from the painter).
        /// </remarks>
        public IDisposable SetClipRegion(ref RectangleF clipRegion)
        {
            // Cache the integer values of the clipping region's boundaries
            int clipX = (int)clipRegion.Left;
            int clipY = (int)clipRegion.Top;
            int clipRight = clipX + (int)clipRegion.Width;
            int clipBottom = clipY + (int)clipRegion.Height;

            // Calculate the viewport's right and bottom coordinates
            ViewportF viewport = _d3dEngine.ViewPort;
            int viewportRight = (int)(viewport.X + viewport.Width);
            int viewportBottom = (int)(viewport.Y + viewport.Height);

            // Extract the part of the clipping region that lies within the viewport
            Rectangle scissorRegion = new Rectangle(
                                                Math.Max(clipX, (int)viewport.X),
                                                Math.Max(clipY, (int)viewport.Y),
                                                (Math.Min(clipRight, viewportRight) - clipX) + (clipX - Math.Max(clipX, (int)viewport.X)),
                                                (Math.Min(clipBottom, viewportBottom) - clipY) + (clipY - Math.Max(clipY, (int)viewport.Y))
                                            );
            //scissorRegion.Width += clipX - scissorRegion.X;
            //scissorRegion.Height += clipY - scissorRegion.Y;

            // If the clipping region was entirely outside of the viewport (meaning
            // the calculated width and/or height are negative), use an empty scissor
            // rectangle instead because XNA doesn't like scissor rectangles with
            // negative coordinates.
            if ((scissorRegion.Width <= 0) || (scissorRegion.Height <= 0))
            {
                scissorRegion = Rectangle.Empty;
            }

            // All done, take over the new scissor rectangle
            this.scissorManager.Assign(ref scissorRegion);

            return this.scissorManager;

        }

        /// <summary>Draws a GUI element onto the drawing buffer</summary>
        /// <param name="frameName">Class of the element to draw</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="color">color</param>
        /// <remarks>
        ///   <para>
        ///     GUI elements are the basic building blocks of a GUI: 
        ///   </para>
        /// </remarks>
        public void DrawElement(string frameName, ref RectangleF bounds, ref ByteColor color, int groupId = 0)
        {
            var frame = lookupFrame(frameName);

            // Draw all the regions defined for the element. Each region is a small bitmap
            // that needs to be blit somewhere into the element to form the element's
            // visual representation step by step.
            foreach (var t in frame.Regions)
            {
                var region = t;
                Rectangle destinationRegion = calculateDestinationRectangle(ref bounds, ref region.DestinationRegion);
                Rectangle sourceRegion = region.SourceRegion;

                if (region.Tiled)
                    spriteRenderer.DrawWithWrapping(region.Texture, ref destinationRegion, ref sourceRegion, ref color, 0, true, groupId);
                else
                    spriteRenderer.Draw(region.Texture, ref destinationRegion, ref sourceRegion, ref color, 0, true, groupId);
            }
        }

        public void DrawElement(string frameName, ref RectangleF bounds, int groupId = 0)
        {
            ByteColor color = Color.White;
            DrawElement(frameName, ref bounds, ref color, groupId);
        }

        //the 2 DrawCustomTexture! 
        public void DrawCustomTexture(SpriteTexture customTex, ref RectangleF bounds, int textureArrayIndex = 0, int groupId = 0, ByteColor? color = null)
        {
            var offset = new UniRectangle(0, 0, bounds.Width, bounds.Height);
            var destinationRegion = calculateDestinationRectangle(
              ref bounds, ref offset
            );

            if (!color.HasValue)
                color = _defaultColor;

            ByteColor drawColor = color.Value;

            Rectangle srcRegion = new Rectangle(0, 0, customTex.Width, customTex.Height);
            spriteRenderer.Draw(customTex, ref destinationRegion, ref srcRegion, ref drawColor, textureArrayIndex, true, groupId);
        }

        public void DrawCustomTexture(SpriteTexture customTex, ref Rectangle textureSourceRect, ref RectangleF bounds, float rotation, SamplerState sampler, int groupId = 0, ByteColor? color = null)
        {
            var offset = new UniRectangle(0, 0, bounds.Width, bounds.Height);
            var destinationRegion = calculateDestinationRectangle(
              ref bounds, ref offset
            );

            ByteColor col;
            if (!color.HasValue)
                col = _defaultColor;
            else col = (ByteColor)color;

            spriteRenderer.Draw(customTex, ref destinationRegion, ref textureSourceRect, ref col, rotation, sampler, drawGroupId: groupId);
        }


        public void DrawCustomTexture(SpriteTexture customTex, ref Rectangle textureSourceRect, ref RectangleF bounds, int groupId = 0)
        {
            var offset = new UniRectangle(0, 0, bounds.Width, bounds.Height);
            var destinationRegion = calculateDestinationRectangle(
              ref bounds, ref offset
            );

            spriteRenderer.Draw(customTex, ref destinationRegion, ref textureSourceRect, ref _defaultColor, groupId);
        }

        public void DrawCustomTextureTiled(SpriteTexture customTex, ref RectangleF bounds, int textureArrayIndex = 0, int groupId = 0)
        {
            Vector2 position;
            position.X = bounds.X;
            position.Y = bounds.Y;

            Vector2 size;
            size.X = bounds.Width;
            size.Y = bounds.Height;

            spriteRenderer.DrawWithWrapping(customTex, ref position, ref size, ref _defaultColor, textureArrayIndex, groupId);
        }


        /// <summary>Draws text into the drawing buffer for the specified element</summary>
        /// <param name="frameName">Class of the element for which to draw text</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text that will be drawn</param>
        public void DrawString(
            string frameName, 
            int frameFontId, 
            ref RectangleF bounds, 
            string text, 
            ref ByteColor color, 
            bool withMaxWidth, 
            int carretPosition = -1,  
            int groupId = 0)
        {
            if (string.IsNullOrWhiteSpace(text) && carretPosition == -1)
                return;

            var frame = lookupFrame(frameName);
            var position = positionText(ref frame.Texts[frameFontId], ref bounds, text);
            spriteRenderer.DrawText(frame.Texts[frameFontId].Font, text, ref position, ref color, withMaxWidth ? (int)bounds.Width : -1, carretPosition, SpriteRenderer.TextFontPosition.RelativeToFontUp, groupId);
        }

        /// <summary>Draws text into the drawing buffer for the specified element</summary>
        /// <param name="frameName">Class of the element for which to draw text</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text that will be drawn</param>
        public void DrawString(
            string frameName, 
            int frameFontId, 
            ref RectangleF bounds, 
            string text, 
            bool withMaxWidth, 
            int carretPosition = -1, 
            int groupId = 0)
        {
            if (string.IsNullOrWhiteSpace(text) && carretPosition == -1)
                return;

            var frame = lookupFrame(frameName);
            Vector2 position = positionText(ref frame.Texts[frameFontId], ref bounds, text);
            ByteColor color = frame.Texts[frameFontId].Color;
            spriteRenderer.DrawText(frame.Texts[frameFontId].Font, text, ref position, ref color, withMaxWidth ? (int)bounds.Width : -1, carretPosition, SpriteRenderer.TextFontPosition.RelativeToFontUp, groupId);
        }


        public void DrawString(
            SpriteFont font, 
            ref RectangleF bounds, 
            string text, 
            ref ByteColor color, 
            bool withMaxWidth, 
            int carretPosition = -1,
            Frame.HorizontalTextAlignment horizontalPlacement = Frame.HorizontalTextAlignment.Left,
            Frame.VerticalTextAlignment verticalPlacement = Frame.VerticalTextAlignment.Top, 
            int groupId = 0)
        {
            if (string.IsNullOrWhiteSpace(text) && carretPosition == -1)
                return;

            var position = positionText(ref bounds, text, font, horizontalPlacement, verticalPlacement);
            spriteRenderer.DrawText(font, text, ref position, ref color, withMaxWidth ? (int)bounds.Width : -1, carretPosition, SpriteRenderer.TextFontPosition.RelativeToFontUp, groupId);
        }

        /// <summary>Measures the extents of a string in the frame's area</summary>
        /// <param name="frameName">Class of the element whose text will be measured</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text that will be measured</param>
        /// <returns>
        ///   The size and extents of the specified string within the frame
        /// </returns>
        public RectangleF MeasureString(string frameName, ref RectangleF bounds, string text)
        {
            var frame = lookupFrame(frameName);
            if (frame.Texts.Length <= 0) return RectangleF.Empty;

            return MeasureString(frame.Texts[0].Font, ref bounds, text);
        }

        public RectangleF MeasureString(SpriteFont spriteFont, ref RectangleF bounds, string text)
        {
            var size = spriteFont.MeasureString(text);

            return new RectangleF(0.0f, 0.0f, size.X, size.Y);
        }

        /// <summary>
        ///   Locates the closest gap between two letters to the provided position
        /// </summary>
        /// <param name="frameName">Class of the element in which to find the gap</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text in which the closest gap will be found</param>
        /// <param name="position">Position of which to determien the closest gap</param>
        /// <returns>The index of the gap the position is closest to</returns>
        public int GetClosestOpening(string frameName, ref RectangleF bounds, string text, ref Vector2 position)
        {
            var frame = lookupFrame(frameName);

            // exTODO: Find the closest gap across multiple text anchors
            //   Frames can repeat their text in several places. Though this is probably
            //   not used very often (if at all), it should work here consistently.

            int closestGap = -1;

            for (int index = 0; index < frame.Texts.Length; ++index)
            {
                var textPosition = positionText(ref frame.Texts[index],ref bounds, text);
                position.X -= textPosition.X;
                position.Y -= textPosition.Y;

                //float openingX = position.X;
                var openingIndex = openingLocator.FindClosestOpening(frame.Texts[index].Font, text, position.X + CaretWidth);

                closestGap = openingIndex;
            }

            return closestGap;
        }

    }

}
