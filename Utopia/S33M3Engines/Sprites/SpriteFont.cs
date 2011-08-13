﻿using System;
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

namespace S33M3Engines.Sprites
{
    public class SpriteFont
    {
        static char StartChar = '!';
        static char EndChar = (char)127;
        static int NumChars = EndChar - StartChar;
        static int TexWidth = 1024;

        #region Internal Types
        public struct CharDesc
        {
            public float X;
            public float Y;
            public float Width;
            public float Height;
        };
        #endregion

        #region Public Variables
        public SpriteTexture SpriteTexture;
        public CharDesc[] CharDescriptors = new CharDesc[255];
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
        #endregion

        #region Public Methods
        public void Initialize(string fontName, float fontSize, FontStyle fontStyle, bool antiAliased, SharpDX.Direct3D11.Device device)
        {
            _size = fontSize;
            TextRenderingHint hint = antiAliased ? TextRenderingHint.AntiAliasGridFit : TextRenderingHint.SystemDefault;

            Font font = new Font(fontName, fontSize, fontStyle, GraphicsUnit.Pixel);

            int size = (int)(fontSize * NumChars * 2) + 1;

            Bitmap sizeBitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);

            Graphics sizeGraphics = Graphics.FromImage(sizeBitmap);

            sizeGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            sizeGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            sizeGraphics.TextRenderingHint = hint;

            _charHeight = font.Height; // *1.1f;

            char[] allChars = new char[NumChars + 1];
            for (int i = 0; i < NumChars; ++i)
            {
                allChars[i] = (char)(i + StartChar);
            }
            allChars[NumChars] = (char)0;

            string allCharsString = new string(allChars);

            SizeF sizeRect = sizeGraphics.MeasureString(allCharsString, font, NumChars);

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
                drawGraphics.DrawString(new string(charString[0], 1), font, brush, new PointF(0, 0));

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

            sizeRect = drawGraphics.MeasureString(new string(charString[0], 1), font, NumChars);
            _spaceWidth = sizeRect.Width;

            // Lock the bitmap for direct memory access
            BitmapData bmData = textBitmap.LockBits(new Rectangle(0, 0, TexWidth, texHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            // Create a D3D texture, initalized with the bitmap data  
            Texture2DDescription texDesc = new Texture2DDescription();
            texDesc.Width = TexWidth;
            texDesc.Height = texHeight;
            texDesc.MipLevels = 1;
            texDesc.ArraySize = 1;
            texDesc.Format = Format.B8G8R8A8_UNorm;
            texDesc.SampleDescription = new SampleDescription(1, 0);
            texDesc.Usage = ResourceUsage.Immutable;
            texDesc.BindFlags = BindFlags.ShaderResource;
            texDesc.CpuAccessFlags = CpuAccessFlags.None;
            texDesc.OptionFlags = ResourceOptionFlags.None;

            DataRectangle data = new DataRectangle(TexWidth * 4, new DataStream(bmData.Scan0, 4 * TexWidth * texHeight, true, false));

            Texture2D texture = new Texture2D(device, texDesc, data);

            textBitmap.UnlockBits(bmData);

            ShaderResourceViewDescription srDesc = new ShaderResourceViewDescription();
            srDesc.Format = Format.B8G8R8A8_UNorm;
            srDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            srDesc.Texture2D = new ShaderResourceViewDescription.Texture2DResource() { MipLevels = 1, MostDetailedMip = 0 };

            _srView = new ShaderResourceView(device, texture, srDesc);

            SpriteTexture = new SpriteTexture(texture, _srView, new Vector2(0, 0));
            texture.Dispose();
        }
        #endregion


        public void Dispose()
        {
            if (_srView != null) _srView.Dispose();
            if (SpriteTexture != null) SpriteTexture.Dispose();

        }


        public Vector2 MeasureString(StringBuilder stringBuilder)
        {
            return (MeasureString(stringBuilder.ToString()));
        }

        public Vector2 MeasureString(string text)
        {
            return new Vector2(text.Length * _size, _charHeight);
            //HACK SpriteFont.MasureString is approximated to Vector2(text.Length * _size, _charHeight)
        }

        public float LineSpacing { get { return 4; } } //HACK SpriteFont.LineSpacing hardcoded
    }
}
