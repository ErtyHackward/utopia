using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_DXEngine.Main;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3DXEngine.Buffers;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Sprites._3D
{
    public class Sprite3DWithTexCoordBuffer : BaseComponent, ISprite3DBuffer
    {
        #region Private Variables
        private List<VertexPointSprite3DTexCoord> _spritesCollection;
        private VertexBuffer<VertexPointSprite3DTexCoord> _vb;
        private bool _isCollectionDirty;
        #endregion

        #region Public Properties
        #endregion

        public Sprite3DWithTexCoordBuffer(int TextureWidth, int TextureHeight)
        {

        }

        #region Public Methods
        public void Init(DeviceContext context, ResourceUsage usage = ResourceUsage.Dynamic)
        {
            _spritesCollection = new List<VertexPointSprite3DTexCoord>();
            _vb = new VertexBuffer<VertexPointSprite3DTexCoord>(context.Device, 16, VertexPointSprite3DTexCoord.VertexDeclaration, PrimitiveTopology.PointList, "VB Sprite3DProcessorTexCoord", usage, 10);
            _isCollectionDirty = false;
        }

        public void Begin()
        {
        }

        public void SetData(DeviceContext context)
        {
            if (_isCollectionDirty)
            {
                _vb.SetData(context, _spritesCollection.ToArray());
                _isCollectionDirty = false;
            }
        }

        public void Set2DeviceAndDraw(DeviceContext context)
        {
            if (_spritesCollection.Count == 0) return;
            _vb.SetToDevice(context, 0);
            context.Draw(_vb.VertexCount, 0);
            _spritesCollection.Clear(); //Free buffer;
        }

        public void Draw(ref Vector3 worldPosition, ref Vector2 size, ref ByteColor color, Sprite3DRenderer.SpriteRenderingType spriterenderingType, int textureArrayIndex = 0)
        {
            //Create the Vertex, add it into the vertex Collection
            Vector4 textCoordU = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
            Vector4 textCoordV = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);
            Draw(ref worldPosition, ref size, ref color, spriterenderingType, ref textCoordU, ref textCoordV, textureArrayIndex);
        }

        public void Draw(ref Vector3 worldPosition, ref Vector2 size, ref ByteColor color, Sprite3DRenderer.SpriteRenderingType spriterenderingType, ref Vector4 textCoordU, ref Vector4 textCoordV, int textureArrayIndex = 0)
        {
            Vector3 Info = new Vector3(size.X, size.Y, (int)spriterenderingType);

            _spritesCollection.Add(new VertexPointSprite3DTexCoord(new Vector4(worldPosition.X, worldPosition.Y, worldPosition.Z, textureArrayIndex), color, Info, textCoordU, textCoordV));
            _isCollectionDirty = true;
        }

        public void DrawText(string text, SpriteFont spriteFont, SpriteTexture texture, ref Vector3 worldPosition, float scaling, ref ByteColor color, ICamera camera, int textureArrayIndex = 0, bool XCenteredText = true, bool MultiLineHandling = false)
        {
            int nextLineOffset = 0;
            Vector3 origin;
            Vector3 textPosition = worldPosition;
            int nbrLine = 1;
            if (MultiLineHandling)
            {
                nbrLine = text.Count(f => f == '\n') + 1;
            }
            
            textPosition.Y += (spriteFont.CharHeight * scaling) * nbrLine; //remove the char. height

            if (XCenteredText)
            {
                if (!MultiLineHandling)
                {
                    textPosition.X -= (spriteFont.MeasureString(text).X / 2) * scaling; //Center text on X World Position
                }
                else
                {
                    textPosition.X -= (spriteFont.MeasureString(GetLine(ref text, nextLineOffset, out nextLineOffset)).X / 2) * scaling; //Center text on X World Position
                }
            }

            origin = worldPosition;

            bool newCharInserted = false;
            int length = text.Length;

            int numCharsToDraw = length;

            //For Each character in the text
            float currentLineWidth = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];

                if (character == '\r')
                {
                    continue;
                }

                //Managing Space
                if (character == ' ')
                {
                    textPosition.X += spriteFont.SpaceWidth * scaling;
                }
                else
                {
                    //Managing New Line
                    if (character == '\n')
                    {
                        InsertNewLine(ref text, ref textPosition, ref currentLineWidth, spriteFont, worldPosition.X, scaling, XCenteredText, MultiLineHandling, ref nextLineOffset);
                    }
                    else
                    {
                        //All other characters goes here
                        RectangleF desc = spriteFont.CharDescriptors[character];
                        //Transform the desc to Texture coordinate with World scaling
                        RectangleF sourceRectInTexCoord = new RectangleF((desc.Left / (float)texture.Width), desc.Top / (float)texture.Height, desc.Right / (float)texture.Width, desc.Bottom / (float)texture.Height);

                        Draw(ref textPosition, desc.Width * scaling, desc.Height * scaling, ref color, Sprite3DRenderer.SpriteRenderingType.BillboardOnLookAt, ref sourceRectInTexCoord, 0, camera, ref origin);
                        
                        textPosition.X += desc.Width * scaling;
                        newCharInserted = true;
                    }
                }

                if (newCharInserted) textPosition.X += 1 * scaling;
            }
        }

        private void Draw(ref Vector3 worldPosition, float width, float height, ref ByteColor color, Sprite3DRenderer.SpriteRenderingType spriterenderingType, ref RectangleF textCoord, int textureArrayIndex, ICamera camera, ref Vector3 origin)
        {

            Vector3 Offset = worldPosition - origin;

            Matrix rotateAndTranslate = Matrix.Translation(Offset) * Matrix.RotationQuaternion(Quaternion.Invert(camera.YAxisOrientation.ValueInterp)) * Matrix.Translation(origin);

            Vector3 newworldPosition = Vector3.TransformCoordinate(Vector3.Zero, rotateAndTranslate);

            Vector4 position = new Vector4(newworldPosition.X, newworldPosition.Y, newworldPosition.Z, textureArrayIndex);

            //Vector4 position = new Vector4(worldPosition.X, worldPosition.Y, worldPosition.Z, textureArrayIndex);
            Vector3 Info = new Vector3(width, height, (int)spriterenderingType);

            Vector4 textCoordU = new Vector4(textCoord.Right, textCoord.Left, textCoord.Right, textCoord.Left);
            Vector4 textCoordV = new Vector4(textCoord.Bottom, textCoord.Bottom, textCoord.Top, textCoord.Top);

            _spritesCollection.Add(new VertexPointSprite3DTexCoord(position, color, Info, textCoordU, textCoordV));
            _isCollectionDirty = true;
        }
        #endregion

        #region Private Methods
        private void InsertNewLine(ref string text, ref Vector3 textPosition, ref float currentLineWidth, SpriteFont spriteFont, float xOffset, float scaling, bool XCenteredText, bool MultiLineHandling, ref int nextLineOffset)
        {
            textPosition.Y -= spriteFont.CharHeight * scaling;
            textPosition.X = xOffset;

            if (XCenteredText)
            {
                if (!MultiLineHandling)
                {
                    textPosition.X -= (spriteFont.MeasureString(text).X / 2) * scaling; //Center text on X World Position
                }
                else
                {
                    textPosition.X -= (spriteFont.MeasureString(GetLine(ref text, nextLineOffset, out nextLineOffset)).X / 2) * scaling; //Center text on X World Position
                }
            }

            currentLineWidth = 0;
        }

        private string GetLine(ref string text, int LastLineOffset, out int nextLineOffset)
        {
            if (LastLineOffset >= text.Length) { nextLineOffset = LastLineOffset; return null; }
            int newlineOffset = text.IndexOf('\n', LastLineOffset);
            if (newlineOffset == -1)
            {
                newlineOffset = text.Length;
            }

            nextLineOffset = newlineOffset + 1;

            return text.Substring(LastLineOffset, newlineOffset - LastLineOffset);
        }
        #endregion

    }
}
