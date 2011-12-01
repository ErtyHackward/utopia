using System; 
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using S33M3Engines.D3D;

namespace Utopia
{
    /// <summary>
    /// Allows to control the game states
    /// </summary>
    public class StatesManager
    {
        private readonly Game _game;
        private readonly List<GameState> _states = new List<GameState>();

        private GameState _currentState;

        /// <summary>
        /// Gets or sets current game state
        /// </summary>
        public GameState CurrentState
        {
            get { return _currentState; }
            set {
                if (_currentState != value)
                {
                    if (_currentState != null)
                        _currentState.OnDisabled(value);
                    
                    var prev = _currentState;

                    _currentState = value;

                    if (_currentState != null && _game.IsStarted && !IsReady(_currentState))
                        throw new InvalidOperationException("Stage is not ready to be activated");
                    
                    InitComponents(prev, _currentState);

                    if (_currentState != null)
                        _currentState.OnEnabled(prev);
                }
            }
        }

        public StatesManager(Game game)
        {
            if (game == null) throw new ArgumentNullException("game");
            _game = game;
        }

        private void InitComponents(GameState previous, GameState current)
        {
            // enable current stuff
            current.EnabledComponents.ForEach(c => c.Enabled = true);
            current.VisibleComponents.ForEach(c => c.Visible = true);

            if (previous == null)
                return;

            // disable previous
            previous.EnabledComponents.ForEach(c => { if (!current.EnabledComponents.Contains(c)) c.Enabled = false; });
            previous.VisibleComponents.ForEach(c => { if (!current.VisibleComponents.Contains(c)) c.Visible = false; });

        }
        
        /// <summary>
        /// Changes current game state
        /// </summary>
        /// <param name="state"></param>
        public void SetGameState(GameState state)
        {
            CurrentState = state;
        }

        /// <summary>
        /// Changes current game state by its name
        /// </summary>
        /// <param name="name"></param>
        public void SetGameState(string name)
        {
            PrepareState(name);
            CurrentState = GetByName(name);
        }

        /// <summary>
        /// Prepares stage to be able to activate
        /// </summary>
        /// <param name="state"></param>
        public void PrepareStateAsync(GameState state)
        {
            if (IsReady(state)) return;

            state.EnabledComponents.ForEach(c =>
            {
                if (!_game.GameComponents.Contains(c))
                {
                    _game.GameComponents.Add(c);
                }
            });

            state.VisibleComponents.ForEach(c =>
            {
                if (!_game.GameComponents.Contains(c))
                {
                    _game.GameComponents.Add(c);
                }
            });
        }

        public GameState GetByName(string stateName)
        {
            if (stateName == null) throw new ArgumentNullException("stateName");
            var state = _states.Find(gs => gs.Name == stateName);
            if (state == null)
                throw new InvalidOperationException("No such state");

            return state;
        }

        public void PrepareStateAsync(string stateName)
        {
            PrepareStateAsync(GetByName(stateName));
        }

        public void PrepareState(string stateName)
        {
            var state = GetByName(stateName);

            PrepareStateAsync(state);

            while (_game.IsStarted && !IsReady(state))
                Thread.Sleep(0);
        }
        
        /// <summary>
        /// Check whether this stage can be activated
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool IsReady(GameState state)
        {
            return state.EnabledComponents.All(c => c.IsInitialized) && state.VisibleComponents.All(c => c.IsInitialized);
        }

        /// <summary>
        /// Allows to change state by its name
        /// </summary>
        /// <param name="state"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RegisterState(GameState state)
        {
            if (state == null) throw new ArgumentNullException("state");
            if (_states.Exists(gs => gs.Name == state.Name))
                throw new InvalidOperationException("State with such name is already added");

            _states.Add(state);
            state.StatesManager = this;
        }
    }
}
