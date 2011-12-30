using System;
using Ninject;
using SharpDX;
using Utopia;
using Utopia.Components;
using Utopia.Entities.Voxel;
using Utopia.GUI.D3D;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Structs;

namespace LostIsland.Client.States
{
    /// <summary>
    /// Voxel model editor state
    /// </summary>
    public class EditorState : GameState
    {
        private readonly IKernel _ioc;
        private ModelEditorComponent _modelEditor;

        /// <summary>
        /// Name of the state
        /// </summary>
        public override string Name
        {
            get { return "Editor"; }
        }

        public EditorState(IKernel iocContainer)
        {
            _ioc = iocContainer;
        }

        public override void Initialize()
        {
            var gui = _ioc.Get<GuiManager>();
            var modelManager = _ioc.Get<VoxelModelManager>();
            _modelEditor = _ioc.Get<ModelEditorComponent>();
            
            _modelEditor.BackPressed += EditorBackPressed;

            AddComponent(modelManager);
            AddComponent(_modelEditor);
            AddComponent(gui);
        }

        void EditorBackPressed(object sender, EventArgs e)
        {
            StatesManager.SetGameState("MainMenu");
        }
    }
}
