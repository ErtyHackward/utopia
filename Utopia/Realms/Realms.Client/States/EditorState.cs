using System;
using Ninject;
using Ninject.Parameters;
using Realms.Client.Components.GUI;
using S33M3CoreComponents.Cameras.Interfaces;
using Utopia.Components;
using Utopia.Entities;
using Utopia.Entities.Voxel;
using S33M3CoreComponents.States;
using S33M3CoreComponents.GUI;
using Utopia.Particules;

namespace Realms.Client.States
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

        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            var bg = _ioc.Get<BlackBgComponent>();
            var gui = _ioc.Get<GuiManager>();
            var modelManager = _ioc.Get<VoxelModelManager>();
            _modelEditor = _ioc.Get<ModelEditorComponent>();
            var iconFactory = _ioc.Get<IconFactory>();

            _modelEditor.BackPressed += EditorBackPressed;

            AddComponent(bg);
            AddComponent(modelManager);
            AddComponent(_modelEditor);
            AddComponent(gui);
            AddComponent(iconFactory);

            var vars = _ioc.Get<RealmRuntimeVariables>();
            _modelEditor.Author = vars.DisplayName;

            base.Initialize(context);
        }

        void EditorBackPressed(object sender, EventArgs e)
        {
            StatesManager.ActivateGameStateAsync("MainMenu");
        }

        public override void OnDisabled(GameState nextState)
        {
            //Dispose all components related to the Game scope
            GameScope.CurrentGameScope.Dispose();
            //Create a new Scope
            GameScope.CreateNewScope();

            base.OnDisabled(nextState);
        }
    }
}
