using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SharpDX.Direct3D11;
using Utopia.Entities;
using Utopia.Entities.Voxel;
using Utopia.Shared.Entities;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Tools;

namespace Utopia.Editor
{
    static class Program
    {
        public static ModelsRepository ModelsRepository { get; set; }

        public static Dictionary<string, Image> ModelIcons { get; set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ModelIcons = new Dictionary<string, Image>();

            ModelsRepository = new ModelsRepository();
            ModelsRepository.Load();

            ModelSelector.Models = ModelsRepository.ModelsFiles;

            #region Generate voxel models icons
            using (var engine = new S33M3DXEngine.D3DEngine(new Size(1024, 768), "Test", new SharpDX.DXGI.SampleDescription { Count = 1, Quality = 0 }, new Size(0, 0)))
            {
                DXStates.CreateStates(engine);
                var modelsStorage = new FileModelStorage(@"C:\Dev\Utopia\Utopia\Realms\Realms.Client\bin\Release\Models");
                var voxelMeshFactory = new VoxelMeshFactory(engine);
                var modelManager = new VoxelModelManager(modelsStorage, null, voxelMeshFactory);
                modelManager.Initialize();
                var iconFactory = new IconFactory(engine, modelManager);
                iconFactory.LoadContent(engine.ImmediateContext);

                foreach (var visualVoxelModel in modelManager.Enumerate())
                {
                    using (var dxIcon = iconFactory.CreateVoxelIcon(visualVoxelModel, new SharpDX.DrawingSize(42, 42)))
                    {
                        var memStr = new MemoryStream();
                        Resource.ToStream(engine.ImmediateContext, dxIcon, ImageFileFormat.Png, memStr);
                        memStr.Position = 0;
                        ModelIcons.Add(visualVoxelModel.VoxelModel.Name, new Bitmap(memStr));
                    }
                }

                modelManager.Dispose();
                iconFactory.Dispose();
            }

            #endregion


            Application.Run(new FrmMain());
        }
    }
}
