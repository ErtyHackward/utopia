using System;
using Ninject;
using Sandbox.Client.Components;
using Utopia;
using Utopia.Components;
using Utopia.Entities.Voxel;
using S33M3_CoreComponents.States;
using S33M3_CoreComponents.GUI;

namespace Sandbox.Client.States
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

        public EditorState(GameStatesManager stateManager, IKernel iocContainer)
            :base(stateManager)
        {
            _ioc = iocContainer;
        }

        public override void Initialize()
        {
            var bg = _ioc.Get<BlackBgComponent>();
            var gui = _ioc.Get<GuiManager>();
            var modelManager = _ioc.Get<VoxelModelManager>();
            _modelEditor = _ioc.Get<ModelEditorComponent>();
            
            _modelEditor.BackPressed += EditorBackPressed;

            AddComponent(bg);
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
