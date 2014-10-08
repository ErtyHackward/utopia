using S33M3CoreComponents.Maths;
using S33M3DXEngine;
using S33M3DXEngine.Main;
using S33M3DXEngine.Textures;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Utopia.Shared.Settings;
using Utopia.Shared.Structs;
using Utopia.Shared.World;

namespace Utopia.Shared.GraphicManagers
{
    /// <summary>
    /// Class that will be responsible to load and create the various textures used by the cubes for rendering
    /// </summary>
    public class CubeTexturesManager : BaseComponent
    {
        #region Private fields
        private D3DEngine _engine;
        #endregion

        #region public properties
        public ShaderResourceView CubeArrayTexture;
        public Dictionary<string, CubeTextureInfo> CubeTexturesMeta;
        
        /// <summary>
        /// Give the LCM from all animated texture frame count
        /// </summary>
        public int TexturesAnimationLCM { get; set; }
        #endregion

        public CubeTexturesManager(D3DEngine engine)
        {
            _engine = engine;
        }

        #region Public methods

        private Size<int> GetPNGSize(string filePath)
        {
            var buff = new byte[32];
            using (var d = File.OpenRead(filePath))
            {
                d.Read(buff, 0, 32);
            }
            const int wOff = 16;
            const int hOff = 20;
            Size<int> imageSize = new Size<int>();
            imageSize.Width = BitConverter.ToInt32(new[] { buff[wOff + 3], buff[wOff + 2], buff[wOff + 1], buff[wOff + 0], }, 0);
            imageSize.Height = BitConverter.ToInt32(new[] { buff[hOff + 3], buff[hOff + 2], buff[hOff + 1], buff[hOff + 0], }, 0);
            return imageSize;
        }

        public void Initialization(DeviceContext context, FilterFlags MIPFilterFlags)
        {
            CreateTexturesMetaData();
            CreateTextureResources(context, MIPFilterFlags);
        }
        #endregion

        #region Private methods

        private void CreateTexturesMetaData()
        {
            List<int> lcm = new List<int>();
            CubeTexturesMeta = new Dictionary<string, CubeTextureInfo>();
            int currentId = 0;
            //Check all existing Textures, and assign them ids that will be use in texture array
            foreach (var file in Directory.GetFiles(ClientSettings.TexturePack + @"Terran/", @"*.png").OrderBy(x => x))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                var size = GetPNGSize(file);

                CubeTextureInfo textureMeta = new CubeTextureInfo() { Size = size, NbrFrame = 1, TextureArrayId = currentId, TextureName = fileName };

                //Multi Frame texture ?
                if (size.Height != size.Width)
                {
                    //Compute the nbr of frames present in the image
                    int nbrFrames = size.Height / size.Width;
                    currentId += (nbrFrames - 1);

                    textureMeta.isAnimated = true;
                    textureMeta.NbrFrame = (byte)nbrFrames;
                    lcm.Add(nbrFrames);
                }

                CubeTexturesMeta[fileName] = textureMeta;

                currentId++;
            }

            TexturesAnimationLCM = MathHelper.LCM(lcm);
        }

        private void CreateTextureResources(DeviceContext context, FilterFlags MIPfilterFlag)
        {
            if (CubeArrayTexture != null)
            {
                RemoveToDispose(CubeArrayTexture);
                CubeArrayTexture.Dispose();
            }
            CubeArrayTexture = null;

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

            List<Texture2D> cubeTextures = new List<Texture2D>();

            //Get all file and create a list of stream !
            foreach (string file in Directory.GetFiles(ClientSettings.TexturePack + @"Terran/", @"*.png").OrderBy(x => x))
            {
                var meta = CubeTexturesMeta[Path.GetFileNameWithoutExtension(file)];

                if (meta.isAnimated)
                {
                    //Split file into small pieces and create Texture2D from it
                    Bitmap animatedTex = new Bitmap(file);
                    Bitmap frameTex = new Bitmap(meta.Size.Width, meta.Size.Width);
                    Rectangle frameRect = new Rectangle(0, 0, meta.Size.Width, meta.Size.Width);
                    using (Graphics gr = Graphics.FromImage(frameTex))
                    {
                        int rows = meta.Size.Height / meta.Size.Width;
                        Rectangle sourceRect = new Rectangle(0, 0, meta.Size.Width, meta.Size.Width);
                        for (int row = 0; row < rows; row++)
                        {
                            // Copy the frame of the image.
                            gr.DrawImage(animatedTex, frameRect, sourceRect, GraphicsUnit.Pixel);
                            //Save into a memoryStream
                            using (MemoryStream textureMemoryStream = new MemoryStream())
                            {
                                frameTex.Save(textureMemoryStream, System.Drawing.Imaging.ImageFormat.Png);
                                textureMemoryStream.Position = 0;
                                Texture2D newTexture = Texture2D.FromStream<Texture2D>(_engine.Device, textureMemoryStream, (int)textureMemoryStream.Length, ImageInfo);
                                cubeTextures.Add(newTexture);
                            }
                            sourceRect.Y += meta.Size.Width;
                        }
                    }

                    frameTex.Dispose();
                    animatedTex.Dispose();
                }
                else
                {
                    Texture2D newTexture = Texture2D.FromFile<Texture2D>(_engine.Device, file, ImageInfo);
                    cubeTextures.Add(newTexture);
                }
            }

            //2 Creation of the TextureArray resource
            ArrayTexture.CreateTexture2D(context, cubeTextures.ToArray(), "CubeTexturesArray", out CubeArrayTexture, ImageInfo.Format);

            //Disposing resources used to create the texture array
            foreach (Texture2D tex in cubeTextures) tex.Dispose();

            ToDispose(CubeArrayTexture);
        }

        #endregion

        public struct CubeTextureInfo
        {
            public Size<int> Size;
            public bool isAnimated;
            public byte NbrFrame;
            public int TextureArrayId;
            public string TextureName;
        }
    }
}
