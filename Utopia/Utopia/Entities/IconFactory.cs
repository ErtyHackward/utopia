using System;
using S33M3Engines;
using S33M3Engines.D3D;
using S33M3Engines.Shared.Sprites;
using S33M3Engines.Textures;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;

namespace Utopia.Entities
{
    public class IconFactory : GameComponent
    {
        public ShaderResourceView CubesTexture { get; private set; }
        private readonly D3DEngine _d3DEngine;

        public IconFactory(D3DEngine d3DEngine)
        {
            _d3DEngine = d3DEngine;
        }

        public const int IconSize = 48;

        public override void Dispose()
        {
            CubesTexture.Dispose();
        }

        public override void LoadContent()
        {
            ShaderResourceView cubeTextureView;
            //TODO this code is at multiple places, could be only handled here, texturefactory instead of IconFactory ? 
            ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, @"Textures/Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer", out cubeTextureView);
            CubesTexture = cubeTextureView;
        }

        public SpriteTexture Lookup(IItem item)
        {
            //TODO pooling in  a dictionary<entityId,Texture>, but don't forget to unpool entities that become unused !

            if (item is BlockAdder)
            {
                BlockAdder blockAdder = item as BlockAdder;
                SpriteTexture texture = new SpriteTexture(IconSize, IconSize, CubesTexture, Vector2.Zero);
                texture.Index = blockAdder.CubeId;
                return texture;
            }
            else if (item is SpriteItem)
            {
                //TODO spriteItem icon (shouldnt be difficult ;)
            }
            else if (item is VoxelItem)
            {
                VoxelItem voxelItem = item as VoxelItem;

                //2 options : 
                // option 1 : draw voxelModel in a render target texture (reuse/pool while unchanged)
                // option 2 :  cpu projection of voxels into a dynamic Texture (making a for loop on blocks, creating a sort of heigtmap in a bitmap)
            }
            return null;
        }
    }
}