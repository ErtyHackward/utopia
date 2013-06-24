using System;
using Ninject;
using Realms.Client.Components.GUI.Settings;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using S33M3Resources.Structs.Vertex;
using Utopia.Entities.Managers;
using Utopia.GUI;
using Utopia.GUI.Inventory;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Settings;

namespace Realms.Client.Components.GUI
{
    public class RealmsHud : Hud
    {
        private readonly MainScreen _screen;
        private readonly D3DEngine _d3DEngine;
        private readonly GodEntityManager _godEntityManager;
        private HLSLVoxelModel _voxelEffect;

        [Inject]
        public SettingsComponent SettignsComponent
        {
            set
            {
                value.KeyBindingChanged += delegate { UpdateLabels(); };
            }
        }

        public RealmsHud(MainScreen screen, 
                         D3DEngine d3DEngine, 
                         ToolBarUi toolbar, 
                         InputsManager inputManager, 
                         CameraManager<ICameraFocused> camManager,
                         GodEntityManager godEntityManager) : 
            base(screen, d3DEngine, toolbar, inputManager, camManager)
        {
            _screen = screen;
            _d3DEngine = d3DEngine;
            _godEntityManager = godEntityManager;

            _d3DEngine.ViewPort_Updated += UpdateLayout;
        }

        private void UpdateLabels()
        {

        }

        public override void Initialize()
        {



            UpdateLabels();

            UpdateLayout(_d3DEngine.ViewPort, _d3DEngine.BackBufferTex.Description);
            
            base.Initialize();
        }

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            var sbToolbar = ToolbarUi as SandboxToolBar;

            if (sbToolbar != null)
            {
                _voxelEffect = ToDispose(new HLSLVoxelModel(context.Device, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration));
                sbToolbar.VoxelEffect = _voxelEffect;

                sbToolbar.EntitySelected += SbToolbarOnEntitySelected;
            }
            
            base.LoadContent(context);
        }

        private void SbToolbarOnEntitySelected(object sender, ToolBarEventArgs e)
        {
            _godEntityManager.GodEntity.DesignationBlueprintId = e.Entity == null ? (ushort)0 : e.Entity.BluePrintId;
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            var sbToolbar = ToolbarUi as SandboxToolBar;

            if (sbToolbar != null)
            {
                sbToolbar.Update();
            }

            base.VTSUpdate(interpolationHd, interpolationLd, elapsedTime);
        }

        public override void EnableComponent(bool forced)
        {


            base.EnableComponent(forced);
        }

        public override void DisableComponent()
        {


            base.DisableComponent();
        }

        void UpdateLayout(SharpDX.ViewportF viewport, SharpDX.Direct3D11.Texture2DDescription newBackBuffer)
        {

        }
    }
}
