using System; 
using System.Collections.Generic;

namespace Utopia
{
    /// <summary>
    /// Allows to control the game states
    /// </summary>
    public class StatesManager
    {
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

                    InitComponents(prev, _currentState);

                    if (_currentState != null)
                        _currentState.OnEnabled(prev);
                }
            }
        }

        private void InitComponents(GameState previous, GameState current)
        {
            if (previous == null)
            {
                // just enable and show all current components
                current.EnabledComponents.ForEach(c => c.Enabled = true);
                current.VisibleComponents.ForEach(c => c.Visible = true);
                return;
            }

            // enable current stuff
            current.EnabledComponents.ForEach(c => c.Enabled = true);
            current.VisibleComponents.ForEach(c => c.Visible = true);

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
            if (name == null) throw new ArgumentNullException("name");
            var state = _states.Find(gs => gs.Name == name);
            if (state == null)
                throw new InvalidOperationException("No such state");

            CurrentState = state;
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
        }
    }
}
