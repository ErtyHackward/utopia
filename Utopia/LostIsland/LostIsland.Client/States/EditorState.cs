using System;
using LostIsland.Client.Components;
using Ninject;
using Utopia;
using Utopia.Components;
using Utopia.Entities.Voxel;
using Utopia.GUI.D3D;

namespace LostIsland.Client.States
{
    /// <summary>
    /// Voxel model editor state
    /// </summary>
    public class EditorState : GameState
    {
        private readonly IKernel _ioc;

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
            var editor = _ioc.Get<EditorComponent>();
            var modelManager = _ioc.Get<VoxelModelManager>();
            var axis = _ioc.Get<EditorAxis>();

            editor.BackPressed += EditorBackPressed;

            AddComponent(gui);
            AddComponent(editor);
            AddComponent(modelManager);
            AddComponent(axis);
        }

        void EditorBackPressed(object sender, EventArgs e)
        {
            StatesManager.SetGameState("MainMenu");
        }
    }
}
