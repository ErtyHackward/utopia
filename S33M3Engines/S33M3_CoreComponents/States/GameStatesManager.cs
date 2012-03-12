using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_DXEngine.Main;
using S33M3_CoreComponents.States.Interfaces;
using System.Threading;
using S33M3_DXEngine.Threading;
using Amib.Threading;
using S33M3_DXEngine.Main.Interfaces;
using SharpDX;
using SharpDX.Direct3D11;
using S33M3_DXEngine;

namespace S33M3_CoreComponents.States
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

        //Thread Helper objects
        internal IWorkItemsGroup GameStatesManagerThreadPoolGrp;
        internal IWorkItemsGroup InitializeThreadPoolGrp;
        private List<IWorkItemResult<GameState>> AsyncStateInitResults = new List<IWorkItemResult<GameState>>();
        #endregion

        #region Public properties/variables

        /// <summary>
        /// Gets or sets current game state
        /// </summary>
        public GameState CurrentState
        {
            get { return _currentState; }
            private set
            {
                if (_currentState != value)
                {
                    //Not State switcher defined, do the activatino directly
                    if (SwitchComponent == null)
                    {
                        SwitchActiveGameState(value);
                    }
                    else
                    {
                        _nextState = value;
                        SwitchComponent.BeginSwitch();
                        SwitchComponent.EnableComponent();
                    }
                }
            }
        }

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

        public GameStatesManager(D3DEngine engine, Game game)
            : this(engine, game, null)
        {
        }

        public GameStatesManager(D3DEngine engine, Game game, ISwitchComponent switchComponent)
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
            _loadContext = ToDispose(new DeviceContext(_engine.Device));
#if DEBUG
            _loadContext.DebugName = "GameState Deffered Context";
#endif
            //Create the 2 ThreadPool "Group" with different concurrency
            InitializeThreadPoolGrp = SmartThread.ThreadPool.CreateWorkItemsGroup(SmartThread.ThreadPool.Concurrency);           //Max possible concurrency possible
            GameStatesManagerThreadPoolGrp = SmartThread.ThreadPool.CreateWorkItemsGroup(SmartThread.ThreadPool.Concurrency);    //Max possible concurrency possible

            //Activate this component
            this.EnableComponent();
        }

        #region Private methods

        //End of activation process where the new state is pushed for rendering
        private void SwitchActiveGameState(GameState newState)
        {
            //Raise the GameState.OnDisabled event from the Currently running GameState
            if (_currentState != null) _currentState.OnDisabled(newState); //Disable the currently running GameState

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

            //Raise the GameState.OnEnabled event from the Newly running GameState
            if (_currentState != null) _currentState.OnEnabled(prev);

            _inActivationProcess = false;
            _currentState.IsActivationRequested = false;
        }

        /// <summary>
        /// Will make the components from the "Current" GameState ready to be rendered
        /// Will disable if needed the components from the previous gamestate.
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="current"></param>
        private void DisablePreviousEnableCurrent(GameState previous, GameState current)
        {
            if (previous != null)
            {
                // disable previous stuff that are not existing in the current gamecomponents
                foreach (GameComponent gc in previous.GameComponents.Except(current.GameComponents))
                {
                    if (gc.IsSystemComponent) continue;
                    gc.DisableComponent();
                }
            }

            // Enable current stuff, except the Switch Component
            foreach (GameComponent gc in current.GameComponents)
            {
                if (gc.IsSystemComponent) continue;
                gc.EnableComponent();
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

        public override void Update(GameTime timeSpent)
        {
            if(AsyncStateInitResults.Count == 0) return;

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
                            gc.LoadContent(_engine.ImmediateContext);
                            gc.IsInitialized = true;
                        }
                    }

                    //Add the current GameState components to the gamecomponents "initialized" collection
                    state.GameComponents.ForEach(gc =>
                    {
                        if (!_game.GameComponents.Contains(gc)) _game.GameComponents.Add(gc);
                    });


                    //The states is initialized, was it an initialization requested from an activation ?
                    if (state.IsActivationRequested == true)
                    {
                        //Set the State;
                        CurrentState = state;
                    }

                    //The init request has been processed, remove its result from result pending list.
                    AsyncStateInitResults.RemoveAt(i);
                }
            }
        }

        public void PrepareState(string stateName)
        {
            PrepareState(GetByName(stateName));
        }

        public void PrepareState(GameState state)
        {
             AsyncStateInitResults.Add(GameStatesManagerThreadPoolGrp.QueueWorkItem(new Amib.Threading.Func<GameState, GameState>(InitializeComponentsAsync), state));
        }

        /// <summary>
        /// Changes current game state by its name
        /// </summary>
        /// <param name="name"></param>
        public void ActivateGameState(string stateName)
        {
            ActivateGameState(GetByName(stateName));
        }

        public void ActivateGameState(GameState state)
        {
            //_inActivationProcess filter that only one Activation can be requested at a time !
            //state.IsActivationRequested filter the case where the requested state is already on an Activation process (Cannot request it twice)
            if (_inActivationProcess == false || state.IsActivationRequested == false)
            {
                _inActivationProcess = true;
                state.IsActivationRequested = true;

                //If ALL the components inside the states are already initiazed, switch directly the States
                if (state.GameComponents.Count > 0 && state.IsInitialized)
                {
                    CurrentState = state;
                    return;
                }

                AsyncStateInitResults.Add(GameStatesManagerThreadPoolGrp.QueueWorkItem(new Amib.Threading.Func<GameState, GameState>(InitializeComponentsAsync), state));
            }
            else
            {
                logger.Warn("{0} requested to be activated while another activation requested is in initialization state, the request is dropped", state.Name);
            }
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
        #endregion
    }
}

