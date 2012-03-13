using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using S33M3Resources.Struct.Vertex;
using SharpDX;
using S33M3Resources.Structs;

namespace S33M3CoreComponents.Sprites
{
    /// <summary>
    /// Provides sprites grouping by texture
    /// </summary>
    public class SpriteBuffer : IEnumerable<DrawInfo>
    {
        private readonly Dictionary<int, DrawInfo> _drawBuffer = new Dictionary<int, DrawInfo>();

        private int _drawItemsCount;

        /// <summary>
        /// Gets total number of draw requests
        /// </summary>
        public int TotalItems
        {
            get { return _drawItemsCount; }
        }

        /// <summary>
        /// Gets number of draw calls need to be done to draw all items
        /// </summary>
        public int DrawCalls
        {
            get { return _drawBuffer.Count; }
        }

        public void Clear()
        {
            _drawBuffer.Clear();
            _drawItemsCount = 0;
        }

        public void Add(SpriteTexture spriteTexture, VertexSpriteInstanced[] items)
        {
            if (spriteTexture == null) throw new ArgumentNullException("spriteTexture");
            if (items == null) throw new ArgumentNullException("items");
            if (items.Length == 0) throw new ArgumentException("items");

            DrawInfo info;
            var key = spriteTexture.GetHashCode();
            if (_drawBuffer.TryGetValue(key, out info))
            {
                if (info.IsGroup)
                {
                    var array = info.Group;
                    var oldLength = array.Length;
                    Array.Resize(ref array, oldLength + items.Length);
                    Array.Copy(items, 0, array, oldLength, items.Length);
                    info.Group = array;
                }
                else throw new NotImplementedException();
            }
            else
            {
                _drawBuffer.Add(key, new DrawInfo { Group = items, SpriteTexture = spriteTexture });
            }
            _drawItemsCount++;
        }

        public void Add(SpriteTexture spriteTexture, ref Matrix transform, Color4 color, RectangleF sourceRect = default(RectangleF), bool sourceRectInTextCoord = true, int textureArrayIndex = 0, float depth = 0)
        {
            DrawInfo info;
            var key = spriteTexture.GetHashCode();
            if (_drawBuffer.TryGetValue(key, out info))
            {
                // already have calls of this texture, group it
                if (info.IsGroup)
                {
                    // just add new group item
                    var oldArray = info.Group;
                    Array.Resize(ref oldArray, oldArray.Length + 1);
                    oldArray[oldArray.Length - 1] = new VertexSpriteInstanced
                    {
                        Tranform = transform,
                        SourceRect = sourceRectInTextCoord ? sourceRect : new RectangleF(0, 0, 1, 1),
                        Color = new ByteColor(color),
                        TextureArrayIndex = textureArrayIndex,
                        Depth = depth
                    };
                    info.Group = oldArray;
                }
                else
                {
                    // create new group
                    var array = new VertexSpriteInstanced[2];

                    array[0] = new VertexSpriteInstanced
                    {
                        Tranform = info.Transform,
                        SourceRect = info.SourceRect,
                        Color = new ByteColor(info.Color4),
                        TextureArrayIndex = info.TextureArrayIndex,
                        Depth = info.Depth
                    };

                    array[1] = new VertexSpriteInstanced
                    {
                        Tranform = transform,
                        SourceRect = sourceRectInTextCoord ? sourceRect : new RectangleF(0, 0, 1, 1),
                        Color = new ByteColor(color),
                        TextureArrayIndex = textureArrayIndex,
                        Depth = depth
                    };
                    info.Group = array;
                }
            }
            else
            {
                // create single draw order
                _drawBuffer.Add(key, new DrawInfo
                {
                    SpriteTexture = spriteTexture,
                    Transform = transform,
                    Color4 = color,
                    SourceRect = sourceRectInTextCoord ? sourceRect : new RectangleF(0, 0, 1, 1),
                    Depth = depth,
                    TextureArrayIndex = textureArrayIndex
                });
            }

            _drawItemsCount++;
        }

        public IEnumerator<DrawInfo> GetEnumerator()
        {
            return _drawBuffer.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
