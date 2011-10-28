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
using Rectangle = System.Drawing.Rectangle;
using Utopia.Shared.Structs;
using S33M3Engines.Shared.Sprites;


namespace Nuclex.UserInterface.Visuals.Flat {

    partial class FlatGuiGraphics
    {


        /// <summary>Needs to be called before the GUI drawing process begins</summary>
        public void BeginDrawing()
        {
            //No sense to define a full screen Scissor ...
            //_game.D3dEngine.ScissorRectangle = new System.Drawing.Rectangle(0, 0, (int)_game.ViewPort.Width, (int)_game.ViewPort.Height);
            spriteRenderer.Begin();
        }

        /// <summary>Needs to be called when the GUI drawing process has ended</summary>
        public void EndDrawing()
        {
            // flushing all pending draw calls to the device
            spriteRenderer.End();

            // update stati
            DrawCalls = spriteRenderer.DrawCalls;
            DrawItems = spriteRenderer.DrawItems;
        }

        ///// <summary>Sets the clipping region for any future drawing commands</summary>
        ///// <param name="clipRegion">Clipping region that will be set</param>
        ///// <returns>
        /////   An object that will unset the clipping region upon its destruction.
        ///// </returns>
        ///// <remarks>
        /////   Clipping regions can be stacked, though this is not very typical for
        /////   a game GUI and also not recommended practice due to performance constraints.
        /////   Unless clipping is implemented in software, setting up a clip region
        /////   on current hardware requires the drawing queue to be flushed, negatively
        /////   impacting rendering performance (in technical terms, a clipping region
        /////   change likely causes 2 more DrawPrimitive() calls from the painter).
        ///// </remarks>
        //public IDisposable SetClipRegion(RectangleF clipRegion)
        //{

        //    // Cache the integer values of the clipping region's boundaries
        //    int clipX = (int)clipRegion.Left;
        //    int clipY = (int)clipRegion.Top;
        //    int clipRight = clipX + (int)clipRegion.Width;
        //    int clipBottom = clipY + (int)clipRegion.Height;

        //    // Calculate the viewport's right and bottom coordinates
        //    Viewport viewport = _game.ViewPort;
        //    int viewportRight = (int)(viewport.TopLeftX + viewport.Width);
        //    int viewportBottom = (int)(viewport.TopLeftY + viewport.Height);

        //    // Extract the part of the clipping region that lies within the viewport
        //    Rectangle scissorRegion = new Rectangle(
        //                                        Math.Max(clipX, (int)viewport.TopLeftX),
        //                                        Math.Max(clipY, (int)viewport.TopLeftY),
        //                                        Math.Min(clipRight, viewportRight) - clipX,
        //                                        Math.Min(clipBottom, viewportBottom) - clipY
        //                                    );
        //    scissorRegion.Width += clipX - scissorRegion.X;
        //    scissorRegion.Height += clipY - scissorRegion.Y;

        //    // If the clipping region was entirely outside of the viewport (meaning
        //    // the calculated width and/or height are negative), use an empty scissor
        //    // rectangle instead because XNA doesn't like scissor rectangles with
        //    // negative coordinates.
        //    if ((scissorRegion.Width <= 0) || (scissorRegion.Height <= 0))
        //    {
        //        scissorRegion = System.Drawing.Rectangle.Empty;
        //    }

        //    // All done, take over the new scissor rectangle
        //    this.scissorManager.Assign(ref scissorRegion);
        //    return this.scissorManager;

        //}

        /// <summary>Draws a GUI element onto the drawing buffer</summary>
        /// <param name="frameName">Class of the element to draw</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="color">color</param>
        /// <remarks>
        ///   <para>
        ///     GUI elements are the basic building blocks of a GUI: 
        ///   </para>
        /// </remarks>
        public void DrawElement(string frameName, RectangleF bounds,Color color)
        {
            Frame frame = lookupFrame(frameName);

            // Draw all the regions defined for the element. Each region is a small bitmap
            // that needs to be blit somewhere into the element to form the element's
            // visual representation step by step.
            for (int index = 0; index < frame.Regions.Length; ++index)
            {
                Rectangle destinationRegion = calculateDestinationRectangle(ref bounds, ref frame.Regions[index].DestinationRegion);

                //this.spriteRenderer.Render(frame.Regions[index].Texture, destinationRegion, frame.Regions[index].SourceRegion, Color.White);
                spriteRenderer.Draw(frame.Regions[index].Texture, destinationRegion, frame.Regions[index].SourceRegion,color);
            }
        }

         public void DrawElement(string frameName, RectangleF bounds)
         {
             DrawElement(frameName, bounds, Color.White);
         }

        //the 2 DrawCustomTexture methods were added by Simon ! 
        public void DrawCustomTexture(SpriteTexture customTex, RectangleF bounds,int textureArrayIndex=0)
        {

            var offset = new UniRectangle(0, 0, bounds.Width, bounds.Height);
            var destinationRegion = calculateDestinationRectangle(
              ref bounds, ref offset
            );

            spriteRenderer.Draw(customTex, destinationRegion, new SharpDX.Rectangle(0, 0, customTex.Width, customTex.Height), Color.White,true ,textureArrayIndex);
        }

        public void DrawCustomTexture(SpriteTexture customTex, Rectangle textureSourceRect, RectangleF bounds)
        {

            var offset = new UniRectangle(0, 0, bounds.Width, bounds.Height);
            var destinationRegion = calculateDestinationRectangle(
              ref bounds, ref offset
            );

            spriteRenderer.Draw(customTex, destinationRegion, textureSourceRect, Color.White);

        }

        /// <summary>Draws text into the drawing buffer for the specified element</summary>
        /// <param name="frameName">Class of the element for which to draw text</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text that will be drawn</param>
        public void DrawString(string frameName, RectangleF bounds, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var frame = lookupFrame(frameName);

            // Draw the text in all anchor locations defined by the skin
            for (var index = 0; index < frame.Texts.Length; ++index)
            {
                spriteRenderer.DrawText(frame.Texts[index].Font, text, positionText(ref frame.Texts[index], bounds, text), frame.Texts[index].Color);
            }
        }

        /// <summary>Draws a caret for text input at the specified index</summary>
        /// <param name="frameName">Class of the element for which to draw a caret</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text for which a caret will be drawn</param>
        /// <param name="caretIndex">Index the caret will be drawn at</param>
        public void DrawCaret(
          string frameName, RectangleF bounds, string text, int caretIndex
        )
        {
            var frame = lookupFrame(frameName);

            stringBuilder.Remove(0, stringBuilder.Length);
            stringBuilder.Append(text, 0, caretIndex);

            for (int index = 0; index < frame.Texts.Length; ++index)
            {
                var textPosition = positionText(ref frame.Texts[index], bounds, text);

                var caretPosition = frame.Texts[index].Font.MeasureString(stringBuilder);
                caretPosition.X -= CaretWidth;
                caretPosition.Y = 0.0f;

                spriteRenderer.DrawText(frame.Texts[index].Font,"|", textPosition + caretPosition, frame.Texts[index].Color);
            }
        }

        /// <summary>Measures the extents of a string in the frame's area</summary>
        /// <param name="frameName">Class of the element whose text will be measured</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text that will be measured</param>
        /// <returns>
        ///   The size and extents of the specified string within the frame
        /// </returns>
        public RectangleF MeasureString(string frameName, RectangleF bounds, string text)
        {
            var frame = lookupFrame(frameName);

            var size = frame.Texts.Length > 0 ? frame.Texts[0].Font.MeasureString(text) : Vector2.Zero;

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
        public int GetClosestOpening(
          string frameName, RectangleF bounds, string text, Vector2 position
        )
        {
            var frame = lookupFrame(frameName);

            // exTODO: Find the closest gap across multiple text anchors
            //   Frames can repeat their text in several places. Though this is probably
            //   not used very often (if at all), it should work here consistently.

            int closestGap = -1;

            for (int index = 0; index < frame.Texts.Length; ++index)
            {
                var textPosition = positionText(ref frame.Texts[index], bounds, text);
                position.X -= textPosition.X;
                position.Y -= textPosition.Y;

                //float openingX = position.X;
                var openingIndex = openingLocator.FindClosestOpening(
                  frame.Texts[index].Font, text, position.X + CaretWidth
                );

                closestGap = openingIndex;
            }

            return closestGap;
        }

    }

} // namespace Nuclex.UserInterface.Visuals.Flat
