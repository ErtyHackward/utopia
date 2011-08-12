using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using RectangleF = System.Drawing.RectangleF;

namespace Utopia.GUI.cegui
{
    public class UISpriteBatchManager : IDisposable
    {
        public List<UISpriteBatch> SpriteBatchs;

        private int _quadPerBatchCount;
        private UISpriteBatch _currentBatch;

        public class UISpriteBatch
        {
            public SpriteGuiTexture SpriteTexture;
            public List<VertexSpriteInstanced> SpriteData;

            private int _maxQuadPerBatchCount;

            public UISpriteBatch(SpriteGuiTexture spriteTexture, int QuadPerBatchCount = 1000)
            {
                _maxQuadPerBatchCount = QuadPerBatchCount;
                SpriteData = new List<VertexSpriteInstanced>(_maxQuadPerBatchCount);
                SpriteTexture = spriteTexture;
            }

            public void AddSpriteData(ref Matrix transform,ref Color4 color,ref RectangleF sourceRect)
            {
                SpriteData.Add(new VertexSpriteInstanced() { Color = color, SourceRect = sourceRect, Tranform = transform });
            }
        }

        public UISpriteBatchManager(int SpriteBatchCount, int QuadPerBatchCount)
        {
            _quadPerBatchCount = QuadPerBatchCount;
            SpriteBatchs = new List<UISpriteBatch>(SpriteBatchCount);
        }

        public void AddSprite(SpriteGuiTexture spriteTexture,ref Matrix transform,ref Color4 color,ref RectangleF sourceRect)
        {
            //If the Texture change than Create a new SpriteBatch
            //=> It means that the sprite must be sorted by Texture type !
            //=> Maybe a more optimize way will be to use a dictionnary of UISpriteBatch instead of a list, but dictionnary is not really good performance wise... to test.
            if (_currentBatch == null || spriteTexture != _currentBatch.SpriteTexture)
            {
                _currentBatch = new UISpriteBatch(spriteTexture, _quadPerBatchCount);
                SpriteBatchs.Add(_currentBatch);
            }

            _currentBatch.AddSpriteData(ref transform, ref color, ref sourceRect);
        }

        public void ClearList()
        {
            SpriteBatchs.Clear();
            _currentBatch = null;
        }

        public void Dispose()
        {
            foreach (var batch in SpriteBatchs) batch.SpriteTexture.Dispose();
        }
    }
}
