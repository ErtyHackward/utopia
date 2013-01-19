using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using S33M3CoreComponents.GUI;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3DXEngine.Main;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Settings;

namespace Utopia.GUI.Crafting
{
    public class CraftingComponent : GameComponent
    {
        private HLSLVoxelModel _voxelEffect;
        private float _modelRotation = 0f;

        [Inject]
        public CraftingWindow CraftingWindow { get; set; }
        
        [Inject]
        public GuiManager GuiManager { get; set; }


        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            _voxelEffect = ToDispose(new HLSLVoxelModel(context.Device, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration));

            CraftingWindow.VoxelEffect = _voxelEffect;

            base.LoadContent(context);
        }

        public void ShowCrafting()
        {
            var desktop = GuiManager.Screen.Desktop;

            CraftingWindow.LayoutFlags = ControlLayoutFlags.Center;
            desktop.Children.Add(CraftingWindow);

            desktop.UpdateLayout();
        }

        public void HideCrafting()
        {
            GuiManager.Screen.Desktop.Children.Remove(CraftingWindow);
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, long elapsedTime)
        {
            _modelRotation += elapsedTime * 0.001f;

            if (_modelRotation > Math.PI*2)
                _modelRotation -= (float)Math.PI*2;

            CraftingWindow.ModelControl.Rotation = Quaternion.RotationYawPitchRoll(_modelRotation, 0, 0);

        }

    }
}
