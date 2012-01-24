using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace S33M3Engines.Fonts
{
            //    //Testing
            //int Errors = 0;
            //S33M3Engines.Fonts.FTFontBitmap samplefont = new S33M3Engines.Fonts.FTFontBitmap("e:\\FreeSans.ttf", out Errors);
            //samplefont.kerning = true;

            ////samplefont.ftClearFont(); // Only if already been set
            //samplefont.ftRenderToTexture(24, 72);
            //samplefont.FT_ALIGN = Fonts.FTFontAlign.FT_ALIGN_CENTERED;

            //Bitmap image = new Bitmap(500,800, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            //samplefont.ftBeginFont(0,0,0,1);
            //samplefont.ftWrite("éVoix ambiguë d'un coeur qui au zéphyr préfère les", image);
            //samplefont.ftEndFont();

            //image.Save("e:\\test.bmp");

            //samplefont.Dispose();
            //S33M3Engines.Fonts.FTFontBitmap.DisposeFreeType();

    // for reporting errors
    public delegate void OnWriteEventType(string EventDetails);

    public enum FontRender { Outline, Filled, Texture };

    /// <summary>
    /// Glyph offset information for advanced rendering and/or conversions.
    /// </summary>
    public struct FTGlyphOffset
    {
        /// <summary>
        /// Width of the Glyph, in pixels.
        /// </summary>
        public int width;
        /// <summary>
        /// height of the Glyph, in pixels. Represents the number of scanlines
        /// </summary>
        public int height;
        /// <summary>
        /// For Bitmap-generated fonts, this is the top-bearing expressed in integer pixels.
        /// This is the distance from the baseline to the topmost Glyph scanline, upwards Y being positive.
        /// </summary>
        public int top;
        /// <summary>
        /// For Bitmap-generated fonts, this is the left-bearing expressed in integer pixels
        /// </summary>
        public int left;
        /// <summary>
        /// This is the transformed advance width for the glyph.
        /// </summary>
        public FT_Vector advance;
        /// <summary>
        /// The difference between hinted and unhinted left side bearing while autohinting is active. 0 otherwise.
        /// </summary>
        public long lsb_delta;
        /// <summary>
        /// The difference between hinted and unhinted right side bearing while autohinting is active. 0 otherwise.
        /// </summary>
        public long rsb_delta;
        /// <summary>
        /// The advance width of the unhinted glyph. Its value is expressed in 16.16 fractional pixels, unless FT_LOAD_LINEAR_DESIGN is set when loading the glyph. This field can be important to perform correct WYSIWYG layout. Only relevant for outline glyphs.
        /// </summary>
        public long linearHoriAdvance;
        /// <summary>
        /// The advance height of the unhinted glyph. Its value is expressed in 16.16 fractional pixels, unless FT_LOAD_LINEAR_DESIGN is set when loading the glyph. This field can be important to perform correct WYSIWYG layout. Only relevant for outline glyphs.
        /// </summary>
        public long linearVertAdvance;
    }

    /// <summary>
    /// For internal use, to represent the type of conversion to apply to the font
    /// </summary>
    public enum FTFontType
    {
        /// <summary>
        /// Font has not been initialised yet
        /// </summary>
        FT_NotInitialised,
        /// <summary>
        /// Font was converted to a series of Textures
        /// </summary>
        FT_Texture,
        /// <summary>
        /// Font was converted to a big texture map, representing a collection of glyphs
        /// </summary>
        FT_TextureMap,
        /// <summary>
        /// Font was converted to outlines and stored as display lists
        /// </summary>
        FT_Outline,
        /// <summary>
        /// Font was convered to Outliens and stored as Vertex Buffer Objects
        /// </summary>
        FT_OutlineVBO
    }

    /// <summary>
    /// Alignment of output text
    /// </summary> 
    public enum FTFontAlign
    {
        /// <summary>
        /// Left-align the text when it is drawn
        /// </summary>
        FT_ALIGN_LEFT,
        /// <summary>
        /// Center-align the text when it is drawn
        /// </summary>
        FT_ALIGN_CENTERED,
        /// <summary>
        /// Right-align the text when it is drawn
        /// </summary>
        FT_ALIGN_RIGHT
    }

    public static class FreeType
    {
        /// <summary>
        /// This event reports on the status of FreeType.
        /// This is useful to assign to this event to record down
        /// FreeType output to a debug log file, for example.
        /// </summary>
        public static OnWriteEventType OnWriteEvent;

        // Global FreeType library pointer
        public static System.IntPtr libptr = IntPtr.Zero;
    }

    /// <summary>
    /// Font class wraper for displaying FreeType fonts in OpenGL.
    /// </summary>
    public abstract class FTFont
    {


        //Public members        
        protected int font_size = 48;
        protected static int max_chars = 70000;
        private int font_max_chars = 0;


        // Whether the font was loaded Ok or not
        protected bool fontloaded = false;
        protected int[] extent_x;
        protected FTGlyphOffset[] offsets;

        protected System.IntPtr faceptr;
        protected FT_FaceRec face;

        // debug variable used to list the state of all characters rendered
        protected string sChars = "";

        protected static void Report(string ErrorText)
        {
            if (FreeType.OnWriteEvent != null)
                FreeType.OnWriteEvent(ErrorText);
            else
                Console.WriteLine(ErrorText);
        }

        /// <summary>
        /// Initialise the FreeType library
        /// </summary>
        /// <returns></returns>
        public static int ftInit()
        {
            // We begin by creating a library pointer            
            if (FreeType.libptr == IntPtr.Zero)
            {
                int ret = FT.FT_Init_FreeType(out FreeType.libptr);

                if (ret != 0)
                {
                    Report("Failed to start FreeType");
                }
                else
                {
                    Report("FreeType Loaded.");
                    Report("FreeType Version " + ftVersionString());
                }

                return ret;
            }

            return 0;
        }

        /// <summary>
        /// Font alignment public parameter
        /// </summary>		
        public FTFontAlign FT_ALIGN = FTFontAlign.FT_ALIGN_LEFT;

        /// <summary>
        /// Initialise the Font. Will Initialise the freetype library if not already done so
        /// </summary>
        /// <param name="resourcefilename">Path to the external font file</param>
        /// <param name="Success">Returns 0 if successful</param>
        public FTFont(string resourcefilename, out int Success)
            : this(resourcefilename, out Success, 0)
        {
        }

        /// <summary>
        /// Initialise the Font. Will Initialise the freetype library if not already done so
        /// </summary>
        /// <param name="resourcefilename">Path to the external font file</param>
        /// <param name="Success">Returns 0 if successful</param>
        /// <param name="fontindex">Specify the font face index (0-default, other positive integers may be italis/bold etc.. depending on the font)</param>
        public FTFont(string resourcefilename, out int Success, int faceindex)
        {
            Report("Creating Font " + resourcefilename);
            Success = ftInit();


            if (FreeType.libptr == IntPtr.Zero) { Report("Couldn't start FreeType"); Success = -1; return; }

            string fontfile = resourcefilename;

            //Once we have the library we create and load the font face                       
            int retb = FT.FT_New_Face(FreeType.libptr, fontfile, faceindex, out faceptr);

            if (retb != 0)
            {
                if (!File.Exists(fontfile))
                    Report(fontfile + " not found.");
                else
                    Report(fontfile + " found.");

                Report("Failed to load font " + fontfile + " (error code " + retb.ToString() + ")");
                fontloaded = true;
                Success = retb;
                return;
            }
            fontloaded = true;

            Success = 0;
        }

        /// <summary>
        /// Return the version information for FreeType.
        /// </summary>
        /// <param name="Major">Major Version</param>
        /// <param name="Minor">Minor Version</param>
        /// <param name="Patch">Patch Number</param>
        public static void ftVersion(ref int Major, ref int Minor, ref int Patch)
        {
            ftInit();
            FT.FT_Library_Version(FreeType.libptr, ref Major, ref Minor, ref Patch);
        }

        /// <summary>
        /// Return the entire version information for FreeType as a String.
        /// </summary>
        /// <returns></returns>
        public static string ftVersionString()
        {
            int major = 0;
            int minor = 0;
            int patch = 0;
            ftVersion(ref major, ref minor, ref patch);
            return major.ToString() + "." + minor.ToString() + "." + patch.ToString();
        }

        public bool kerning = false;

        public bool Kerning { get { return kerning; } set { kerning = value; } }

        internal int next_po2(int a)
        {
            int rval = 1;
            while (rval < a) rval <<= 1;
            return rval;
        }

        public abstract void ftCheckGlyph(int c);

        protected abstract void RenderAsTexture(FT_GlyphSlotRec glyphrec, int c);

        /// <summary>
        /// Render the font to a series of OpenGL textures (one per letter)
        /// </summary>
        /// <param name="fontsize">size of the font</param>
        /// <param name="DPI">dots-per-inch setting</param>
        public void ftRenderToTexture(int fontsize, uint DPI)
        {
            if (!fontloaded) return;
            font_size = fontsize;

            if (faceptr == IntPtr.Zero)
            {
                Report("ERROR: No Face Pointer. Font was not created properly");
                return;
            }

            face = (FT_FaceRec)Marshal.PtrToStructure(faceptr, typeof(FT_FaceRec));

            Report("Num Faces:" + face.num_faces.ToString());
            Report("Num Glyphs:" + face.num_glyphs.ToString());
            Report("Num Char Maps:" + face.num_charmaps.ToString());
            Report("Font Family:" + face.family_name);
            Report("Style Name:" + face.style_name);
            Report("Generic:" + face.generic);
            Report("Bbox:" + face.bbox);
            Report("Glyph:" + face.glyph);
            //kerning = FT.FT_ker(faceptr);

            font_max_chars = (int)face.num_glyphs;

            //   IConsole.Write("Num Glyphs:", );

            //Freetype measures the font size in 1/64th of pixels for accuracy 
            //so we need to request characters in size*64
            FT.FT_Set_Char_Size(faceptr, font_size << 6, font_size << 6, DPI, DPI);

            //Provide a reasonably accurate estimate for expected pixel sizes
            //when we later on create the bitmaps for the font            

            FT.FT_Set_Pixel_Sizes(faceptr, (uint)font_size - 2, (uint)font_size - 2);

            // Once we have the face loaded and sized we generate opengl textures 
            // from the glyphs  for each printable character
            Report("Compiling Font Characters 0.." + max_chars.ToString());
            extent_x = new int[max_chars];
            offsets = new FTGlyphOffset[max_chars];

            ftRenderToTextureInternal();

            //Console.WriteLine("Font Compiled:" + sChars);            
        }

        protected abstract void ftRenderToTextureInternal();

        protected void CompileCharacter(FT_FaceRec face, int c)
        {
            //We first convert the number index to a character index
            uint index = FT.FT_Get_Char_Index(faceptr, (uint)c);
            
            string sError = "";
            if (index == 0) sError = "No Glyph";

            //Here we load the actual glyph for the character
            int ret = FT.FT_Load_Glyph(faceptr, index, FT.FT_LOAD_RENDER);
            if (ret != 0)
            {
                Report("Load_Glyph failed for character " + c.ToString());
            }

            FT_GlyphSlotRec glyphrec = (FT_GlyphSlotRec)Marshal.PtrToStructure(face.glyph, typeof(FT_GlyphSlotRec));

            RenderAsTexture(glyphrec, c);
        }

        /// <summary>
        /// Dispose of the font
        /// </summary>
        public void Dispose()
        {
            ftClearFont();
            // Dispose of these as we don't need
            if (faceptr != IntPtr.Zero)
            {
                FT.FT_Done_Face(faceptr);
                faceptr = IntPtr.Zero;
            }

        }

        /// <summary>
        /// Dispose of the FreeType library
        /// </summary>
        public static void DisposeFreeType()
        {
            FT.FT_Done_FreeType(FreeType.libptr);
        }

        /// <summary>
        /// Clear all OpenGL-related structures.
        /// </summary>
        public abstract void ftClearFont();

        /// <summary>
        /// Return the horizontal extent (width),in pixels, of a given font string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public float ftExtent(ref string text)
        {
            if (extent_x == null) return 0;
            int ret = 0;
            for (int c = 0; c < text.Length; c++)
                if (text[c] == 10)
                    ret = 0;
                else
                {
                    ret += extent_x[text[c]];
                }
            return ret;
        }

        /// <summary>
        /// Return the Glyph offsets for the first character in "text"
        /// </summary>
        public FTGlyphOffset ftGetGlyphOffset(Char glyphchar)
        {
            return offsets[glyphchar];
        }

        public uint GetChar(uint index)
        {
            return FT.FT_Get_Char_Index(faceptr, index);
        }

        /*      /// <summary>
               /// Initialise the OpenGL state necessary for rendering the font properly. Call once prior to rendering of all font text. Do not put inside a display list.
               /// </summary>
               public abstract void ftBeginFont(bool AlphaMasking);
        
               #region ftWrite(string text)
               /// <summary>
               ///     Custom GL "write" routine.
               /// </summary>
               /// <param name="text">
               ///     The text to print.
               /// </param>
               public abstract void ftWrite(string text, ref int gllist);        
               #endregion
         */


        /*        #region ftWrite(string text)
                /// <summary>
                ///     Custom GL "write" routine. Slow version. If you use this for display lists, ensure you call this version first outside the display list to "preload"
                ///     all the characters. Once all the characters are preloaded, you may then call this function again inside a display list.
                ///     
                /// If you require white text with a displaylist, you may also consider the ftWrite(string text, ref listnumber) overload of this function, which will automatically
                /// create the display list for you in a single pass instead.
                /// </summary>
                /// <param name="text">
                ///     The text to print.
                /// </param>
                public abstract void ftWrite(string text);        
                #endregion*/

        /// <summary>
        /// Take an original line of text and then wrap it at a specific width. The wrapping is basic, and only off character sizes, it does not do word-wrapping at this time.
        /// The returned strign will contain the relevant carraige returns to wrap thetext at the required size.
        /// </summary>
        /// <param name="OriginalText"></param>
        /// <param name="TextWidth"></param>
        /// <returns></returns>
        public string ftWrapText(string OriginalText, float TextWidth)
        {
            string wt = "";
            string word = "";
            float extent = 0;
            byte[] newline = new byte[1];
            newline[0] = 10;
            float wordsize = 0;
            string string_newline = Encoding.ASCII.GetString(newline);

            OriginalText = OriginalText.Replace("<BR>", "\n");
            OriginalText = OriginalText.Replace("<br>", "\n");
            OriginalText = OriginalText.Replace("&nbsp;", " ");

            for (int i = 0; i < OriginalText.Length; i++)
            {
                string chr = OriginalText.Substring(i, 1);
                int chrcode = (int)chr[0];
                ftCheckGlyph(chrcode);
                float chrsize = ftExtent(ref chr);
                wordsize += chrsize;

                //  extent += chrsize;
                bool valid = true;

                if (chrcode == 32)
                {
                    if ((extent + wordsize) > TextWidth)
                    {
                        if (word != "")
                            wt += string_newline + word + " ";
                        else
                            wt += string_newline;

                        extent = wordsize;
                        word = "";
                        valid = false;
                    }
                    else
                    {
                        wt = wt + word + " ";
                        extent += wordsize;
                        word = "";
                        valid = false;
                    }

                    wordsize = 0;
                }

                if (chrcode == 10 || chrcode == 13)
                {
                    wt = wt + word + string_newline;
                    extent = 0;
                    wordsize = 0;
                    word = "";
                    valid = false;
                }

                if (valid) word += chr;

                if ((extent + wordsize) > TextWidth)
                {
                    wt += string_newline + word;
                    extent = wordsize;
                    word = "";
                    valid = false;
                }

            }

            wt += word;

            return wt;
        }

        /// <summary>
        /// Restore the OpenGL state to what it was prior
        /// to starting to draw the font. Try to call this once per frame to reduce overheads. Do not call from inside a display list.
        /// </summary>
        public abstract void ftEndFont();
    }
}
