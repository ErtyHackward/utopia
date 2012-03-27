using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main.Interfaces;
using SharpDX;
using SharpDX.Direct3D11;

namespace S33M3DXEngine.Main
{
    public abstract class GameComponent : Component, IUpdatableComponent
    {
        #region Private variables
        private bool _updatable = false;
        private int _updateOrder = 10;
        private bool _isInitialized = false;
        #endregion

        #region Public properties
        public bool IsSystemComponent { get; protected set; }
        /// <summary>
        /// Indicate if the loadcontent can be created using a defered context, or not. By default = false, deffered rendering impose some restrictions
        /// That must be known
        /// </summary>
        public bool IsDefferedLoadContent { get; protected set; }
        public event EventHandler<EventArgs> UpdateOrderChanged;
        public event EventHandler<EventArgs> UpdatableChanged;
        public bool IsInitialized
        {
            get { return _isInitialized; }
            set { _isInitialized = value; }
        }

        public virtual bool isEnabled
        {
            get { return Updatable; }
        }

        public bool CatchExclusiveActions { get; set; }

        public bool Updatable
        {
            get { return _updatable; }
            set
            {
                if (_updatable != value)
                {
                    _updatable = value;
                    OnUpdatableChanged(this, EventArgs.Empty);
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

        public GameComponent()
            :this(null)
        {
        }

        public GameComponent(string name)
            : base(name)
        {
            IsDefferedLoadContent = false;
        }

        public override void Dispose()
        {
            if (UpdateOrderChanged != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in UpdateOrderChanged.GetInvocationList())
                {
                    UpdateOrderChanged -= (EventHandler<EventArgs>)d;
                }
            }

            if (UpdatableChanged != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in UpdatableChanged.GetInvocationList())
                {
                    UpdatableChanged -= (EventHandler<EventArgs>)d;
                }
            }
            base.Dispose();
        }

        public virtual void EnableComponent()
        {
            this.Updatable = true;
        }

        public virtual void DisableComponent()
        {
            this.Updatable = false;
        }

        public virtual void Initialize()
        {
        }

        public virtual void LoadContent(DeviceContext context)
        {
        }

        public virtual void Update(GameTime timeSpent)
        {
        }

        public virtual void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
        }

        protected virtual void OnUpdatableChanged(object sender, EventArgs args)
        {
            if (UpdatableChanged != null)
                UpdatableChanged(this, args);
        }

        protected virtual void OnUpdateOrderChanged(object sender, EventArgs args)
        {
            if (UpdateOrderChanged != null)
                UpdateOrderChanged(this, args);
        }

    }
}
