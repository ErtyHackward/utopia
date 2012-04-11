﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using System.Drawing.Text;
using System.Drawing;
using System.Drawing.Imaging;
using SharpDX.DXGI;
using Rectangle = System.Drawing.Rectangle;
using RectangleF = System.Drawing.RectangleF;
using SharpDX.Direct3D;

namespace S33M3CoreComponents.Sprites
{
    public class SpriteFont : Component
    {
        static char StartChar = '!';
        static char EndChar = (char)255;
        static int NumChars = EndChar - StartChar;
        static int TexWidth = 2048;

        #region Public Variables
        public FontStyle FontStyle;
        public SpriteTexture SpriteTexture;
        public RectangleF[] CharDescriptors = new RectangleF[255];
        //public CharDesc& GetCharDescriptor(WCHAR character) const;
        public float Size { get { return _size; } }
        public int TextureWidth { get { return TexWidth; } }
        public int TextureHeight { get { return _texHeight; } }
        public float SpaceWidth { get { return _spaceWidth; } }
        public float CharHeight { get { return _charHeight; } }
        public float HeightInPoints { get; set; }
        public float HeightInPixel { get; set; }
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

        public void Initialize(FontFamily family, float fontSize, FontStyle fontStyle, bool antiAliased, SharpDX.Direct3D11.Device device)
        {
            _size = fontSize;
            this.FontStyle = fontStyle;
            _font = ToDispose(new Font(family, fontSize, fontStyle, GraphicsUnit.Pixel));
            Initialize(antiAliased, device);
        }

        public void Initialize(string fontName, float fontSize, FontStyle fontStyle, bool antiAliased, SharpDX.Direct3D11.Device device)
        {
            _size = fontSize;
            this.FontStyle = fontStyle;
            _font = ToDispose(new Font(fontName, fontSize, fontStyle, GraphicsUnit.Pixel));
            Initialize(antiAliased, device);
        }

        private void Initialize(bool antiAliased, SharpDX.Direct3D11.Device device)
        {
            TextRenderingHint hint = antiAliased ? TextRenderingHint.AntiAliasGridFit : TextRenderingHint.SystemDefault;


            Bitmap measuringBitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);

            _fontGraphics = ToDispose(Graphics.FromImage(measuringBitmap));

            _fontGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            _fontGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            _fontGraphics.TextRenderingHint = hint;

            HeightInPoints = _font.SizeInPoints;
            _charHeight = _font.Height;// * 1.1f;

            var ascent = _font.FontFamily.GetCellAscent(FontStyle.Regular);
            var descent = _font.FontFamily.GetCellDescent(FontStyle.Regular);

            var ascentPixel = _font.Size * ascent / _font.FontFamily.GetEmHeight(FontStyle);
            var descentPixel = _font.Size * descent / _font.FontFamily.GetEmHeight(FontStyle);

            HeightInPixel = ascentPixel + descentPixel;

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
            int tempSize = (int)(_size * 2);
            Bitmap drawBitmap = new Bitmap(tempSize, tempSize, PixelFormat.Format32bppArgb);
            Graphics drawGraphics = Graphics.FromImage(drawBitmap);
            drawGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            drawGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
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

            _srView = ToDispose(new ShaderResourceView(device, texture, srDesc));

            SpriteTexture = ToDispose(new SpriteTexture(texture, _srView, new Vector2(0, 0)));
            texture.Dispose();

            measuringBitmap.Dispose();
            textGraphics.Dispose();
            drawGraphics.Dispose();
            drawBitmap.Dispose();

        }
        #endregion

        public Vector2 MeasureString(StringBuilder stringBuilder)
        {
            return (MeasureString(stringBuilder.ToString()));
        }

        public Vector2 MeasureString3(string text)
        {
            text += StartChar;
            SizeF sizeRect = (_fontGraphics.MeasureString(text, _font));
            sizeRect.Width -= CharDescriptors[StartChar].Width;
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

            textWidth += length; // spacing between characters

            var test = MeasureString3(text);

            return new Vector2(textWidth, textHeight);
        }

        public struct WordInfo
        {
            public int IndexStart;
            public int Length;
            public float Width;
        }

        public void MeasureStringWords(string text, float maxWidth, out WordInfo[] infos)
        {
            var space = 1;
            int arraySize = 1;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == ' ')
                    arraySize++;
                if (text[i] == '\n' || text[i] == '\r')
                    arraySize += 2;
            }

            var wordWidths = new WordInfo[arraySize];

            // collect words lengths
            int arrayIndex = 0;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == ' ')
                {
                    if (wordWidths[arrayIndex].Width > 0) wordWidths[arrayIndex].Width -= space;
                    arrayIndex++;
                    if (wordWidths.Length > arrayIndex) wordWidths[arrayIndex].IndexStart = i + 1;
                }
                else if (c == '\n' || c == '\r')
                {
                    if (wordWidths[arrayIndex].Width > 0) wordWidths[arrayIndex].Width -= space;
                    arrayIndex++;
                    wordWidths[arrayIndex].IndexStart = i;
                    wordWidths[arrayIndex].Width = -1;
                    arrayIndex++;
                    if (wordWidths.Length > arrayIndex) wordWidths[arrayIndex].IndexStart = i + 1;
                }
                else
                {
                    wordWidths[arrayIndex].Width += CharDescriptors[c].Width + space;
                    wordWidths[arrayIndex].Length++;
                }
            }

            //// calculate the size
            //var size = new Vector2();
            //size.Y = CharHeight;
            //var width = 0f;

            //for (int i = 0; i < arraySize; i++)
            //{
            //    if (wordWidths[i].Width == -1)
            //    {
            //        size.Y += CharHeight;
            //        width = wordWidths[++i].Width;
            //    }
            //    else if (width + wordWidths[i].Width > maxWidth)
            //    {
            //        size.Y += CharHeight;
            //        width = wordWidths[i].Width;
            //    }
            //    else
            //    {
            //        width += wordWidths[i].Width + SpaceWidth;
            //        if (size.X <= width - SpaceWidth)
            //            size.X = width - SpaceWidth;
            //    }
            //}

            infos = wordWidths;

            //return size;
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

        public float LineSpacing { get { return _charHeight * 0.1f; } } 
    }
}
