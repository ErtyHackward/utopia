﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Sprites;
using SharpDX;
using S33M3Resources.Structs;
using S33M3DXEngine.Effects.HLSLFramework;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Sprites
{
    /// <summary>
    /// The aim of this class is to bufferize the incoming sprite draw request, and sort them by Texture type
    /// </summary>
    public class SpriteDrawBuffer
    {
        #region Private variables
        private Dictionary<int, SpriteDrawInfo> _spritesByTexture;
        private bool _enableDepthSprite;
        #endregion

        #region Public variables
        public float AutoDepth;
        #endregion

        public SpriteDrawBuffer()
        {
            _spritesByTexture = new Dictionary<int, SpriteDrawInfo>();
        }

        #region Public method
        public void Reset(bool enableDepthSprite)
        {
            AutoDepth = 0.99999f;
            Restart(enableDepthSprite);
        }

        public void Restart(bool enableDepthSprite)
        {
            _enableDepthSprite = enableDepthSprite;
            _spritesByTexture.Clear();
        }

        public void AddSprite(SpriteTexture texture, SamplerState sampler, ref Vector2 position, ref Vector2 size, int textureArrayIndex, ref ByteColor color, int drawGroupId)
        {
            AddSprite(texture, sampler, ref position, ref size, textureArrayIndex, ref color, AutoDepth, drawGroupId);
            if (_enableDepthSprite) AutoDepth -= 0.0001f;
        }

        public void AddSprite(SpriteTexture texture, SamplerState sampler, ref Vector2 position, ref Vector2 size, int textureArrayIndex, ref ByteColor color, float spriteDepth, int drawGroupId)
        {
            GetSpriteDrawInfo(texture, sampler, drawGroupId).AddSprite(ref position, ref size, textureArrayIndex, ref color, spriteDepth);
        }

        public void AddWrappingSprite(SpriteTexture texture, SamplerState sampler, ref Vector2 position, ref Vector2 size, int textureArrayIndex, ref ByteColor color, int drawGroupId)
        {
            GetSpriteDrawInfo(texture, sampler, drawGroupId).AddWrappingSprite(ref position, ref size, new Vector2(texture.Width, texture.Height), textureArrayIndex, ref color, AutoDepth);
            if (_enableDepthSprite) AutoDepth -= 0.0001f;
        }

        public void AddWrappingSprite(SpriteTexture texture, SamplerState sampler, ref Vector2 position, ref Vector2 size, int textureArrayIndex, ref ByteColor color, float spriteDepth, int drawGroupId)
        {
            GetSpriteDrawInfo(texture, sampler, drawGroupId).AddWrappingSprite(ref position, ref size, new Vector2(texture.Width, texture.Height), textureArrayIndex, ref color, spriteDepth);
        }

        public void AddSprite(SpriteTexture texture, SamplerState sampler, ref Vector2 position, ref RectangleF sourceRect, bool sourceRectInTextCoord, int textureArrayIndex, ref ByteColor color, int drawGroupId)
        {
            AddSprite(texture, sampler, ref position, ref sourceRect, sourceRectInTextCoord, textureArrayIndex, ref color, AutoDepth, drawGroupId);
            if (_enableDepthSprite) AutoDepth -= 0.0001f;
        }

        public void AddSprite(SpriteTexture texture, SamplerState sampler, ref Vector2 position, ref RectangleF sourceRect, bool sourceRectInTextCoord, int textureArrayIndex, ref ByteColor color, float spriteDepth, int drawGroupId)
        {
            GetSpriteDrawInfo(texture, sampler, drawGroupId).AddSprite(ref position, ref sourceRect, sourceRectInTextCoord, textureArrayIndex, ref color, spriteDepth);
        }

        public void AddSprite(SpriteTexture texture, SamplerState sampler, ref Vector2 position, ref Vector2 size, ref RectangleF sourceRect, bool sourceRectInTextCoord, int textureArrayIndex, ref ByteColor color, int drawGroupId)
        {
            AddSprite(texture, sampler, ref position, ref size, ref sourceRect, sourceRectInTextCoord, textureArrayIndex, ref color, AutoDepth, drawGroupId);
            if (_enableDepthSprite) AutoDepth -= 0.0001f;
        }

        public void AddSprite(SpriteTexture texture, SamplerState sampler, ref Vector2 position, ref Vector2 size, ref RectangleF sourceRect, bool sourceRectInTextCoord, int textureArrayIndex, ref ByteColor color, float spriteDepth, int drawGroupId)
        {
            GetSpriteDrawInfo(texture, sampler, drawGroupId).AddSprite(ref position, ref size, ref sourceRect, sourceRectInTextCoord, textureArrayIndex, ref color, spriteDepth);
        }

        /// <summary>
        /// Get the Buffer group where the sprite will be added, these compononents will be draw at the same time
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="sampler"></param>
        /// <returns></returns>
        public SpriteDrawInfo GetSpriteDrawInfo(SpriteTexture texture, SamplerState sampler, int drawGroupId)
        {
            SpriteDrawInfo spriteDrawInfo;
            int textureHashCode = SpriteDrawInfo.ComputeHashCode(texture, sampler, drawGroupId);
            if (_spritesByTexture.TryGetValue(textureHashCode, out spriteDrawInfo) == false)
            {
                //The sprite group for this texture is not existing => Create it !
                spriteDrawInfo = new SpriteDrawInfo(texture, sampler);
                _spritesByTexture.Add(textureHashCode, spriteDrawInfo);
            }

            return spriteDrawInfo;
        }

        public IEnumerable<SpriteDrawInfo> GetAllSpriteGroups()
        {
            return _spritesByTexture.Values;
        }
        #endregion
    }
}