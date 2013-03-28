using System;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.GUI.Nuclex;
using Utopia.Shared.World;
using S33M3CoreComponents.GUI;

namespace Realms.Client.Components.GUI.SinglePlayer
{
    public partial class SinglePlayerComponent : MenuTemplate1Component
    {
        #region Private variables
        private WorldParameters _currentWorldParameter;
        private RealmRuntimeVariables _vars;
        private readonly D3DEngine _engine;
        private GuiManager _guiManager;

        private SpriteTexture _stNewGameLabel;
        private SpriteTexture _stSavedGamesLabel;
        private SpriteTexture _stLoadLabel;
        private SpriteTexture _stDeleteLabel;
        private SpriteTexture _stCreateLabel;

        #endregion

        #region Events
        public event EventHandler StartingGameRequested;
        private void OnStartingGameRequested()
        {
            if (StartingGameRequested != null) 
                StartingGameRequested(this, EventArgs.Empty);
        }

        #endregion

        public SinglePlayerComponent(Game game, D3DEngine engine, MainScreen screen, SandboxCommonResources commonResources, WorldParameters currentWorldParameter, RealmRuntimeVariables var, GuiManager guiManager)
            : base(game, engine, screen, commonResources)
        {
            _engine = engine;
            _guiManager = guiManager;
            _vars = var;
            _currentWorldParameter = currentWorldParameter;
        }

        public override void Initialize()
        {
            _stNewGameLabel     = ToDispose(SandboxCommonResources.LoadTexture(_engine, "Images\\newgame_label.png"));
            _stSavedGamesLabel  = ToDispose(SandboxCommonResources.LoadTexture(_engine, "Images\\saved_games_label.png"));
            _stLoadLabel        = ToDispose(SandboxCommonResources.LoadTexture(_engine, "Images\\load_label.png"));
            _stDeleteLabel      = ToDispose(SandboxCommonResources.LoadTexture(_engine, "Images\\delete_label.png"));
            _stCreateLabel      = ToDispose(SandboxCommonResources.LoadTexture(_engine, "Images\\create_label.png"));

            base.Initialize();
        }

        public override void FTSUpdate(GameTime timeSpent)
        {
            if (_savedGamePanel.NeedShowResults)
                _savedGamePanel.ShowResults();

            base.FTSUpdate(timeSpent);
        }

    }
}
