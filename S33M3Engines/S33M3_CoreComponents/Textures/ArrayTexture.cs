using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using System.IO;
using SharpDX;
using SharpDX.Direct3D;

namespace S33M3_DXEngine.Textures
{
    public static class ArrayTexture
    {
        /// <summary>
        /// Create A shaderViewresource on a TextureArray object created from image files
        /// </summary>
        /// <param name="device">The graphic device</param>
        /// <param name="directories">where to look</param>
        /// <param name="fileNames">The files names that will be used to create the array, this filename can use * or ?, ... the file name will be sorted by name for the array index</param>
        /// <param name="miPfilterFlag">Filter used to create the mipmap lvl from the loaded images</param>
        /// <param name="textureArrayView">The created textureArray view that can directly be used inside shaders</param>       
        public static void CreateTexture2DFromFiles(Device device, DeviceContext context, string[] directories, string fileNames, FilterFlags miPfilterFlag, string ResourceName, out ShaderResourceView textureArrayView)
        {
            List<string> fileCollection = new List<string>();
            foreach (var dir in directories)
            {
                DirectoryInfo dinfo = new DirectoryInfo(dir);

                foreach (FileInfo fi in dinfo.GetFiles(fileNames).OrderBy(x => x.Name))
                {
                    fileCollection.Add(dir + fi.Name);
                }

            }
            CreateTexture2DFromFiles(device, context, fileCollection.ToArray(), miPfilterFlag, ResourceName, out textureArrayView);
        }

        public static void CreateTexture2DFromFiles(Device device, string[] directories, string fileNames, FilterFlags miPfilterFlag, string ResourceName, out Texture2D[] textureArrayView, int MaxMipLevels = 0)
        {
            List<string> fileCollection = new List<string>();
            foreach (var dir in directories)
            {
                DirectoryInfo dinfo = new DirectoryInfo(dir);

                foreach (FileInfo fi in dinfo.GetFiles(fileNames).OrderBy(x => x.Name))
                {
                    fileCollection.Add(dir + fi.Name);
                }

            }
            CreateTexture2DFromFiles(device, fileCollection.ToArray(), miPfilterFlag, ResourceName, out textureArrayView, MaxMipLevels);
        }

        public static void CreateTexture2DFromFiles(Device device,DeviceContext context , string directory, string fileNames, FilterFlags miPfilterFlag, string ResourceName, out ShaderResourceView textureArrayView)
        {
            CreateTexture2DFromFiles(device,context, new string[] { directory }, fileNames, miPfilterFlag, ResourceName, out textureArrayView);
        }

        public static void CreateTexture2DFromFiles(Device device, string directory, string fileNames, FilterFlags miPfilterFlag, string ResourceName, out Texture2D[] textureArray, int MaxMipLevels = 0)
        {
            CreateTexture2DFromFiles(device, new string[] { directory }, fileNames, miPfilterFlag, ResourceName, out textureArray, MaxMipLevels);
        }

        public static void CreateTexture2D(Device device ,DeviceContext context, Texture2D[] texturesCollection, FilterFlags MIPfilterFlag, string ResourceName, out ShaderResourceView TextureArrayView)
        {

            //Create TextureArray object
            Texture2DDescription Imagesdesc = texturesCollection[0].Description;
            Texture2DDescription texArrayDesc = new Texture2DDescription()
            {
                Width = Imagesdesc.Width,
                Height = Imagesdesc.Height,
                MipLevels = Imagesdesc.MipLevels,
                ArraySize = texturesCollection.Length,
                Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                SampleDescription = new SharpDX.DXGI.SampleDescription() { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            Texture2D texArray = new Texture2D(device, texArrayDesc);
            DataBox box;
            //Foreach Texture
            //Foreach Texture
            for (int arraySlice = 0; arraySlice < texturesCollection.Length; arraySlice++)
            {
                //Foreach mipmap level
                for (int mipSlice = 0; mipSlice < Imagesdesc.MipLevels; mipSlice++)
                {
                    box = context.MapSubresource(texturesCollection[arraySlice], mipSlice, MapMode.Read, MapFlags.None);

                    context.UpdateSubresource(box,
                                                texArray,
                                                ArrayTextureHelper.CalcSubresource(mipSlice, arraySlice, Imagesdesc.MipLevels)
                                                );

                    context.UnmapSubresource(texturesCollection[arraySlice], mipSlice);
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
                    ArraySize = texturesCollection.Length
                }
            };

            TextureArrayView = new ShaderResourceView(device, texArray, viewDesc);

            //Set resource Name, will only be done at debug time.
#if DEBUG
            TextureArrayView.DebugName = ResourceName;
#endif

            //Disposing resources used to create the texture array
            texArray.Dispose();
        }

        /// <summary>
        /// Create A shaderViewresource on a TextureArray object created from image files
        /// </summary>
        /// <param name="device">The graphic device</param>
        /// <param name="FileNames">The files names that will be used to create the array, the array's index will be based on the order of the file inside this collection</param>
        /// <param name="MIPfilterFlag">Filter used to create the mipmap lvl from the loaded images</param>
        /// <param name="ArrayTextureView">The create textureArray view that can directly be used inside shaders</param>
        public static void CreateTexture2DFromFiles(Device device, DeviceContext context, string[] FileNames, FilterFlags MIPfilterFlag, string ResourceName, out ShaderResourceView TextureArrayView)
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

            CreateTexture2D(device,context, srcTex, MIPfilterFlag, ResourceName, out TextureArrayView);

            //Disposing resources used to create the texture array
            foreach (Texture2D tex in srcTex) tex.Dispose();
        }

        /// <summary>
        /// Create A shaderViewresource on a TextureArray object created from image files
        /// </summary>
        /// <param name="device">The graphic device</param>
        /// <param name="FileNames">The files names that will be used to create the array, the array's index will be based on the order of the file inside this collection</param>
        /// <param name="MIPfilterFlag">Filter used to create the mipmap lvl from the loaded images</param>
        /// <param name="ArrayTextureView">The create textureArray view that can directly be used inside shaders</param>
        public static void CreateTexture2DFromFiles(Device device, string[] FileNames, FilterFlags MIPfilterFlag, string ResourceName, out Texture2D[] TextureArray, int MaxMipLevels)
        {
            int inputImagesCount = FileNames.Length;

            //1 First loading the textures from files
            TextureArray = new Texture2D[inputImagesCount];

            ImageLoadInformation ImageInfo = new ImageLoadInformation()
            {
                FirstMipLevel = 0,
                MipLevels = MaxMipLevels,
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
                TextureArray[imgInd] = Texture2D.FromFile<Texture2D>(device, FileNames[imgInd], ImageInfo);
            }

        }

    }
}
