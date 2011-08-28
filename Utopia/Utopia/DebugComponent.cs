using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using S33M3Engines;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.GameStates;
using S33M3Engines.InputHandler;
using Utopia.GameStates;
using Utopia.Settings;
using Screen = Nuclex.UserInterface.Screen;
using Utopia.GUI.D3D.DebugUI;

namespace Utopia
{
    public class DebugComponent : GameComponent
    {
        private readonly InputHandlerManager _inputHandler;
        private readonly D3DEngine _d3DEngine;
        private readonly UtopiaRender _game;
        private readonly GameStatesManager _gameStateManagers;
        private readonly DebugInfo _debugInfo;
        private readonly Screen _screen;
        public DebugComponent(DebugInfo debugInfo, GameStatesManager gameStateManagers, UtopiaRender game, D3DEngine d3DEngine, InputHandlerManager inputHandler, Screen screen)
        {

            _debugInfo = debugInfo;
            _inputHandler = inputHandler;
            _d3DEngine = d3DEngine;
            _game = game;
            _gameStateManagers = gameStateManagers;
            _screen = screen;
        }


        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(ref GameTime timeSpent)
        {
            KeyboardStateHandling();
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            KeyboardStateHandling();
        }

        private void KeyboardStateHandling()
        {
            if (_inputHandler.PrevKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) &&
                _inputHandler.PrevKeyboardState.IsKeyDown(Keys.LControlKey) &&
                _inputHandler.CurKeyboardState.IsKeyUp(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) &&
                _inputHandler.CurKeyboardState.IsKeyDown(Keys.LControlKey))
            {
                _game.FixedTimeSteps = !_game.FixedTimeSteps;
                GameConsole.Write("FixeTimeStep Mode : " + _game.FixedTimeSteps.ToString());
            }

            if (_inputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.FullScreen))
                _d3DEngine.isFullScreen = !_d3DEngine.isFullScreen; //Go full screen !

            if (_inputHandler.PrevKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) &&
                !_inputHandler.PrevKeyboardState.IsKeyDown(Keys.LControlKey) &&
                _inputHandler.CurKeyboardState.IsKeyUp(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) &&
                !_inputHandler.CurKeyboardState.IsKeyDown(Keys.LControlKey))
            {
                _gameStateManagers.DebugActif = !_gameStateManagers.DebugActif;
                if (!_game.DebugActif)
                {
                    _gameStateManagers.DebugDisplay = 0;
                }
            }
            if (_inputHandler.IsKeyPressed(Keys.Up))
            {
                if (!_gameStateManagers.DebugActif) return;
                _gameStateManagers.DebugDisplay++;
                if (_gameStateManagers.DebugDisplay > 2) _gameStateManagers.DebugDisplay = 2;
            }
            if (_inputHandler.IsKeyPressed(Keys.Down))
            {
                if (!_gameStateManagers.DebugActif) return;
                _gameStateManagers.DebugDisplay--;
                if (_gameStateManagers.DebugDisplay < 0) _gameStateManagers.DebugDisplay = 0;
            }

            if (_inputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.LockMouseCursor))
            {
                _d3DEngine.UnlockedMouse = !_d3DEngine.UnlockedMouse;
            }

            if (_inputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.VSync))
            {
                _game.VSync = !_game.VSync;
            }

            if (_inputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.DebugInfo))
            {
                _debugInfo.Activated = !_debugInfo.Activated;
            }

            if (_inputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.Console))
            {    
                GameConsole.Show = !GameConsole.Show;
                if (GameConsole.Show){
                 //I want a fresh dynamically built UI  each time i press the key, and I dont care for garbage in debug components !
                    _screen.Desktop.Children.Add(new DebugUi(_game.GameComponents) );
                }
                else
                {
                //TODO remove the debugui from screen on second console key press
                }
            }
        

        //Exit application
            if (_inputHandler.IsKeyPressed(Keys.Escape)) _game.Exit();

        }
    }
}