using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RectangleF = System.Drawing.RectangleF;
using SharpDX;
using S33M3Resources.Struct.Vertex;

namespace S33M3CoreComponents.Sprites
{
    /// <summary>
    /// Class for storing single and batched draw data
    /// </summary>
    public class DrawInfo
    {
        public SpriteTexture SpriteTexture { get; set; }
        public RectangleF SourceRect { get; set; }
        public int TextureArrayIndex { get; set; }
        public Color4 Color4 { get; set; }
        public Matrix Transform { get; set; }

        public float Depth { get; set; }

        public bool IsGroup { get { return Group != null; } }
        /// <summary>
        /// Batched draw array
        /// </summary>
        public VertexSpriteInstanced[] Group { get; set; }
    }
}
