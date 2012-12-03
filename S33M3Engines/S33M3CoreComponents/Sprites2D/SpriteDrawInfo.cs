using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Buffers;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using S33M3Resources.Structs;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine.Effects.HLSLFramework;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Sprites2D
{
    /// <summary>
    /// Class that will hold the collection of sprites sharings the same characteristics : They can be draw at the same time (1 draw call)
    /// </summary>
    public class SpriteDrawInfo
    {
        #region Private variables
        #endregion

        #region Public variables
        public readonly SpriteTexture Texture;
        public readonly SamplerState TextureSampler;
        public readonly List<VertexSprite2> Vertices;
        public readonly List<ushort> Indices;
        #endregion

        public SpriteDrawInfo(SpriteTexture texture, SamplerState textureSampler)
        {
            Texture = texture;
            TextureSampler = textureSampler;
            Vertices = new List<VertexSprite2>();
            Indices = new List<ushort>();
        }

        #region Public methods
        public void AddWrappingSprite(ref Vector2 position, ref Vector2 size, Vector2 textureSize, int textureArrayIndex, ref ByteColor color, float depth)
        {
            ushort indiceVertexOffset = (ushort)Vertices.Count;

            Vector2 wrappingSize = new Vector2(size.X / textureSize.X, size.Y / textureSize.Y);

            //Create the vertices
            Vertices.Add(new VertexSprite2(new Vector3(position.X, position.Y, depth), new Vector3(0.0f, 0.0f, textureArrayIndex), color));
            Vertices.Add(new VertexSprite2(new Vector3(position.X + size.X, position.Y, depth), new Vector3(wrappingSize.X, 0.0f, textureArrayIndex), color));
            Vertices.Add(new VertexSprite2(new Vector3(position.X + size.X, position.Y + size.Y, depth), new Vector3(wrappingSize.X, wrappingSize.Y, textureArrayIndex), color));
            Vertices.Add(new VertexSprite2(new Vector3(position.X, position.Y + size.Y, depth), new Vector3(0.0f, wrappingSize.Y, textureArrayIndex), color));

            //Create the indices
            Indices.Add((ushort)(0 + indiceVertexOffset));
            Indices.Add((ushort)(1 + indiceVertexOffset));
            Indices.Add((ushort)(2 + indiceVertexOffset));
            Indices.Add((ushort)(3 + indiceVertexOffset));
            Indices.Add((ushort)(0 + indiceVertexOffset));
            Indices.Add((ushort)(2 + indiceVertexOffset));
        }

        public void AddSprite(ref Vector2 position, ref Vector2 size, int textureArrayIndex, ref ByteColor color, float depth)
        {
            ushort indiceVertexOffset = (ushort)Vertices.Count;

            //Create the vertices
            Vertices.Add(new VertexSprite2(new Vector3(position.X, position.Y, depth), new Vector3(0.0f, 0.0f, textureArrayIndex), color));
            Vertices.Add(new VertexSprite2(new Vector3(position.X + size.X, position.Y, depth), new Vector3(1.0f, 0.0f, textureArrayIndex), color));
            Vertices.Add(new VertexSprite2(new Vector3(position.X + size.X, position.Y + size.Y, depth), new Vector3(1.0f, 1.0f, textureArrayIndex), color));
            Vertices.Add(new VertexSprite2(new Vector3(position.X, position.Y + size.Y, depth), new Vector3(0.0f, 1.0f, textureArrayIndex), color));

            //Create the indices
            Indices.Add((ushort)(0 + indiceVertexOffset));
            Indices.Add((ushort)(1 + indiceVertexOffset));
            Indices.Add((ushort)(2 + indiceVertexOffset));
            Indices.Add((ushort)(3 + indiceVertexOffset));
            Indices.Add((ushort)(0 + indiceVertexOffset));
            Indices.Add((ushort)(2 + indiceVertexOffset));
        }

        /// <summary>
        /// Add a spritefor drawing
        /// </summary>
        /// <param name="position">The sprite location in screen coordinate</param>
        /// <param name="sourceRect">The texture sources, will also be used as Sprite target size (= scaling of 1)</param>
        /// <param name="textureArrayIndex"></param>
        /// <param name="color"></param>
        /// <param name="depth"></param>
        public void AddSprite(ref Vector2 position, ref RectangleF sourceRect, bool sourceRectInTextCoord, int textureArrayIndex, ref ByteColor color, float depth)
        {
            ushort indiceVertexOffset = (ushort)Vertices.Count;

            RectangleF sourceRectInTexCoord;
            if (sourceRectInTextCoord)
            {
                sourceRectInTexCoord = new RectangleF(sourceRect.Left / (float)Texture.Width, sourceRect.Top / (float)Texture.Height, sourceRect.Right / (float)Texture.Width, sourceRect.Bottom / (float)Texture.Height);
            }
            else
            {
                sourceRectInTexCoord = new RectangleF(sourceRect.Left, sourceRect.Top, sourceRect.Right, sourceRect.Bottom);
            }

            //Create the vertices
            Vertices.Add(new VertexSprite2(new Vector3(position.X, position.Y, depth),
                                           new Vector3(sourceRectInTexCoord.Left, sourceRectInTexCoord.Top, textureArrayIndex), color));

            Vertices.Add(new VertexSprite2(new Vector3(position.X + sourceRect.Width, position.Y, depth),
                                           new Vector3(sourceRectInTexCoord.Left + sourceRectInTexCoord.Width, sourceRectInTexCoord.Top, textureArrayIndex), color));

            Vertices.Add(new VertexSprite2(new Vector3(position.X + sourceRect.Width, position.Y + sourceRect.Height, depth),
                                           new Vector3(sourceRectInTexCoord.Left + sourceRectInTexCoord.Width, sourceRectInTexCoord.Top + sourceRectInTexCoord.Height, textureArrayIndex), color));

            Vertices.Add(new VertexSprite2(new Vector3(position.X, position.Y + sourceRect.Height, depth),
                                           new Vector3(sourceRectInTexCoord.Left, sourceRectInTexCoord.Top + sourceRectInTexCoord.Height, textureArrayIndex), color));

            //Create the indices
            Indices.Add((ushort)(0 + indiceVertexOffset));
            Indices.Add((ushort)(1 + indiceVertexOffset));
            Indices.Add((ushort)(2 + indiceVertexOffset));
            Indices.Add((ushort)(3 + indiceVertexOffset));
            Indices.Add((ushort)(0 + indiceVertexOffset));
            Indices.Add((ushort)(2 + indiceVertexOffset));
        }

        public void AddSprite(ref Vector2 position, ref Vector2 size, ref RectangleF sourceRect, bool sourceRectInTextCoord, int textureArrayIndex, ref ByteColor color, float depth)
        {
            ushort indiceVertexOffset = (ushort)Vertices.Count;

            RectangleF sourceRectInTexCoord;
            if (sourceRectInTextCoord)
            {
                sourceRectInTexCoord = new RectangleF(sourceRect.Left / (float)Texture.Width, sourceRect.Top / (float)Texture.Height, sourceRect.Right / (float)Texture.Width, sourceRect.Bottom / (float)Texture.Height);
            }
            else
            {
                sourceRectInTexCoord = new RectangleF(sourceRect.Left, sourceRect.Top, sourceRect.Right, sourceRect.Bottom);
            }

            //Create the vertices
            Vertices.Add(new VertexSprite2(new Vector3(position.X, position.Y, depth),
                                           new Vector3(sourceRectInTexCoord.Left, sourceRectInTexCoord.Top, textureArrayIndex), color));

            Vertices.Add(new VertexSprite2(new Vector3(position.X + size.X, position.Y, depth),
                                           new Vector3(sourceRectInTexCoord.Left + sourceRectInTexCoord.Width, sourceRectInTexCoord.Top, textureArrayIndex), color));

            Vertices.Add(new VertexSprite2(new Vector3(position.X + size.X, position.Y + size.Y, depth),
                                           new Vector3(sourceRectInTexCoord.Left + sourceRectInTexCoord.Width, sourceRectInTexCoord.Top + sourceRectInTexCoord.Height, textureArrayIndex), color));

            Vertices.Add(new VertexSprite2(new Vector3(position.X, position.Y + size.Y, depth),
                                           new Vector3(sourceRectInTexCoord.Left, sourceRectInTexCoord.Top + sourceRectInTexCoord.Height, textureArrayIndex), color));

            //Create the indices
            Indices.Add((ushort)(0 + indiceVertexOffset));
            Indices.Add((ushort)(1 + indiceVertexOffset));
            Indices.Add((ushort)(2 + indiceVertexOffset));
            Indices.Add((ushort)(3 + indiceVertexOffset));
            Indices.Add((ushort)(0 + indiceVertexOffset));
            Indices.Add((ushort)(2 + indiceVertexOffset));
        }

        public override int GetHashCode()
        {
            return Texture.GetHashCode() ^ TextureSampler.GetHashCode();
        }

        public static int ComputeHashCode(SpriteTexture texture, SamplerState sampler, int groupId)
        {
            return texture.GetHashCode() ^ sampler.GetHashCode() ^ groupId.GetHashCode();
        }
        #endregion

        #region Private methods
        #endregion
    }
}
