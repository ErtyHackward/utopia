using System;
using System.Collections.Generic;
using System.Drawing;
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

        public VoxelModelManager ModelManager
        {
            get { return _modelManager; }
            set { _modelManager = value; }
        }

        public DrawingSize IconSize { get; set; }

        public IconManager()
        {
            IconSize = new DrawingSize(42, 42);
        }

        public void Initialize(string utopiaPath)
        {
            if (_engine != null)
                throw new ApplicationException("The IconManager is already initialized");

            _utopiaFolder = utopiaPath;
            ClientSettings.PathRoot = _utopiaFolder;
            ClientSettings.EffectPack = Path.Combine(_utopiaFolder, @"EffectsPacks\Default\");
            ClientSettings.TexturePack = Path.Combine(_utopiaFolder, @"TexturesPacks\Default\");

            _engine = new D3DEngine();
            DXStates.CreateStates(_engine);
            var modelsStorage = new FileModelStorage(Path.Combine(_utopiaFolder, "Models"));
            var voxelMeshFactory = new VoxelMeshFactory(_engine);
            _modelManager = new VoxelModelManager(modelsStorage, null, voxelMeshFactory);
            _modelManager.Initialize();

            _iconFactory = new IconFactory(_engine, _modelManager, new Shared.World.VisualWorldParameters());

            
            ArrayTexture.CreateTexture2DFromFiles(_engine.Device, _engine.ImmediateContext,
                                                    Path.Combine(ClientSettings.TexturePack, @"Terran\"), @"ct*.png",
                                                    FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer",
                                                    out _cubeTextureView);
        }

        public Dictionary<string, Image> GenerateIcons(WorldConfiguration configuration)
        {
            var result = new Dictionary<string, Image>();

            _iconFactory.Configuration = configuration;
            _iconFactory.LoadContent(_engine.ImmediateContext);

            //Create Items icons
            foreach (var visualVoxelModel in _modelManager.Enumerate())
            {
                using (var dxIcon = _iconFactory.CreateVoxelIcon(visualVoxelModel, IconSize))
                {
                    var memStr = new MemoryStream();
                    Resource.ToStream(_engine.ImmediateContext, dxIcon, ImageFileFormat.Png, memStr);
                    memStr.Position = 0;
                    result.Add(visualVoxelModel.VoxelModel.Name, new Bitmap(memStr));
                    memStr.Dispose();
                }
            }

            //Create Blocks icons
            int i = 0;
            List<Texture2D> cubeIcons = _iconFactory.Get3DBlockIcons(_engine.ImmediateContext, IconSize, _cubeTextureView);
            foreach (var cubeprofiles in configuration.GettAllCubesProfiles())
            {
                if (cubeprofiles.Id == WorldConfiguration.CubeId.Air) continue;
                var memStr = new MemoryStream();
                Resource.ToStream(_engine.ImmediateContext, cubeIcons[i], ImageFileFormat.Png, memStr);
                memStr.Position = 0;
                result.Add("CubeResource_" + cubeprofiles.Name, new Bitmap(memStr));
                memStr.Dispose();

                i++;
            }

            return result;
        }

        public void Dispose()
        {
            _cubeTextureView.Dispose();
            _modelManager.Dispose();
            _iconFactory.Dispose();
        }

    }
}
