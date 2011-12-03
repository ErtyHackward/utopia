using Nuclex.UserInterface;
using S33M3Engines;
using S33M3Engines.D3D;
using S33M3Engines.GameStates;
using Utopia.Action;
using Utopia.Entities.Managers;
using Utopia.GUI.D3D.DebugUI;

namespace Utopia.Components
{
    public class DebugComponent : GameComponent
    {
        private readonly ActionsManager _actions;
        private readonly PlayerEntityManager _playerMgr;
        private readonly D3DEngine _d3DEngine;
        private readonly UtopiaRender _game;
        private readonly Screen _screen;
        private readonly GameStatesManager _gameStateManagers;

        private DebugUi _debugUi;

        public DebugComponent(UtopiaRender game, D3DEngine d3DEngine, Screen screen, GameStatesManager gameStateManagers, ActionsManager actions, PlayerEntityManager playerMgr )
        {
            _d3DEngine = d3DEngine;
            _game = game;
            _screen = screen;
            _gameStateManagers = gameStateManagers;
            _actions = actions;
            _playerMgr = playerMgr;
        }


        public override void Update(ref GameTime timeSpend)
        {
            KeyboardStateHandling();
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
        }

        private void KeyboardStateHandling()
        {
            //if (_inputHandler.PrevKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) &&
            //    _inputHandler.PrevKeyboardState.IsKeyDown(Keys.LControlKey) &&
            //    _inputHandler.CurKeyboardState.IsKeyUp(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) &&
            //    _inputHandler.CurKeyboardState.IsKeyDown(Keys.LControlKey))
            //{
            //    _game.FixedTimeSteps = !_game.FixedTimeSteps;
            //    GameConsole.Write("FixeTimeStep Mode : " + _game.FixedTimeSteps.ToString());
            //}

            //if (_inputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.FullScreen))
            //    _d3DEngine.isFullScreen = !_d3DEngine.isFullScreen; //Go full screen !

            //if (_inputHandler.PrevKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) &&
            //    !_inputHandler.PrevKeyboardState.IsKeyDown(Keys.LControlKey) &&
            //    _inputHandler.CurKeyboardState.IsKeyUp(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) &&
            //    !_inputHandler.CurKeyboardState.IsKeyDown(Keys.LControlKey))
            //{
            //    _gameStateManagers.DebugActif = !_gameStateManagers.DebugActif;
            //    if (!_game.DebugActif)
            //    {
            //        _gameStateManagers.DebugDisplay = 0;
            //    }
            //}
            //if (_inputHandler.IsKeyPressed(Keys.Up))
            //{
            //    if (!_gameStateManagers.DebugActif) return;
            //    _gameStateManagers.DebugDisplay++;
            //    if (_gameStateManagers.DebugDisplay > 2) _gameStateManagers.DebugDisplay = 2;
            //}
            //if (_inputHandler.IsKeyPressed(Keys.Down))
            //{
            //    if (!_gameStateManagers.DebugActif) return;
            //    _gameStateManagers.DebugDisplay--;
            //    if (_gameStateManagers.DebugDisplay < 0) _gameStateManagers.DebugDisplay = 0;
            //}

            if (_actions.isTriggered(Actions.Engine_VSync))
            {
                _game.VSync = !_game.VSync;
            }

            //if (_inputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.DebugInfo))
            //{
            //    _debugInfo.Activated = !_debugInfo.Activated;
            //}

            //if (_inputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.Console))
            //{
            //    GameConsole.Show = !GameConsole.Show;
            //}

            if (_actions.isTriggered(Actions.Engine_ShowDebugUI))
            {
                if (_screen.Desktop.Children.Contains(_debugUi))
                {
                    _screen.Desktop.Children.Remove(_debugUi);
                    _playerMgr.HasMouseFocus = true;
                }
                else {

                    //each time i press the key, I want a fresh dynamically built UI, in synch with components, and I dont care for garbage in debug components !
                    _debugUi = new DebugUi(_game, _game.GameComponents, _d3DEngine, _gameStateManagers);
                    //made this without ninject to keep it explicit, but debugui could be injected into debugcomponent !
                    _screen.Desktop.Children.Add(_debugUi);
                    _playerMgr.HasMouseFocus = false;
                }
            }

        }
    }
}