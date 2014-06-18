using System;
using System.Collections.Generic;
using System.Linq;
using S33M3DXEngine.Main;
using S33M3CoreComponents.States.Interfaces;
using System.Threading;
using S33M3DXEngine.Threading;
using SharpDX.Direct3D11;
using S33M3DXEngine;
using System.Threading.Tasks;

namespace S33M3CoreComponents.States
{
    public class GameStatesManager : GameComponent
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        private readonly Game _game;
        private readonly List<GameState> _gameStates = new List<GameState>();

        //Will manage, "the progression" from one given state being rendered, to another.
        private GameState _currentState;            //Current GameState being rendered
        private GameState _nextState;               //Next GameState
        private ISwitchComponent _switchComponent;  //The "Fade-out / Fade-in" component to go from the _currentState to _nextState
        private D3DEngine _engine;
        private DeviceContext _loadContext;
        private CommandList _loadContextCommandList;
        private bool _inActivationProcess;

        private List<Task<GameState>> AsyncStateInitResults = new List<Task<GameState>>();

        #endregion

        #region Public properties/variables

        /// <summary>
        /// Gets or sets current game state
        /// </summary>
        public GameState CurrentState
        {
            get { return _currentState; }
        }

        /// <summary>
        /// Don't use this unless, you know what you are doing
        /// </summary>
        /// <param name="currentState"></param>
        public void ForceCurrentState(GameState currentState)
        {
            _currentState = currentState;
        }

        public bool DeactivateSwitchComponent { get; set; }

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
                        //Switch component changed, we remove the Event handle + remove from Main loop component
                        _switchComponent.SwitchMoment -= SwitchComponentSwitchMoment;
                        _switchComponent.EffectComplete -= SwitchComponentEffectComplete;
                        _game.GameComponents.Remove((GameComponent)_switchComponent);
                    }

                    _switchComponent = value;

                    if (_switchComponent != null)
                    {
                        //Insert the switch component into the Main Loop, and sign for its events
                        _switchComponent.SwitchMoment += SwitchComponentSwitchMoment;
                        _switchComponent.EffectComplete += SwitchComponentEffectComplete;
                        _game.GameComponents.Add((GameComponent)_switchComponent);
                    }
                }
            }
        }
        #endregion

        public GameStatesManager(D3DEngine engine, Game game, int allocatedThreadPool = 3)
            : this(engine, game, null, allocatedThreadPool)
        {
        }

        public GameStatesManager(D3DEngine engine, Game game, ISwitchComponent switchComponent, int allocatedThreadPool = 3):
            base("GameStatesManager")
        {
            if (game == null)
            {
                logger.Error("game value is nullin constructor");
                throw new ArgumentNullException("Game is null");
            }

            IsSystemComponent = true;
            _engine = engine;
            _game = game;
            SwitchComponent = switchComponent;
            _loadContext = _engine.CreateDeviceContext();
#if DEBUG
            _loadContext.DebugName = "GameState Deffered Context";
#endif
            //Activate this component
            this.EnableComponent();
        }

        //Will search trhough all registered States, and remove the Disposed components inside them
        //To give the possibility to release not used resources
        public void GameStatesCleanUp()
        {
            foreach (var state in _gameStates)
            {
                //Remvoe all components that have been disposed from the States
                if (state.GameComponents.RemoveAll(x => x.IsDisposed) > 0)
                {
                    state.IsDirty = true;
                }
            }
        }



        #region Private methods

        //End of activation process where the new state is pushed for rendering
        private void SwitchActiveGameState(GameState newState)
        {
            //send the current to previous state, and set the new state to current
            GameState prev = _currentState;
            _currentState = newState;

            //Do a check to be sure that the new state is ready to be used (All components initialized)
            if (_currentState != null && _currentState.IsInitialized == false)
            {
                logger.Error("Stage is not ready to be activated");
                throw new InvalidOperationException("Stage is not ready to be activated");
            }
            
            //Deactivate the "previous" state's component, and enable the new one !
            DisablePreviousEnableCurrent(prev, _currentState);

            //Raise the GameState.OnDisabled event from the Currently running GameState
            if (prev != null) prev.OnDisabled(newState); //Disable the currently running GameState

            //Raise the GameState.OnEnabled event from the Newly running GameState
            if (_currentState != null) _currentState.OnEnabled(prev);

            //In case some Disabled components have been disposed, start a Main Rendering Collection cleanup.
            _game.GameComponents.CleanUp();

            _inActivationProcess = false;
            _currentState.IsActivationRequested = false;
            _currentState.WithPreservePreviousStates = false;

#if DEBUG
            logger.Debug("State activated : {0}", newState.Name);
#endif
        }

        /// <summary>
        /// Will make the components from the "Current" GameState ready to be rendered
        /// Will disable if needed the components from the previous gamestate.
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="current"></param>
        private void DisablePreviousEnableCurrent(GameState previous, GameState current)
        {
            if (previous != null && current.WithPreservePreviousStates == false)
            {
                // disable previous stuff that are not existing in the current gamecomponents
                foreach (GameComponent gc in previous.GameComponents.Except(current.GameComponents))
                {
                    if (gc.IsSystemComponent) continue;
                    gc.DisableComponent();
                }
            }

            // Enable current stuff, except the system Component
            foreach (GameComponent gc in current.GameComponents)
            {
                //if (gc.IsSystemComponent || gc.isEnabled == true) continue;
                if (gc.isEnabled == true) continue;
                gc.EnableComponent();
            }
        }

        private void SetCurrentState(GameState state)
        {
            //Not State switcher defined, do the activatino directly
            if (SwitchComponent == null || state == _currentState || DeactivateSwitchComponent == true)
            {
                SwitchActiveGameState(state);
            }
            else
            {
                _nextState = state;
                SwitchComponent.BeginSwitch();
                SwitchComponent.EnableComponent();
            }
        }

        //Initialize in an async mod all the components.
        //Add them in the Main Loop
        private GameState InitializeComponentsAsync(GameState state)
        {
            //Initialize the game state and all the components from the GameStates
            state.Initialize(_loadContext);

            return state;
        }

        //Switch component management
        private void SwitchComponentEffectComplete(object sender, EventArgs e)
        {
            SwitchComponent.DisableComponent();
        }

        private void SwitchComponentSwitchMoment(object sender, EventArgs e)
        {
            SwitchActiveGameState(_nextState);
            _nextState = null;
            SwitchComponent.FinishSwitch();
        }
        #endregion

        #region Public methods

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            //For each Pending Initizalion running
            for (int i = AsyncStateInitResults.Count - 1; i >= 0; i--)
            {
                var stateResult = AsyncStateInitResults[i];
                //Thread work finished ??
                if (stateResult.IsCompleted)
                {
                    GameState state = stateResult.Result;

                    //Does one component use a deffered loadcontent ?
                    int nbrDefferedContentComponent = state.GameComponents.Count(x => x.IsDefferedLoadContent);
                    if (nbrDefferedContentComponent > 0)
                    {
                        //Apply the loadContext command list on the Immediat context
                        _loadContextCommandList = _loadContext.FinishCommandList(false);

                        //The ExecuteCommandList reset all Setted states from Immediate context if set to false
                        _engine.ImmediateContext.ExecuteCommandList(_loadContextCommandList, true);

                        //Dispose the command list
                        _loadContextCommandList.Dispose();
                        _loadContextCommandList = null;
                    }

                    //Execute the not deferred loadcontent.
                    if (nbrDefferedContentComponent != state.GameComponents.Count)
                    {
                        foreach (var gc in state.GameComponents.Where(x => x.IsDefferedLoadContent == false && x.IsInitialized == false))
                        {
                            Thread.Sleep(0);
                            gc.LoadContent(_engine.ImmediateContext);
                            gc.IsInitialized = true;
                        }
                    }

                    //Add the current GameState components to the gamecomponents "initialized" collection
                    state.GameComponents.ForEach(gc =>
                    {
                        if (!_game.GameComponents.Contains(gc)) _game.GameComponents.Add(gc);
                    });

#if DEBUG
                    logger.Debug("State Initialization finished : {0}", state.Name);
#endif
                    state.RaisedInitializedEvent();
                    //The states is initialized, was it an initialization requested from an activation ?
                    if (state.IsActivationRequested == true)
                    {
                        //Set the State;
                        SetCurrentState(state);
                    }

                    //The init request has been processed, remove its result from result pending list.
                    AsyncStateInitResults.RemoveAt(i);
                }
            }
        }

        public void PrepareStateAsync(string stateName)
        {
            PrepareStateAsync(GetByName(stateName));
        }

        public void PrepareStateAsync(GameState state)
        {
#if DEBUG
            logger.Debug("State Initialization requested (by Prepare) : {0}", state.Name);
#endif
            state.PreviousGameState = _currentState;
            AsyncStateInitResults.Add(ThreadsManager.RunAsync(() => InitializeComponentsAsync(state), singleConcurrencyRun: true));
        }

        /// <summary>
        /// Changes current game state by its name
        /// </summary>
        /// <param name="name"></param>
        public void ActivateGameStateAsync(string stateName, bool preservePreviousStates = false)
        {
            ActivateGameStateAsync(GetByName(stateName), preservePreviousStates);
        }

        public void ActivateGameStateAsync(GameState state, bool preservePreviousStates = false)
        {
            logger.Debug("Activating new game state {0}", state.Name);

            //_inActivationProcess filter that only one Activation can be requested at a time !
            //state.IsActivationRequested filter the case where the requested state is already on an Activation process (Cannot request it twice)
            if (_inActivationProcess == false || state.IsActivationRequested == false)
            {
                state.WithPreservePreviousStates = preservePreviousStates;
                _inActivationProcess = true;
                state.IsActivationRequested = true;

                state.PreviousGameState = _currentState;

#if DEBUG
                logger.Debug("State requested for activation : {0}", state.Name);
#endif

                //If ALL the components inside the states are already initiazed, switch directly the States
                if (state.GameComponents.Count > 0 && state.IsInitialized)
                {
                    SetCurrentState(state);
                    return;
                }

#if DEBUG
                logger.Debug("State Initialization requested in Async Mode : {0}", state.Name);
#endif
                AsyncStateInitResults.Add(ThreadsManager.RunAsync(() => InitializeComponentsAsync(state)));
            }
            else
            {
                logger.Warn("State : '{0}' requested to be activated while another activation requested is in initialization state, the request is dropped", state.Name);
            }
        }

        /// <summary>
        /// Changes current game state by its name
        /// </summary>
        /// <param name="name"></param>
        public bool ActivateGameState(string stateName, bool preservePreviousStates = false)
        {
            return ActivateGameState(GetByName(stateName), preservePreviousStates);
        }

        public bool ActivateGameState(GameState state, bool preservePreviousStates = false)
        {
            //_inActivationProcess filter that only one Activation can be requested at a time !
            //state.IsActivationRequested filter the case where the requested state is already on an Activation process (Cannot request it twice)
            if (_inActivationProcess == false || state.IsActivationRequested == false)
            {
                state.WithPreservePreviousStates = preservePreviousStates;
                _inActivationProcess = true;
                state.IsActivationRequested = true;

#if DEBUG
                logger.Debug("State requested for activation in Sync mode : {0}", state.Name);
#endif

                //If ALL the components inside the states are already initiazed, switch directly the States
                if (state.GameComponents.Count > 0 && state.IsInitialized)
                {
                    state.PreviousGameState = _currentState;

                    SetCurrentState(state);
                    return true;
                }

                logger.Warn("State : '{0}' requested to be activated but its state is not initialized, the request is dropped", state.Name);

            }
            else
            {
                logger.Warn("State : '{0}' requested to be activated while another activation requested is in initialization state, the request is dropped", state.Name);
            }

            return false;

        }

        public GameState GetByName(string stateName)
        {
            if (stateName == null) throw new ArgumentNullException("stateName");
            var state = _gameStates.Find(gs => gs.Name == stateName);
            if (state == null)
                throw new InvalidOperationException("No such state");

            return state;
        }

        /// <summary>
        /// Add a game state into the Gamestate's collection
        /// </summary>
        /// <param name="state">The new state</param>
        public void RegisterState(GameState state)
        {
            if (state == null)
            {
                logger.Error("Cannot add an empty game state to the collection !");
                throw new ArgumentNullException("state");
            }
            if (_gameStates.Exists(gs => gs.Name == state.Name))
            {
                logger.Error("State with such name is already added");
                throw new InvalidOperationException("State with such name is already added");
            }

            _gameStates.Add(state);
            state.StatesManager = this;
        }

        /// <summary>
        /// Will at first, disable all components from the state.
        /// Will call the Unload Methods to all registered components of the passed in state
        /// All the component will also see their IsInitialized flag set back to False
        /// After this each component should be in the same state as before the call to Initialize() and LoadContent()
        /// </summary>
        /// <param name="state">The State</param>
        public void FlushStateComponents(GameState state)
        {
            foreach (var comp in state.GameComponents.Where(x => x.IsSystemComponent == false))
            {
#if DEBUG
                logger.Debug("Components {0} flushed", comp.Name);
#endif
                comp.DisableComponent();
                comp.UnloadContent();
                comp.IsInitialized = false;
            }
        }

        /// <summary>
        /// Will at first, disable all components from the state.
        /// Will call the Unload Methods to all registered components of the passed in state
        /// All the component will also see their IsInitialized flag set back to False
        /// </summary>
        /// <param name="state">The State Name</param>
        public void FlushStateComponents(string stateName)
        {
            if (stateName == null) throw new ArgumentNullException("stateName");
            var state = _gameStates.Find(gs => gs.Name == stateName);
            if (state == null)
                throw new InvalidOperationException("No such state");
            FlushStateComponents(state);
        }
        #endregion
    }
}

