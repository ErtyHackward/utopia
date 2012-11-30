using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Sprites._3D
{
    public interface ISprite3DBuffer
    {
        void Init(DeviceContext context, ResourceUsage usage);
        void Begin();

        void SetData(DeviceContext context);
        void Set2DeviceAndDraw(DeviceContext context);

        void Draw(ref Vector3 worldPosition, ref Vector2 size, ref ByteColor color, Sprite3DRenderer.SpriteRenderingType spriterenderingType, int textureArrayIndex);
        void Draw(ref Vector3 worldPosition, ref Vector2 size, ref ByteColor color, Sprite3DRenderer.SpriteRenderingType spriterenderingType, ref Vector4 textCoordU, ref Vector4 textCoordV, int textureArrayIndex );
        void DrawText(string text, SpriteFont spriteFont, SpriteTexture texture, ref Vector3 worldPosition, float scaling, ref ByteColor color, ICamera camera, int textureArrayIndex, bool XCenteredText, bool MultiLineHandling);
    }
}
