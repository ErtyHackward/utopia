using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Sprites2D;
using S33M3CoreComponents.Sprites3D.Interfaces;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3Resources.Effects.Sprites;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Sprites3D.Processors
{
    public class Sprite3DTextProc : BaseComponent, ISprite3DProcessor
    {
        #region Private Variables
        private List<VertexPointSprite3DTexCoord> _spritesCollection;
        private VertexBuffer<VertexPointSprite3DTexCoord> _vb;
        private bool _isCollectionDirty;
        private SpriteFont _spriteFont;
        private HLSLPointSprite3DText _effect;
        private SamplerState _spriteSampler;
        private Include _sharedCBIncludeHandler;
        private iCBuffer _frameSharedCB;
        private string _effectFilePath;
        #endregion

        #region Public Properties
        #endregion
        public Sprite3DTextProc(SpriteFont spriteFont, SamplerState SpriteSampler, Include sharedCBIncludeHandler, iCBuffer frameSharedCB, string EffectFilePath)
        {
            _spriteFont = spriteFont;
            _spriteSampler = SpriteSampler;
            _sharedCBIncludeHandler = sharedCBIncludeHandler;
            _frameSharedCB = frameSharedCB;
            _effectFilePath = EffectFilePath;
        }

        #region Public Methods
        public void Init(DeviceContext context, ResourceUsage usage = ResourceUsage.Dynamic)
        {
            _effect = ToDispose(new HLSLPointSprite3DText(context.Device, _effectFilePath, VertexPointSprite3DTexCoord.VertexDeclaration, _frameSharedCB, _sharedCBIncludeHandler));

            //Set the Texture
            _effect.DiffuseTexture.Value = _spriteFont.SpriteTexture.Texture;
            _effect.SamplerDiffuse.Value = _spriteSampler;

            _spritesCollection = new List<VertexPointSprite3DTexCoord>();
            _vb = new VertexBuffer<VertexPointSprite3DTexCoord>(context.Device, 16, PrimitiveTopology.PointList, "VB Sprite3DProcessorTexCoord", usage, 10);
            _isCollectionDirty = false;
        }

        public void Begin()
        {
            _spritesCollection.Clear(); //Free buffer;
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

            //Set Effect Constant Buffer
            _effect.Begin(context);
            _effect.Apply(context);

            _vb.SetToDevice(context, 0);
            context.Draw(_vb.VertexCount, 0);
        }

        public void DrawText(string text, ref Vector3 worldPosition, float scaling, ref ByteColor color, ICamera camera, int textureArrayIndex = 0, bool XCenteredText = true, bool MultiLineHandling = false)
        {
            int nextLineOffset = 0;
            Vector3 origin = worldPosition;

            Vector3 localPosition = Vector3.Zero;
            int nbrLine = 1;
            if (MultiLineHandling)
            {
                nbrLine = text.Count(f => f == '\n') + 1;
            }

            localPosition.Y += (_spriteFont.CharHeight) * nbrLine; //remove the char. height

            if (XCenteredText)
            {
                if (!MultiLineHandling)
                {
                    localPosition.X -= (_spriteFont.MeasureString(text).X / 2); //Center text on X World Position
                }
                else
                {
                    localPosition.X -= (_spriteFont.MeasureString(GetLine(ref text, nextLineOffset, out nextLineOffset)).X / 2); //Center text on X World Position
                }
            }

            int length = text.Length;

            int numCharsToDraw = length;
            char previousChar = (char)0;
            bool isFirstChar = true;

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
                    localPosition.X += _spriteFont.SpaceWidth;
                }
                else
                {
                    //Managing New Line
                    if (character == '\n')
                    {
                        InsertNewLine(ref text, ref localPosition, ref currentLineWidth, _spriteFont, XCenteredText, MultiLineHandling, ref nextLineOffset);
                    }
                    else
                    {
                        //All other characters goes here
                        RectangleF desc = _spriteFont.CharDescriptors[character];
                        //Create texture coordinate in Texture coordinate
                        RectangleF sourceRectInTexCoord = new RectangleF(desc.Left / (float)_spriteFont.SpriteTexture.Width, 
                                                                         desc.Top / (float)_spriteFont.SpriteTexture.Height, 
                                                                         desc.Width / (float)_spriteFont.SpriteTexture.Width, 
                                                                         desc.Height / (float)_spriteFont.SpriteTexture.Height);

                        //Apply Kerning
                        if (!isFirstChar && _spriteFont.WithKerning)
                        {
                            int kerningAmount = _spriteFont.GetKerning(previousChar, character);
                            localPosition.X += kerningAmount;
                        }

                        //The drawing is done from the center of the font in 3d mode !
                        localPosition.X += (desc.Width / 2);

                        AddNewGlyph(ref localPosition, scaling, desc.Width, desc.Height, ref color,  ref sourceRectInTexCoord, 0, camera, ref origin);

                        localPosition.X += (desc.Width / 2);

                        previousChar = character;
                        isFirstChar = false;
                    }
                }

            }
        }
        #endregion

        #region Private Methods

        private void AddNewGlyph(ref Vector3 Offset, float scaling, float width, float height, ref ByteColor color, ref RectangleF textCoord, int textureArrayIndex, ICamera camera, ref Vector3 origin)
        {
            Matrix scaleRotateAndTranslate = Matrix.Transpose(Matrix.Scaling(scaling) * Matrix.RotationQuaternion(Quaternion.Invert(camera.Orientation.ValueInterp)) * Matrix.Translation(origin));
            //Vector3 worldPosition = Vector3.TransformCoordinate(Offset, scaleRotateAndTranslate);

            Vector4 position = new Vector4(Offset.X, Offset.Y, Offset.Z, textureArrayIndex);
            Vector4 textCoordU = new Vector4(textCoord.Right, textCoord.Left, textCoord.Right, textCoord.Left);
            Vector4 textCoordV = new Vector4(textCoord.Bottom, textCoord.Bottom, textCoord.Top, textCoord.Top);
            Vector2 Size = new Vector2(width, height);

            _spritesCollection.Add(new VertexPointSprite3DTexCoord(ref scaleRotateAndTranslate, ref position, ref color, ref Size, ref textCoordU, ref textCoordV));
            _isCollectionDirty = true;
        }

        private void InsertNewLine(ref string text, ref Vector3 textPosition, ref float currentLineWidth, SpriteFont spriteFont, bool XCenteredText, bool MultiLineHandling, ref int nextLineOffset)
        {
            textPosition.Y -= spriteFont.CharHeight;
            textPosition.X = 0;

            if (XCenteredText)
            {
                if (!MultiLineHandling)
                {
                    textPosition.X -= (spriteFont.MeasureString(text).X / 2); //Center text on X World Position
                }
                else
                {
                    textPosition.X -= (spriteFont.MeasureString(GetLine(ref text, nextLineOffset, out nextLineOffset)).X / 2); //Center text on X World Position
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
