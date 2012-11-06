using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using Utopia.Entities;
using Utopia.Entities.Voxel;
using Utopia.Shared.Configuration;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;

namespace Utopia.Editor
{
    public class IconHelper
    {
        public Dictionary<string, Image> GenerateIcons(WorldConfiguration configuration)
        {
            var result = new Dictionary<string, Image>();
            using (var engine = new S33M3DXEngine.D3DEngine())
            {
                ClientSettings.PathRoot = @"E:\Dev\Utopia\Utopia\Realms\Realms.Client\bin\Release\";
                ClientSettings.EffectPack = @"E:\Dev\Utopia\Utopia\Realms\Realms.Client\bin\Release\EffectsPacks\Default\";
                ClientSettings.TexturePack = @"E:\Dev\Utopia\Utopia\Realms\Realms.Client\bin\Release\TexturesPacks\Default\";
                DXStates.CreateStates(engine);
                var modelsStorage = new FileModelStorage(@"E:\Dev\Utopia\Utopia\Realms\Realms.Client\bin\Release\Models");
                var voxelMeshFactory = new VoxelMeshFactory(engine);
                var modelManager = new VoxelModelManager(modelsStorage, null, voxelMeshFactory);
                modelManager.Initialize();
                var iconFactory = new IconFactory(engine, modelManager, configuration);
                iconFactory.LoadContent(engine.ImmediateContext);

                foreach (var visualVoxelModel in modelManager.Enumerate())
                {
                    using (var dxIcon = iconFactory.CreateVoxelIcon(visualVoxelModel, new SharpDX.DrawingSize(42, 42)))
                    {
                        var memStr = new MemoryStream();
                        Resource.ToStream(engine.ImmediateContext, dxIcon, ImageFileFormat.Png, memStr);
                        memStr.Position = 0;
                        result.Add(visualVoxelModel.VoxelModel.Name, new Bitmap(memStr));
                    }
                }

                modelManager.Dispose();
                iconFactory.Dispose();
            }
            return result;
        }

    }
}
