using System;
using LostIsland.Client.Components;
using Ninject;
using S33M3Engines.Cameras;
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
        private VisualVoxelModel _model;

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
            _modelEditor = _ioc.Get<ModelEditorComponent>();
            var voxelFactory = _ioc.Get<VoxelMeshFactory>();

            #region Predefined model

            var model = new VoxelModel();

            model.ColorMapping = new ColorMapping { BlockColors = new Color4[256] };

            model.ColorMapping.BlockColors[0] = new Color4(1, 1, 1, 1);
            model.ColorMapping.BlockColors[1] = new Color4(1, 0, 0, 1);
            model.ColorMapping.BlockColors[2] = new Color4(0, 1, 0, 1);
            model.ColorMapping.BlockColors[3] = new Color4(0, 0, 1, 1);

            var frameSize = new Vector3I(16, 16, 16);

            model.Parts.Add(new VoxelModelPart { Name = "body" });
            model.Parts[0].Frames.Add(new VoxelFrame(frameSize ));
            var rnd = new Random();

            for (int y = 0; y < frameSize.Y; y++)
            {
                for (int x = y; x < frameSize.X; x++)
                {
                    for (int z = y; z < frameSize.Z; z++)
                    {
                        model.Parts[0].Frames[0].BlockData.SetBlock(new Vector3I(x, y, z), 1); //(byte) rnd.Next(1, 4)
                    }
                }
            }

            model.Parts.Add(new VoxelModelPart { Name = "part2" });
            model.Parts[1].Frames.Add(new VoxelFrame(frameSize));

            for (int y = 0; y < frameSize.Y; y++)
            {
                for (int x = 0; x < frameSize.X; x++)
                {
                    for (int z = 0; z < frameSize.Z; z++)
                    {
                        model.Parts[1].Frames[0].BlockData.SetBlock(new Vector3I(x, y, z), (byte)rnd.Next(1, 5)); //(byte) rnd.Next(1, 4)
                    }
                }
            }

            model.States.Add(new VoxelModelState 
            { 
                PartsStates = new[] 
                { 
                    new VoxelModelPartState { Transform = Matrix.Identity }, 
                    new VoxelModelPartState { Transform = Matrix.Scaling(0.2f) * Matrix.Translation(new Vector3(18, 18, 18)) }
                } 
            });


            _model = new VisualVoxelModel(model, voxelFactory);

            _model.BuildMesh();

            #endregion

            _modelEditor.VisualVoxelModel = _model;

            

            editor.BackPressed += EditorBackPressed;
            editor.ViewModePressed += EditorViewModePressed;
            editor.LayoyutModePressed += EditorLayoyutModePressed;
            editor.FrameModePressed += EditorFrameModePressed;

            AddComponent(editor);
            AddComponent(modelManager);
            AddComponent(_modelEditor);
            AddComponent(gui);
        }

        public override void OnEnabled(GameState previousState)
        {
            var editor = _ioc.Get<EditorComponent>();
            editor.UpdateNavigation(_model, 0, 0, 0);
            base.OnEnabled(previousState);
        }

        void EditorFrameModePressed(object sender, EventArgs e)
        {
            _modelEditor.Mode = EditorMode.FrameEdit;
        }

        void EditorLayoyutModePressed(object sender, EventArgs e)
        {
            _modelEditor.Mode = EditorMode.ModelLayout;
        }

        void EditorViewModePressed(object sender, EventArgs e)
        {
            _modelEditor.Mode = EditorMode.ModelView;
        }

        void EditorBackPressed(object sender, EventArgs e)
        {
            StatesManager.SetGameState("MainMenu");
        }
    }
}
