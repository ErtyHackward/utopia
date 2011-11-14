#region

using System;

#endregion

namespace S33M3Engines.D3D
{
    public abstract class GameComponent : IUpdateableComponent
    {
        #region Private variables
        private bool _enabled = true;
        private int _updateOrder = 10;

        private string _initStep = "Ready";
        private int _initVal = 100;
        private bool _isInitialized = true;
        #endregion

        #region Public properties

        public event EventHandler<EventArgs> EnabledChanged;

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
                    OnEnabledChanged(this, EventArgs.Empty);
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
                    OnUpdateOrderChanged(this, EventArgs.Empty);
                }
            }
        }

        #endregion

        public virtual
            void Initialize()
        {
        }

        public virtual
            void LoadContent()
        {
        }

        public virtual
            void UnloadContent()
        {
        }

        public virtual
            void Update(ref GameTime timeSpent)
        {
        }

        public virtual
            void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
        }

        public
            event EventHandler<EventArgs> UpdateOrderChanged;

        protected virtual void OnEnabledChanged(object sender, EventArgs args)
        {
            if (EnabledChanged != null)
                EnabledChanged(this, args);
        }

        protected virtual void OnUpdateOrderChanged(object sender, EventArgs args)
        {
            if (UpdateOrderChanged != null)
                UpdateOrderChanged(this, args);
        }

        public virtual void Dispose()
        {
        }
    }
}