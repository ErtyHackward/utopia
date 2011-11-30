using System;

namespace S33M3Engines.D3D
{
    public abstract class GameComponent : IUpdateableComponent
    {
        #region Private variables
        private bool _enabled = true;
        private int _updateOrder = 10;

        private string _initStep = "Ready";
        private int _initVal = 100;
        private bool _isInitialized;
        #endregion

        #region Public properties
        
        public virtual string InitStep { get { return _initStep; } }

        public virtual int InitVal { get { return _initVal; } }

        public virtual bool IsInitialized { get { return _isInitialized; } }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnEnabledChanged();
                }
            }
        }

        public int UpdateOrder
        {
            get { return _updateOrder; }
            set
            {
                if (_updateOrder != value)
                {
                    _updateOrder = value;
                    OnUpdateOrderChanged();
                }
            }
        }

        #endregion

        public event EventHandler<EventArgs> EnabledChanged;

        protected virtual void OnEnabledChanged()
        {
            var handler = EnabledChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        
        public event EventHandler<EventArgs> UpdateOrderChanged;

        protected virtual void OnUpdateOrderChanged()
        {
            var handler = UpdateOrderChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when component was initialized
        /// </summary>
        public event EventHandler Initialized;

        protected void OnInitialized()
        {
            var handler = Initialized;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when all required component content was loaded
        /// </summary>
        public event EventHandler ContentLoaded;

        protected void OnContentLoaded()
        {
            var handler = ContentLoaded;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        
        internal void InternalInitialize()
        {
            Initialize();
            OnInitialized();
        }

        internal void InternalLoadContent()
        {
            LoadContent();
            OnContentLoaded();
            _isInitialized = true;
        }
        
        public virtual void Initialize() { }

        public virtual void LoadContent() { }

        public virtual void UnloadContent() { }

        public virtual void Update(ref GameTime timeSpent) { }

        public virtual void Interpolation(ref double interpolationHd, ref float interpolationLd) { }

        public virtual void Dispose() { }
    }
}