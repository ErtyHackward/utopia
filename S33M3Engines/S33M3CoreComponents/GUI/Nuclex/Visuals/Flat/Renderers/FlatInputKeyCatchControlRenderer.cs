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
using System.Collections.Generic;
using SharpDX;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop.Interfaces;
using S33M3Resources.Structs;

namespace S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Renderers
{

    /// <summary>Renders text input controls in a traditional flat style</summary>
    public class FlatInputKeyCatchControlRenderer : IFlatControlRenderer<Controls.Desktop.InputKeyCatchControl>, IOpeningLocator
    {

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>Style from the skin this renderer uses</summary>
        private const string Style = "input.normal";

        /// <summary>
        ///   Renders the specified control using the provided graphics interface
        /// </summary>
        /// <param name="control">Control that will be rendered</param>
        /// <param name="graphics">
        ///   Graphics interface that will be used to draw the control
        /// </param>
        public void Render(Controls.Desktop.InputKeyCatchControl control, IFlatGuiGraphics graphics)
        {
            RectangleF controlBounds = control.GetAbsoluteBounds();

            // Draw the control's frame and background
            if (control.CustomBackground == null)
            {
                if (control.HasFocus)
                {
                    graphics.DrawElement(Style, ref  controlBounds, ref control.HasFocusBackColor);
                }
                else
                {
                    graphics.DrawElement(Style, ref  controlBounds);
                }
            }
            else
            {
                graphics.DrawCustomTexture(control.CustomBackground, ref controlBounds);
            }

            if (control.CustomHintImage != null && string.IsNullOrEmpty(control.Text) && !control.HasFocus)
            {
                var bounds = controlBounds;

                bounds.Width = control.CustomHintImage.Width;
                bounds.Height = control.CustomHintImage.Height;

                var dx = (controlBounds.Height - bounds.Height) / 2;

                bounds.X += dx;
                bounds.Y += dx;

                graphics.DrawCustomTexture(control.CustomHintImage, ref bounds);
            }

            using (graphics.SetClipRegion(ref controlBounds))
            {

                string text = control.Text ?? string.Empty;

                // Amount by which the text will be moved within the input box in
                // order to keep the caret in view even when the text is wider than
                // the input box.
                float left = 0;

                // Only scroll the text within the input box when it has the input
                // focus and the caret is being shown.
                if (control.HasFocus)
                {

                    // Find out where the cursor is from the left end of the text
                    RectangleF stringSize = graphics.MeasureString(Style,ref controlBounds, text);

                    // exTODO: Renderer should query the size of the control's frame
                    //   Otherwise text will be visible over the frame, which might look bad
                    //   if a skin uses a frame wider than 2 pixels or in a different color
                    //   than the text.
                    while (stringSize.Width + left > controlBounds.Width)
                    {
                        left -= controlBounds.Width / 10.0f;
                    }

                }

                // Draw the text into the input box
                controlBounds.X += left;

                string textToDraw = control.Text;

                // If the input box is in focus, also draw the caret so the user knows
                // where characters will be inserted into the text.
                if (control.ColorSet)
                {
                    ByteColor color = control.Color;

                    if (control.CustomFont != null)
                    {
                        var move = (controlBounds.Height - control.CustomFont.CharHeight) / 2;
                        controlBounds.X += move;
                        controlBounds.Y += move;
#if DEBUG
                        if (move <= 2)
                        {
                            logger.Warn("Input component height ({0}) too small for the font size ({1})!", controlBounds.Height, control.CustomFont.CharHeight);
                        }
#endif
                        graphics.DrawString(control.CustomFont, ref controlBounds, textToDraw, ref color, false);
                    }
                    else
                        graphics.DrawString(Style, 0, ref controlBounds, textToDraw, ref color, false);
                }
                else
                {
                    if (control.CustomFont != null)
                    {
                        ByteColor color = control.Color;
                        var move = (controlBounds.Height - control.CustomFont.CharHeight) / 2;
                        controlBounds.X += move;
                        controlBounds.Y += move;

                        graphics.DrawString(control.CustomFont, ref controlBounds, textToDraw, ref color, false);
                    }
                    else
                    {
                        graphics.DrawString(Style, 0, ref controlBounds, textToDraw, false);
                    }
                }

            }

            // Let the control know that we can provide it with additional informations
            // about how its text is being rendered
            control.OpeningLocator = this;
            this.graphics = graphics;
        }

        /// <summary>
        ///   Calculates which opening between two letters is closest to a position
        /// </summary>
        /// <param name="bounds">
        ///   Boundaries of the control, should be in absolute coordinates
        /// </param>
        /// <param name="text">Text in which the opening will be looked for</param>
        /// <param name="position">
        ///   Position to which the closest opening will be found,
        ///   should be in absolute coordinates
        /// </param>
        /// <returns>The index of the opening closest to the provided position</returns>
        public int GetClosestOpening(ref RectangleF bounds, string text,ref Vector2 position)
        {
            return this.graphics.GetClosestOpening("input.normal",ref bounds, text,ref position);
        }

        // exTODO: Find a better solution than remembering the graphics interface here
        //   Otherwise the renderer could try to renderer when no frame is being drawn.
        //   Also, the renderer makes the assumption that all drawing happens through
        //   one graphics interface only.

        /// <summary>Graphics interface we used for the last draw call</summary>
        private IFlatGuiGraphics graphics;

    }
}
