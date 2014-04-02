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
using Utopia.Entities.Managers;
using Utopia.GUI.Crafting;
using Utopia.Network;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Configuration;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Settings;

namespace Utopia.GUI.CharacterSelection
{
    public class CharacterSelectionComponent : GameComponent
    {
        private HLSLVoxelModel _voxelEffect;

        [Inject]
        public CharacterSelectionWindow SelectionWindow { get; set; }

        [Inject]
        public GuiManager GuiManager { get; set; }

        [Inject]
        public ServerComponent ServerComponent { get; set; }

        [Inject]
        public PlayerEntityManager PlayerEntityManager { get; set; }   

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            _voxelEffect = ToDispose(new HLSLVoxelModel(context.Device, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration));

            SelectionWindow.VoxelEffect = _voxelEffect;
            SelectionWindow.SelectionButton.Pressed += SelectionButtonOnPressed;

            base.LoadContent(context);
        }

        private void SelectionButtonOnPressed(object sender, EventArgs eventArgs)
        {
            // inform server about model change
            ServerComponent.ServerConnection.Send(new EntityVoxelModelMessage { 
                EntityLink = PlayerEntityManager.PlayerCharacter.GetLink(),
                ClassName = ((CharacterClassItem)SelectionWindow.ClassList.SelectedItem).ClassName
            });
        }
        
        public void ShowSelection()
        {
            var desktop = GuiManager.Screen.Desktop;

            if (SelectionWindow.ClassList.Items.Count > 0 && SelectionWindow.ClassList.SelectedItems.Count == 0)
            {
                SelectionWindow.ClassList.SelectItem(0);
            }

            SelectionWindow.LayoutFlags = ControlLayoutFlags.Center;
            desktop.Children.Add(SelectionWindow);
            SelectionWindow.Update();

            desktop.UpdateLayout();
        }

        public void HideSelection()
        {
            GuiManager.Screen.Desktop.Children.Remove(SelectionWindow);
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            if (SelectionWindow.CharacterModel.ManualRotation)
                return;

            SelectionWindow.CharacterModel.Rotation *= Quaternion.RotationYawPitchRoll(elapsedTime, 0, 0);

        }
    }
}
