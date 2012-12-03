﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_DXEngine.Main;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Sprites2D;
using S33M3CoreComponents.Sprites3D.Interfaces;
using S33M3DXEngine.Buffers;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Sprites3D
{
    public class Sprite3DBuffer : BaseComponent, ISprite3DBuffer
    {
        #region Private Variables
        private List<VertexPointSprite3D> _spritesCollection;
        private VertexBuffer<VertexPointSprite3D> _vb;
        private bool _isCollectionDirty;
        #endregion

        #region Public Properties
        #endregion

        #region Public Methods
        public void Init(DeviceContext context, ResourceUsage usage = ResourceUsage.Dynamic)
        {
            _spritesCollection = new List<VertexPointSprite3D>();
            _vb = new VertexBuffer<VertexPointSprite3D>(context.Device, 16, VertexPointSprite3D.VertexDeclaration, PrimitiveTopology.PointList, "VB Sprite3DProcessor", usage, 10);
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
            Vector3 Info = new Vector3(size.X, size.Y, (int)spriterenderingType);

            _spritesCollection.Add(new VertexPointSprite3D(new Vector4(worldPosition.X, worldPosition.Y, worldPosition.Z, textureArrayIndex), color, Info));

            _isCollectionDirty = true;
        }

        public void Draw(ref Vector3 worldPosition, ref Vector2 size, ref ByteColor color, Sprite3DRenderer.SpriteRenderingType spriterenderingType, ref Vector4 textCoordU, ref Vector4 textCoordV, int textureArrayIndex = 0)
        {
            Draw(ref worldPosition, ref size, ref color, spriterenderingType, textureArrayIndex);
        }

        public void DrawText(string text, SpriteFont spriteFont, SpriteTexture texture, ref Vector3 worldPosition, float scaling, ref ByteColor color, ICamera camera, int textureArrayIndex = 0, bool XCenteredText = true, bool MultiLineHandling = false)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        #endregion






    }
}