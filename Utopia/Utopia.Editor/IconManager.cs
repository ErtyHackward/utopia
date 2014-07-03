using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using S33M3DXEngine;
using S33M3DXEngine.Textures;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Entities;
using Utopia.Entities.Voxel;
using Utopia.Shared.Configuration;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using Utopia.Shared.GraphicManagers;
using Utopia.Shared.World;

namespace Utopia.Editor
{
    /// <summary>
    /// Provides possibility to generate icons for the voxel models and cubes
    /// </summary>
    public class IconManager : IDisposable
    {
        private string _utopiaFolder = "";
        private D3DEngine _engine;
        private IconFactory _iconFactory;
        private VoxelModelManager _modelManager;
        private ShaderResourceView _cubeTextureView;
        private CubeTexturesManager _textureManager;
        private VisualWorldParameters _visualWorldParameters;

        public VoxelModelManager ModelManager
        {
            get { return _modelManager; }
            set { _modelManager = value; }
        }

        public IconFactory IconFactory
        {
            get { return _iconFactory; }
            set { _iconFactory = value; }
        }

        public Size2 IconSize { get; set; }

        public IconManager()
        {
            IconSize = new Size2(42, 42);
        }

        public void Initialize(string utopiaPath)
        {
            if (_engine == null)
            {

                _utopiaFolder = utopiaPath;
                ClientSettings.PathRoot = _utopiaFolder;
                ClientSettings.EffectPack = Path.Combine(_utopiaFolder, @"EffectsPacks\Default\");
                ClientSettings.TexturePack = Path.Combine(_utopiaFolder, @"TexturesPacks\Default\");

                _engine = new D3DEngine();
                DXStates.CreateStates(_engine);
            }

            var modelsStorage = new ModelSQLiteStorage(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Realms", "Common", "models.db"));
            var voxelMeshFactory = new VoxelMeshFactory(_engine);
            _modelManager = new VoxelModelManager();
            _modelManager.VoxelModelStorage = modelsStorage;
            _modelManager.VoxelMeshFactory = voxelMeshFactory;
            _modelManager.Initialize();

            _visualWorldParameters = new VisualWorldParameters();
            _textureManager = new CubeTexturesManager(_engine);
            _textureManager.Initialization(_engine.ImmediateContext, FilterFlags.Point);
            _cubeTextureView = _textureManager.CubeArrayTexture;

            _visualWorldParameters.CubeTextureManager = _textureManager;

            _iconFactory = new IconFactory(_engine, _modelManager, _visualWorldParameters);

            //ArrayTexture.CreateTexture2DFromFiles(_engine.Device, _engine.ImmediateContext,
            //                                        Path.Combine(ClientSettings.TexturePack, @"Terran\"), @"ct*.png",
            //                                        FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer",
            //                                        out _cubeTextureView);
        }

        public Dictionary<string, Image> GenerateIcons(WorldConfiguration configuration)
        {
            var result = new Dictionary<string, Image>();

            if (configuration.isCubesProfilesIDInitialized == false)
            {
                _visualWorldParameters.WorldParameters.Configuration = configuration;
                _visualWorldParameters.InitCubesProfiles();
            }

            _iconFactory.Configuration = configuration;
            _iconFactory.LoadContent(_engine.ImmediateContext);

            var iconsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Realms", "Common", "Icons");

            if (!Directory.Exists(iconsDir))
                Directory.CreateDirectory(iconsDir);

            //Create Items icons
            foreach (var visualVoxelModel in _modelManager.Enumerate())
            {
                foreach (var voxelModelState in visualVoxelModel.VoxelModel.States)
                {

                    var iconId = visualVoxelModel.VoxelModel.Name +
                                 (voxelModelState.IsMainState ? "" : ":" + voxelModelState.Name);

                    var fileName = iconId.Replace(':', '_') + ".png";

                    var path = Path.Combine(iconsDir, fileName);

                    if (File.Exists(path))
                    {
                        result.Add(iconId, Image.FromFile(path));
                    }
                    else
                    {
                        using (var dxIcon = _iconFactory.CreateVoxelIcon(visualVoxelModel, IconSize, voxelModelState))
                        {
                            var memStr = new MemoryStream();
                            Resource.ToStream(_engine.ImmediateContext, dxIcon, ImageFileFormat.Png, memStr);
                            memStr.Position = 0;
                            var bmp = new Bitmap(memStr);
                            result.Add(iconId, bmp);
                            memStr.Dispose();

                            bmp.Save(path, ImageFormat.Png);
                        }
                    }
                }
            }

            //Create Blocks icons
            int i = 0;
            List<Texture2D> cubeIcons = _iconFactory.Get3DBlockIcons(_engine.ImmediateContext, IconSize, _cubeTextureView);
            foreach (var cubeprofiles in configuration.GetAllCubesProfiles())
            {
                if (cubeprofiles.Id == WorldConfiguration.CubeId.Air) 
                    continue;

                var blockId = "CubeResource_" + cubeprofiles.Name;

                                    var fileName = blockId.Replace(':', '_') + ".png";

                    var path = Path.Combine(iconsDir, fileName);

                if (File.Exists(path))
                {
                    result.Add(blockId, Image.FromFile(path));
                }
                else
                {

                    var memStr = new MemoryStream();
                    Resource.ToStream(_engine.ImmediateContext, cubeIcons[i], ImageFileFormat.Png, memStr);
                    memStr.Position = 0;
                    var bmp = new Bitmap(memStr);
                    result.Add(blockId, bmp);
                    memStr.Dispose();
                    bmp.Save(path, ImageFormat.Png);
                }
                i++;
            }

            //Create texture icons
            foreach (var textureFile in Directory.GetFiles(ClientSettings.TexturePack + @"Terran\", "*.png"))
            {
                //Load Image
                Bitmap img = new Bitmap(textureFile);

                int NbrFrames = img.Height / img.Width;
                //Generate texture ICON
                Bitmap resized = Copy(img, new System.Drawing.Rectangle() { X=0, Y=0, Width = img.Width, Height = img.Width });

                result.Add("TextureCube_" + Path.GetFileNameWithoutExtension(textureFile) + "@" + NbrFrames, resized);
            }

            return result;
        }

        private Bitmap Copy(Bitmap srcBitmap, System.Drawing.Rectangle srcRegion)
        {
            // Create the new bitmap and associated graphics object
            Bitmap destBitmap = new Bitmap(32, 32);
            //Graphics g = Graphics.FromImage(bmp);

            //// Draw the specified section of the source bitmap to the new one
            //g.DrawImage(srcBitmap, 0, 0, section, GraphicsUnit.Pixel);

            //// Clean up
            //g.Dispose();

            using (Graphics grD = Graphics.FromImage(destBitmap))
            {
                grD.DrawImage(srcBitmap, srcRegion, srcRegion, GraphicsUnit.Pixel);
            }


            // Return the bitmap
            return destBitmap;
        }

        public void Dispose()
        {
            _textureManager.Dispose();
            _modelManager.Dispose();
            _iconFactory.Dispose();
        }

    }
}
