﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3DXEngine.Main.Interfaces;
using S33M3DXEngine.Threading;
using SharpDX.Direct3D11;
using S33M3DXEngine;
using System.Threading.Tasks;

namespace S33M3CoreComponents.States
{

    /// <summary>
    /// Base class for game state. Each state contains a list of components that should be active at the same time
    /// It keep a collection of components that must run at the same time in order to display "something" properly. 
    /// This "Something" is call a Game State
    /// </summary>
    public abstract class GameState
    {
        private bool _allowMouseCaptureChange = false;


        #region Public variables

        public bool IsActivationRequested { get; set; }
        public bool WithPreservePreviousStates { get; set; }
        public bool AllowMouseCaptureChange
        {
            get { return _allowMouseCaptureChange; }
            set { _allowMouseCaptureChange = value; }
        }

        /// <summary>
        /// Contains the GameState from wich this one has been activated
        /// </summary>
        public GameState PreviousGameState { get; set; }

        /// <summary>
        /// Name of the state
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// List of components that are only of type GameComponent (Not involving drawing)
        /// </summary>
        public List<GameComponent> GameComponents { get; protected set; }

        /// <summary>
        /// Gets states manager
        /// </summary>
        public GameStatesManager StatesManager { get; internal set; }

        /// <summary>
        /// A GameState is initialized if all its components have been initiazed and it is itself initialized
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return GameComponents.All(c => c.IsInitialized) && IsDirty == false;
            }
        }

        public bool IsDirty { get; set; }

        public event EventHandler StateInitialized;
        #endregion

        public GameState(GameStatesManager StatesManager)
        {
            this.StatesManager = StatesManager;
            GameComponents = new List<GameComponent>();
        }

        #region Private methods
        /// <summary>
        /// Make component enabled and visible if possible
        /// </summary>
        /// <param name="gc"></param>
        protected void AddComponent(GameComponent gc)
        {
            if (gc == null) return;
            //Should only be called "once", but doesn't hurt to check the collection state before each modification
            CleanUpComponents();
            //Add a component only if not already added
            if (GameComponents.Contains(gc) == false)
            {
                GameComponents.Add(gc);
            }
        }

        protected void CleanUpComponents()
        {
            //Remove all disposed components
            GameComponents.RemoveAll(x => x.IsDisposed);
        }

        protected void AddComponent(IGameComponent igc)
        {
            AddComponent((GameComponent)igc);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// All Components used by this GameState should be Added in the GameState here
        /// This will be called in a multithreaded way !
        /// </summary>
        public virtual void Initialize(DeviceContext context)
        {
            //Call the Initialized from each Registered component, if needed !
            //Will be thread dispatched !
            List<Task> _startedTask = new List<Task>();
            GameComponents.ForEach(gc =>
            {
                if (gc.IsInitialized == false)
                {
                    //Start the Initialize()
                    _startedTask.Add(ThreadsManager.RunAsync(gc.Initialize));
                }
            });

            Task.WaitAll(_startedTask.ToArray());

            //Call the LoadContents from each Registered component, if needed !
            //!! Those methods are not Thread Safe !! => cannot thread dispatch them, can only run once at a time in this thread context
            foreach (var gc in GameComponents.Where(x => x.IsDefferedLoadContent))
            {
                if (gc.IsInitialized == false)
                {
                    gc.LoadContent(context);
                    gc.IsInitialized = true;
                }
            }

            IsDirty = false;
        }

        /// <summary>
        /// Called when componend get initialized. This is a place where you can perform custom actions
        /// </summary>
        /// <param name="previousState">Previous game state or null</param>
        public virtual void OnEnabled(GameState previousState)
        {
        }

        public void RaisedInitializedEvent()
        {
            if (StateInitialized != null) StateInitialized(this, null); 
        }

        /// <summary>
        /// Called when componend get disabled. This is a place where you can perform custom actions
        /// </summary>
        /// <param name="nextState">Next game state or null</param>
        public virtual void OnDisabled(GameState nextState)
        {
        }
        #endregion

    }
}
