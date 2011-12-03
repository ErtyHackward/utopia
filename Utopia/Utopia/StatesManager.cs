using System; 
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using S33M3Engines.D3D;
using S33M3Engines.Threading;
using Utopia.Components;

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
        private GameState _nextState;

        /// <summary>
        /// Gets or sets current game state
        /// </summary>
        public GameState CurrentState
        {
            get { return _currentState; }
            set {
                if (_currentState != value)
                {
                    if (SwitchComponent == null || !_game.IsStarted)
                    {
                        UpdateComponent(value);
                    }
                    else
                    {
                        _nextState = value;
                        SwitchComponent.BeginSwitch();
                        SwitchComponent.Enabled = true;
                        SwitchComponent.Visible = true;
                    }

                }
            }
        }

        private void UpdateComponent(GameState newState)
        {
            if (_currentState != null)
                _currentState.OnDisabled(newState);

            var prev = _currentState;

            _currentState = newState;

            if (_currentState != null && _game.IsStarted && !IsReady(_currentState))
                throw new InvalidOperationException("Stage is not ready to be activated");


            InitComponents(prev, _currentState);

            if (_currentState != null)
                _currentState.OnEnabled(prev);
        }

        private ISwitchComponent _switchComponent;

        /// <summary>
        /// Gets or sets a component used when current state changes, maybe null
        /// </summary>
        public ISwitchComponent SwitchComponent
        {
            get { return _switchComponent; }
            set
            {
                if (_switchComponent != value)
                {
                    if (_switchComponent != null)
                    {
                        _switchComponent.SwitchMoment -= SwitchComponentSwitchMoment;
                        _switchComponent.EffectComplete -= SwitchComponentEffectComplete;
                        _game.GameComponents.Remove((GameComponent)_switchComponent);
                    }

                    _switchComponent = value;

                    if (_switchComponent != null)
                    {
                        _switchComponent.SwitchMoment += SwitchComponentSwitchMoment;
                        _switchComponent.EffectComplete += SwitchComponentEffectComplete;
                        _game.GameComponents.Add((GameComponent)_switchComponent);
                    }
                }
            }
        }

        void SwitchComponentEffectComplete(object sender, EventArgs e)
        {
            SwitchComponent.Enabled = false;
            SwitchComponent.Visible = false;
        }

        void SwitchComponentSwitchMoment(object sender, EventArgs e)
        {
            UpdateComponent(_nextState);
            _nextState = null;
            SwitchComponent.FinishSwitch();
        }

        public StatesManager(Game game)
            : this(game, null)
        {

        }

        public StatesManager(Game game, ISwitchComponent switchComponent)
        {
            if (game == null) throw new ArgumentNullException("game");
            _game = game;
            SwitchComponent = switchComponent;
        }

        private void InitComponents(GameState previous, GameState current)
        {
            // enable current stuff
            current.EnabledComponents.ForEach(c => { if (c != SwitchComponent) c.Enabled = true; });
            current.VisibleComponents.ForEach(c => { if (c != SwitchComponent) c.Visible = true; });

            if (previous == null)
                return;

            // disable previous
            previous.EnabledComponents.ForEach(c => { if (!current.EnabledComponents.Contains(c) && c != SwitchComponent) c.Enabled = false; });
            previous.VisibleComponents.ForEach(c => { if (!current.VisibleComponents.Contains(c) && c != SwitchComponent) c.Visible = false; });
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
            
            WorkQueue.ThreadPool.QueueWorkItem(delegate
            {
                state.Initialize();

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
                state.IsInitialized = true;
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

            while (!state.IsInitialized || _game.IsStarted && !IsReady(state))
                Thread.Sleep(0);
        }
        
        /// <summary>
        /// Check whether this stage can be activated
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool IsReady(GameState state)
        {
            return state.IsInitialized && state.EnabledComponents.All(c => c.IsInitialized) && state.VisibleComponents.All(c => c.IsInitialized);
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
