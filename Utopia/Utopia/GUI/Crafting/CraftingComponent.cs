using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using S33M3CoreComponents.GUI;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.Sound;
using S33M3DXEngine.Main;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Entities;
using Utopia.Shared.Settings;

namespace Utopia.GUI.Crafting
{
    public class CraftingComponent : GameComponent
    {
        private HLSLVoxelModel _voxelEffect;

        [Inject]
        public CraftingWindow CraftingWindow { get; set; }
        
        [Inject]
        public GuiManager GuiManager { get; set; }

        [Inject]
        public ISoundEngine SoundEngine { get; set; }


        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            _voxelEffect = ToDispose(new HLSLVoxelModel(context.Device, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration));

            CraftingWindow.VoxelEffect = _voxelEffect;
            CraftingWindow.CraftButton.Pressed += CraftButtonOnPressed;

            base.LoadContent(context);
        }

        private void CraftButtonOnPressed(object sender, EventArgs eventArgs)
        {
            if (CraftingWindow.CanCraft)
            {
                var recipe = (Recipe)CraftingWindow.RecipesList.SelectedItem;
                var recipeIndex = CraftingWindow.Player.EntityFactory.Config.Recipes.IndexOf(recipe);
                 
                CraftingWindow.Player.CraftUse(recipeIndex);
                CraftingWindow.Update();
            }
        }

        public void ShowCrafting()
        {
            var desktop = GuiManager.Screen.Desktop;

            if (CraftingWindow.RecipesList.Items.Count > 0 && CraftingWindow.RecipesList.SelectedItems.Count == 0)
            {
                CraftingWindow.RecipesList.SelectItem(0);
            }

            CraftingWindow.LayoutFlags = ControlLayoutFlags.Center;
            desktop.Children.Add(CraftingWindow);
            CraftingWindow.Update();

            desktop.UpdateLayout();
        }

        public void HideCrafting()
        {
            GuiManager.Screen.Desktop.Children.Remove(CraftingWindow);
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            if (CraftingWindow.ModelControl.ManualRotation)
                return;

            CraftingWindow.ModelControl.Rotation *= Quaternion.RotationYawPitchRoll(elapsedTime, 0, 0);

        }

    }
}
