using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace S33M3Engines.Fonts
{
    /// <summary>
    /// Font class wraper for displaying FreeType fonts in bitmaps
    /// </summary>
    public class FTFontBitmap : FTFont
    {
        public FTFontBitmap(string resourcefilename, out int Success) : base(resourcefilename, out Success) { }
        public FTFontBitmap(string resourcefilename, out int Success, int faceindex) : base(resourcefilename, out Success, faceindex) { }

        private Bitmap[] images;

        public override void ftCheckGlyph(int c)
        {
            //If the Character is not exsiting, then create it
            if (images[c] == null)
                //This will first create the Glyph characteristics = FT_GlyphSlotRec, and then will "call back" the RenderAsTexture abstracted RenderAsTexture(...)
                CompileCharacter(face, c);          
        }

        protected override void ftRenderToTextureInternal()
        {
            images = new Bitmap[max_chars];
        }

        //Create the character
        protected override void RenderAsTexture(FT_GlyphSlotRec glyphrec, int c)
        {
            int ret = FT.FT_Render_Glyph(ref glyphrec, FT_Render_Mode.FT_RENDER_MODE_NORMAL);

            FTGlyphOffset offset = new FTGlyphOffset();

            if (ret != 0)
            {
                Report("Render failed for character " + c.ToString());
            }

            string sError = "";

            int size = (glyphrec.bitmap.width * glyphrec.bitmap.rows);
            if (size <= 0)
            {
                //Console.Write("Blank Character: " + c.ToString());
                //space is a special `blank` character
                extent_x[c] = 0;
                if (c == 32)
                {
                    extent_x[c] = font_size >> 1;
                    offset.left = 0;
                    offset.top = 0;
                    offset.height = 0;
                    offset.width = extent_x[c];
                    offsets[c] = offset;
                }
                return;

            }

            if ((c == 10) || (c == 13))
            {
                extent_x[c] = 0;
                return;
            }

            byte[] bmp = new byte[size];
            Marshal.Copy(glyphrec.bitmap.buffer, bmp, 0, bmp.Length);

            //Create bitmap with Widht and Height being a multiple of Pow2
            //int width = next_po2(glyphrec.bitmap.width);
            //int height = next_po2(glyphrec.bitmap.rows);

            images[c] = new Bitmap(glyphrec.bitmap.width, glyphrec.bitmap.rows);

            for (int height = 0; height < glyphrec.bitmap.rows; height++)
            {
                for (int width = 0; width < glyphrec.bitmap.width; width++)
                {
                    //// Alpha
                    //int alpha = (width >= glyphrec.bitmap.width || height >= glyphrec.bitmap.rows) ?
                    //    0 : ((int)bmp[width + glyphrec.bitmap.width * height]);

                    //int color = (width >= glyphrec.bitmap.width || height >= glyphrec.bitmap.rows) ?
                    //    0 : ((int)bmp[width + glyphrec.bitmap.width * height]);

                    int alpha = (int)bmp[width + glyphrec.bitmap.width * height];

                    Color pixcolor = Color.FromArgb(alpha, 255, 255, 255);

                    images[c].SetPixel(width, height, pixcolor);
                }
            }

            offset.left = glyphrec.bitmap_left;
            offset.top = glyphrec.bitmap_top;
            offset.height = glyphrec.bitmap.rows;
            offset.width = glyphrec.bitmap.width;
            offset.advance = glyphrec.advance;
            offset.lsb_delta = glyphrec.lsb_delta;
            offset.rsb_delta = glyphrec.rsb_delta;
            offset.linearHoriAdvance = glyphrec.linearHoriAdvance;
            offset.linearVertAdvance = glyphrec.linearVertAdvance;
            offsets[c] = offset;

            //Advance for the next character			
            extent_x[c] = offset.advance.x >> 6; // offset.left + offset.width;
            sChars += "f:" + c.ToString() + "[w:" + glyphrec.bitmap.width.ToString() + "][h:" + glyphrec.bitmap.rows.ToString() + "]" + sError;
        }


        /// <summary>
        /// Clear all OpenGL-related structures.
        /// </summary>
        public override void ftClearFont()
        {
            if (images != null)
            {
                foreach (Bitmap b in images.Where(x => x != null))
                    b.Dispose();
            }


            images = null;
            extent_x = null;
        }


        private System.Drawing.Imaging.ImageAttributes attributes = null;

        public void ftBeginFont(System.Drawing.Imaging.ImageAttributes Attributes)
        {
            attributes = Attributes;
        }

        /// <summary>
        /// Creates the font with a color matrix that will balance the colour of the font based on the ratio between the values passed for Red, Green, Blue and Alpha.
        /// For example: Red: 0.5, Green: 1, Blue, 1, Alpha, 1 will make Red 50% less. These values, in the ISE.FreeType context, work as multipliers on the 255 value for white.
        /// So in this context, it works just like OpenGL colour does, 0 being all-black, 1 being full-colour.
        /// </summary>
        /// <param name="Red">0 - no red. 1 - full red</param>
        /// <param name="Green">0 - no green. 1 - full green</param>
        /// <param name="Blue">0 - no blue. 1 - full blue</param>
        /// <param name="Alpha">0 - no alpha. 1 - full alpha</param>
        public void ftBeginFont(float Red, float Green, float Blue, float Alpha)
        {
            attributes = new System.Drawing.Imaging.ImageAttributes();
            System.Drawing.Imaging.ColorMatrix colormatrix = new System.Drawing.Imaging.ColorMatrix();

            colormatrix.Matrix00 = Red;
            colormatrix.Matrix11 = Green;
            colormatrix.Matrix22 = Blue;
            colormatrix.Matrix33 = Alpha;
            colormatrix.Matrix44 = 1.0f;
            attributes.SetColorMatrix(colormatrix);
        }

        public Graphics ftWrite(string text, Bitmap background)
        {
            int pos = 0;
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(background);

            for (int i = 0; i < text.Length; i++)
            {
                int charcode = (int)text[i];
                ftCheckGlyph(charcode);

                if (images[charcode] != null)
                {
                    Bitmap image = images[charcode];

                    if (attributes != null)
                        g.DrawImage(image, new System.Drawing.Rectangle(pos, 48 - offsets[charcode].top  , image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                    else
                        g.DrawImage(image, new System.Drawing.Rectangle(pos, 0, image.Width, image.Height));
                }

                pos += extent_x[charcode];
            }

            return g;
        }

        /// <summary>
        /// Restore the OpenGL state to what it was prior
        /// to starting to draw the font. Try to call this once per frame to reduce overheads. Do not call from inside a display list.
        /// </summary>
        public override void ftEndFont()
        {
            attributes = null;
        }
    }
}
