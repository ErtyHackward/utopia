using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using SharpDX;
using System.IO;
using SharpDX.Direct3D;

namespace S33M3Engines.Textures
{
    public static class ArrayTexture
    {
        /// <summary>
        /// Create A shaderViewresource on a TextureArray object created from image files
        /// </summary>
        /// <param name="device">The graphic device</param>
        /// <param name="FileNames">The files names that will be used to create the array, this filename can use * or ?, ... the file name will be sorted by name for the array index</param>
        /// <param name="MIPfilterFlag">Filter used to create the mipmap lvl from the loaded images</param>
        /// <param name="ArrayTextureView">The create textureArray view that can directly be used inside shaders</param>
        public static void CreateTexture2DFromFiles(Device device, string Directory, string FileNames, FilterFlags MIPfilterFlag, out ShaderResourceView TextureArrayView)
        {
            DirectoryInfo dinfo = new DirectoryInfo(Directory);
            List<string> fileCollection = new List<string>();
            foreach (FileInfo fi in dinfo.GetFiles(FileNames).OrderBy(x => x.Name))
            {
                fileCollection.Add(Directory + fi.Name);
            }

            CreateTexture2DFromFiles(device, fileCollection.ToArray(), MIPfilterFlag, out TextureArrayView);
        }

        /// <summary>
        /// Create A shaderViewresource on a TextureArray object created from image files
        /// </summary>
        /// <param name="device">The graphic device</param>
        /// <param name="FileNames">The files names that will be used to create the array, the array's index will be based on the order of the file inside this collection</param>
        /// <param name="MIPfilterFlag">Filter used to create the mipmap lvl from the loaded images</param>
        /// <param name="ArrayTextureView">The create textureArray view that can directly be used inside shaders</param>
        public static void CreateTexture2DFromFiles(Device device, string[] FileNames, FilterFlags MIPfilterFlag, out ShaderResourceView TextureArrayView)
        {
            int inputImagesCount = FileNames.Length;

            //1 First loading the textures from files
            Texture2D[] srcTex = new Texture2D[inputImagesCount];

            ImageLoadInformation ImageInfo = new ImageLoadInformation()
            {
                FirstMipLevel = 0,
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Write | CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None,
                Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                Filter = FilterFlags.None,
                MipFilter = MIPfilterFlag
            };

            for (int imgInd = 0; imgInd < inputImagesCount; imgInd++)
            {
                srcTex[imgInd] = Texture2D.FromFile<Texture2D>(device, FileNames[imgInd], ImageInfo);
            }

            //2 Creation of the TextureArray resource

            //Create TextureArray object
            Texture2DDescription Imagesdesc = srcTex[0].Description;
            Texture2DDescription texArrayDesc = new Texture2DDescription()
            {
                Width = Imagesdesc.Width,
                Height = Imagesdesc.Height,
                MipLevels = Imagesdesc.MipLevels,
                ArraySize = srcTex.Length,
                Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                SampleDescription = new SharpDX.DXGI.SampleDescription() { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            Texture2D texArray = new Texture2D(device, texArrayDesc);
            DataBox box;
            int mipHeight, lockedSize, RowPitch;
            //Foreach Texture
            RowPitch = 0;
            for (int arraySlice = 0; arraySlice < srcTex.Length; arraySlice++)
            {
                //Foreach mipmap level
                for (int mipSlice = 0; mipSlice < Imagesdesc.MipLevels; mipSlice++)
                {
                    mipHeight = ArrayTextureHelper.GetMipSize(mipSlice, srcTex[arraySlice].Description.Height);

                    ////It's a HACK version 2 ==> VERY VERY BAD ! Slower, but more safe !
                    box = device.ImmediateContext.MapSubresource(srcTex[arraySlice], mipSlice, 1, MapMode.Read, MapFlags.None);
                    RowPitch = box.RowPitch;
                    device.ImmediateContext.UnmapSubresource(srcTex[arraySlice], mipSlice);
                    //// Very very BBAaAaAADdDdd ===========================================

                    lockedSize = mipHeight * RowPitch;//RowPitch

                    box = device.ImmediateContext.MapSubresource(srcTex[arraySlice], mipSlice, lockedSize, MapMode.Read, MapFlags.None);


                    box.Data.Position = 0;
                    device.ImmediateContext.UpdateSubresource(box,
                                                         texArray,
                                                         S33M3Engines.D3D.Tools.Resource.CalcSubresource(mipSlice, arraySlice, Imagesdesc.MipLevels)
                                                         );
                    device.ImmediateContext.UnmapSubresource(srcTex[arraySlice], mipSlice);

                }
            }

            //Create Resource view to texture array
            ShaderResourceViewDescription viewDesc = new ShaderResourceViewDescription()
            {
                Format = texArrayDesc.Format,
                Dimension = ShaderResourceViewDimension.Texture2DArray,
                Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource()
                {
                    MostDetailedMip = 0,
                    MipLevels = texArrayDesc.MipLevels,
                    FirstArraySlice = 0,
                    ArraySize = srcTex.Length
                }
            };

            TextureArrayView = new ShaderResourceView(device, texArray, viewDesc);

            //Disposing resources used to create the texture array
            texArray.Dispose();
            foreach (Texture2D tex in srcTex) tex.Dispose();

        }

    }
}
