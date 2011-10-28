using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using System.Drawing.Text;
using System.Drawing.Imaging;
using SharpDX.DXGI;
using SharpDX;
using Rectangle = System.Drawing.Rectangle;
using System.Drawing;
using SharpDX.Direct3D;
using S33M3Engines.Shared.Sprites;
using RectangleF = System.Drawing.RectangleF;

namespace S33M3Engines.Sprites
{
    public class SpriteFont
    {
        static char StartChar = '!';
        static char EndChar = (char)127;
        static int NumChars = EndChar - StartChar;
        static int TexWidth = 1024;

        #region Public Variables
        public SpriteTexture SpriteTexture;
        public RectangleF[] CharDescriptors = new RectangleF[255];
        //public CharDesc& GetCharDescriptor(WCHAR character) const;
        public float Size { get { return _size; } }
        public int TextureWidth { get { return TexWidth; } }
        public int TextureHeight { get { return _texHeight; } }
        public float SpaceWidth { get { return _spaceWidth; } }
        public float CharHeight { get { return _charHeight; } }
        #endregion

        #region Private Variables
        protected ShaderResourceView _srView;
        protected float _size;
        protected int _texHeight;
        protected float _spaceWidth;
        protected float _charHeight;
        protected Font _font;
        protected Graphics _fontGraphics;
        #endregion

        #region Public Methods
        public void Initialize(string fontName, float fontSize, FontStyle fontStyle, bool antiAliased, SharpDX.Direct3D11.Device device)
        {
            _size = fontSize;
            TextRenderingHint hint = antiAliased ? TextRenderingHint.AntiAliasGridFit : TextRenderingHint.SystemDefault;

            _font = new Font(fontName, fontSize, fontStyle, GraphicsUnit.Pixel);

            int size = (int)(fontSize * NumChars * 2) + 1;

            Bitmap sizeBitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);

            _fontGraphics = Graphics.FromImage(sizeBitmap);

            _fontGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            _fontGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            _fontGraphics.TextRenderingHint = hint;

            _charHeight = _font.Height;// * 1.1f;

            char[] allChars = new char[NumChars + 1];
            for (int i = 0; i < NumChars; ++i)
            {
                allChars[i] = (char)(i + StartChar);
            }
            allChars[NumChars] = (char)0;

            string allCharsString = new string(allChars);

            SizeF sizeRect = _fontGraphics.MeasureString(allCharsString, _font, NumChars);

            int numRows = (int)(sizeRect.Width / TexWidth) + 1;
            int texHeight = (int)(numRows * _charHeight) + 1;

            // Create a temporary Bitmap and Graphics for drawing the characters one by one
            int tempSize = (int)(fontSize * 2);
            Bitmap drawBitmap = new Bitmap(tempSize, tempSize, PixelFormat.Format32bppArgb);
            Graphics drawGraphics = Graphics.FromImage(drawBitmap);
            drawGraphics.TextRenderingHint = hint;

            // Create a temporary Bitmap + Graphics for creating a full character set
            Bitmap textBitmap = new Bitmap(TexWidth, texHeight, PixelFormat.Format32bppArgb);
            Graphics textGraphics = Graphics.FromImage(textBitmap);
            textGraphics.Clear(Color.FromArgb(0, 255, 255, 255));
            textGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

            // Solid brush for text rendering
            SolidBrush brush = new SolidBrush(Color.FromArgb(255, 255, 255, 255));

            // Draw all of the characters, and copy them to the full character set               
            char[] charString = new char[2];
            charString[1] = (char)0;
            int currentX = 0;
            int currentY = 0;
            for (int i = 0; i < NumChars; ++i)
            {
                charString[0] = (char)(i + StartChar);

                // Draw the character
                drawGraphics.Clear(Color.FromArgb(0, 255, 255, 255));
                drawGraphics.DrawString(new string(charString[0], 1), _font, brush, new PointF(0, 0));

                // Figure out the amount of blank space before the character
                int minX = 0;
                for (int x = 0; x < tempSize; ++x)
                {
                    for (int y = 0; y < tempSize; ++y)
                    {
                        Color color;
                        color = drawBitmap.GetPixel(x, y);
                        if (color.A > 0)
                        {
                            minX = x;
                            x = tempSize;
                            break;
                        }
                    }
                }

                // Figure out the amount of blank space after the character
                int maxX = tempSize - 1;
                for (int x = tempSize - 1; x >= 0; --x)
                {
                    for (int y = 0; y < tempSize; ++y)
                    {
                        Color color;
                        color = drawBitmap.GetPixel(x, y);
                        if (color.A > 0)
                        {
                            maxX = x;
                            x = -1;
                            break;
                        }
                    }
                }

                int charWidth = maxX - minX + 1;

                // Figure out if we need to move to the next row
                if (currentX + charWidth >= TexWidth)
                {
                    currentX = 0;
                    currentY += (int)(_charHeight) + 1;
                }

                // Fill out the structure describing the character position
                CharDescriptors[i + StartChar].X = (float)(currentX);
                CharDescriptors[i + StartChar].Y = (float)(currentY);
                CharDescriptors[i + StartChar].Width = (float)(charWidth);
                CharDescriptors[i + StartChar].Height = (float)(_charHeight);

                // Copy the character over 
                int height = (int)(_charHeight + 1);
                textGraphics.DrawImage(drawBitmap, currentX, currentY, new Rectangle(minX, 0, charWidth, height), GraphicsUnit.Pixel);
                currentX += charWidth + 1;
            }

            // Figure out the width of a space character
            charString[0] = ' ';
            charString[1] = (char)0;

            sizeRect = drawGraphics.MeasureString(new string(charString[0], 1), _font, NumChars);
            _spaceWidth = sizeRect.Width;

            // Lock the bitmap for direct memory access
            BitmapData bmData = textBitmap.LockBits(new Rectangle(0, 0, TexWidth, texHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            // Create a D3D texture, initalized with the bitmap data  
            Texture2DDescription texDesc = new Texture2DDescription();
            texDesc.Width = TexWidth;
            texDesc.Height = texHeight;
            texDesc.MipLevels = 1;
            texDesc.ArraySize = 1;
            texDesc.Format = Format.R8G8B8A8_UNorm;
            texDesc.SampleDescription = new SampleDescription(1, 0);
            texDesc.Usage = ResourceUsage.Immutable;
            texDesc.BindFlags = BindFlags.ShaderResource;
            texDesc.CpuAccessFlags = CpuAccessFlags.None;
            texDesc.OptionFlags = ResourceOptionFlags.None;

            DataRectangle data = new DataRectangle(new DataStream(bmData.Scan0, 4 * TexWidth * texHeight, true, false).DataPointer, TexWidth * 4);

            Texture2D texture = new Texture2D(device, texDesc, data);

            textBitmap.UnlockBits(bmData);

            ShaderResourceViewDescription srDesc = new ShaderResourceViewDescription();
            srDesc.Format = Format.R8G8B8A8_UNorm;
            //srDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            //srDesc.Texture2D = new ShaderResourceViewDescription.Texture2DResource() { MipLevels = 1, MostDetailedMip = 0 };
            srDesc.Dimension = ShaderResourceViewDimension.Texture2DArray;
            srDesc.Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource() { MostDetailedMip = 0, MipLevels = 1, FirstArraySlice = 0, ArraySize = 1 };

            _srView = new ShaderResourceView(device, texture, srDesc);

            SpriteTexture = new SpriteTexture(texture, _srView, new Vector2(0, 0));
            texture.Dispose();
        }
        #endregion


        public void Dispose()
        {
            if (_srView != null) _srView.Dispose();
            if (SpriteTexture != null) SpriteTexture.Dispose();
            if (_fontGraphics != null) _fontGraphics.Dispose();
            if (_font != null) _font.Dispose();
        }


        public Vector2 MeasureString(StringBuilder stringBuilder)
        {
            return (MeasureString(stringBuilder.ToString()));
        }

        public Vector2 MeasureString3(string text)
        {
            //return new Vector2(text.Length * _size, _charHeight);
            //HACK SpriteFont.MasureString is approximated to Vector2(text.Length * _size, _charHeight)
            SizeF sizeRect = (_fontGraphics.MeasureString(text, _font));
            return new Vector2(sizeRect.Width, sizeRect.Height);
        }

        public Vector2 MeasureString(string text)
        {
            int length = text.Length;
            float textWidth = 0;
            float textHeight = CharHeight;
            for (int i = 0; i < length; ++i)
            {
                char character = text[i];
                if (character == ' ')
                    textWidth += this.SpaceWidth;
                else if (character == '\n')
                {
                    textHeight += CharHeight;
                }
                else
                {
                    var desc = CharDescriptors[character];
                    textWidth += desc.Width;
                }
            }

            return new Vector2(textWidth, textHeight);

        }

        public Vector2 MeasureString2(string text)
        {
            System.Drawing.StringFormat format = new System.Drawing.StringFormat();
            System.Drawing.RectangleF rect = new System.Drawing.RectangleF(0, 0, 1000, 1000);
            System.Drawing.CharacterRange[] ranges = { new System.Drawing.CharacterRange(0, text.Length) };
            System.Drawing.Region[] regions = new System.Drawing.Region[1];

            format.SetMeasurableCharacterRanges(ranges);

            regions = _fontGraphics.MeasureCharacterRanges(text, _font, rect, format);
            rect = regions[0].GetBounds(_fontGraphics);

            return new Vector2(rect.Right + 1.0f, _charHeight);
        }

        public float LineSpacing { get { return _charHeight * 0.1f; } } //HACK SpriteFont.LineSpacing hardcoded
    }
}
