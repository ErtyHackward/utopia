using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Direct3D;

namespace S33M3CoreComponents.Textures
{
    public static class TextureCreator
    {
        /// <summary>
        /// Will generate a texture of size 1 x 1 pixel, composed of the color passed in.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="context"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static ShaderResourceView GenerateColoredTexture(Device device, DeviceContext context, ByteColor color, bool asTexture2DArray = false)
        {
            Texture2DDescription desc = new Texture2DDescription()
            {
                Width = 1,
                Height = 1,
                MipLevels = 1,
                ArraySize = 1,
                Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1,0),
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.Write
            };

            Texture2D texture = new Texture2D(device, desc);
            DataBox d = context.MapSubresource(texture, 0, MapMode.WriteDiscard, MapFlags.None);

            Utilities.Write<ByteColor>(d.DataPointer, ref color);

            context.UnmapSubresource(texture, 0);

            ShaderResourceViewDescription viewDesc = new ShaderResourceViewDescription()
            {
                Format = texture.Description.Format
            };

            if (asTexture2DArray)
            {
                viewDesc.Dimension = ShaderResourceViewDimension.Texture2DArray;
                viewDesc.Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource()
                {
                    MostDetailedMip = 0,
                    MipLevels = texture.Description.MipLevels,
                    FirstArraySlice = 0,
                    ArraySize = 1
                };
            }
            else
            {
                viewDesc.Dimension = ShaderResourceViewDimension.Texture2D;
                viewDesc.Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                {
                    MipLevels = texture.Description.MipLevels,
                };
            }

            ShaderResourceView textureView = new ShaderResourceView(device, texture, viewDesc);

            texture.Dispose();

            return textureView;

        }
    }
}
