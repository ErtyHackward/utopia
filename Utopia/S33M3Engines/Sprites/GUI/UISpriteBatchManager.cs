using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using RectangleF = System.Drawing.RectangleF;

namespace S33M3Engines.Sprites.GUI
{
    /// <summary>
    /// Class responsible to manage the sprites created by CeGui, in order to pack to limit the number of draw call
    /// </summary>
    public class UISpriteBatchManager : IDisposable
    {
        #region Private variables
        private int _quadPerBatchCount;
        private UISpriteBatch _currentBatch;
        #endregion

        #region Public variables/Properties
        public List<UISpriteBatch> SpriteBatchs;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="SpriteBatchCount">Evaluated number of batch</param>
        /// <param name="QuadPerBatchCount">Evaluated number of sprite per batch</param>
        public UISpriteBatchManager(int SpriteBatchCount, int QuadPerBatchCount)
        {
            _quadPerBatchCount = QuadPerBatchCount;
            SpriteBatchs = new List<UISpriteBatch>(SpriteBatchCount);
        }


        #region Public methods
        /// <summary>
        /// Enqueue a sprite for rendering
        /// </summary>
        /// <param name="spriteTexture">Texture</param>
        /// <param name="transform">Operation that must be realize on the texture for displaying (Translation and scaling mostly)</param>
        /// <param name="color">The Color by witch each pixel of the texture will by multiplied</param>
        /// <param name="sourceRect">The subTexture rectangle to use as source</param>
        public void AddSprite(SpriteGuiTexture spriteTexture, ref Matrix transform, ref Color4 color, ref RectangleF sourceRect)
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

        /// <summary>
        /// Clear the rendering queue
        /// </summary>
        public void ClearList()
        {
            SpriteBatchs.Clear();
            _currentBatch = null;
        }

        /// <summary>
        /// Dispose DX resources binded (buffers)
        /// </summary>
        public void Dispose()
        {
            foreach (var batch in SpriteBatchs) batch.SpriteTexture.Dispose();
        }
        #endregion

        #region Inner Class
        /// <summary>
        /// This Inner class will represent a Batch of sprites, each sprite stored inside will be rendered at once using only one draw call
        /// Instancing method used.
        /// </summary>
        public class UISpriteBatch
        {
            #region Public variables
            //Texture use inside this batch
            public SpriteGuiTexture SpriteTexture;
            //Sprites container
            public List<VertexSpriteInstanced> SpriteData;
            #endregion

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="spriteTexture">Texture use inside this batch</param>
            /// <param name="quadPerBatchCount">Initial size of the Sprite container</param>
            public UISpriteBatch(SpriteGuiTexture spriteTexture, int quadPerBatchCount = 1000)
            {
                SpriteData = new List<VertexSpriteInstanced>(quadPerBatchCount);
                SpriteTexture = spriteTexture;
            }

            #region Public methods
            /// <summary>
            /// Add a new sprite to the container
            /// </summary>
            /// <param name="transform">Sprite transformation matrix (Mostly scaling with translation</param>
            /// <param name="color">The Color by witch each pixel of the texture will by multiplied</param>
            /// <param name="sourceRect">The subTexture rectangle to use as source</param>
            public void AddSpriteData(ref Matrix transform, ref Color4 color, ref RectangleF sourceRect)
            {
                SpriteData.Add(new VertexSpriteInstanced() { Color = color, SourceRect = sourceRect, Tranform = transform });
            }
            #endregion
        }
        #endregion

    }
}
