using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main.Interfaces;
using SharpDX.Direct3D11;

namespace S33M3DXEngine.Main
{
    public class DrawableGameComponent : GameComponent, IDrawableComponent
    {
        #region Private Fields
        private DrawOrders _drawOrder;
        private bool _visible = false;

        #endregion

        #region Public Properties

        public DrawOrders DrawOrders
        {
            get { return _drawOrder; }
        }

        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    OnVisibleChanged(this, EventArgs.Empty);
                }
            }
        }

        public override bool isEnabled
        {
            get
            {
                return base.isEnabled && Visible;
            }
        }

        #endregion

        public override void EnableComponent(bool forced = false)
        {
            if (!AutoStateEnabled && !forced) return;

            this.Visible = true;
            base.EnableComponent(forced);
        }

        public override void DisableComponent()
        {
            this.Visible = false;
            base.DisableComponent();
        }

        public DrawableGameComponent()
            :base()
        {
            _drawOrder = new DrawOrders(this);
        }

        public DrawableGameComponent(string name)
            : base(name)
        {
            _drawOrder = new DrawOrders(this);
        }

        public override void BeforeDispose()
        {
            if (DrawOrderChanged != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in DrawOrderChanged.GetInvocationList())
                {
                    DrawOrderChanged -= (EventHandler<EventArgs>)d;
                }
            }

            if (VisibleChanged != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in VisibleChanged.GetInvocationList())
                {
                    VisibleChanged -= (EventHandler<EventArgs>)d;
                }
            }
        }

        public virtual void OnDrawOrderChanged(object sender, EventArgs args)
        {
            if (DrawOrderChanged != null)
                DrawOrderChanged(sender, args);
        }

        protected virtual void OnVisibleChanged(object sender, EventArgs args)
        {
            if (VisibleChanged != null)
                VisibleChanged(sender, args);
        }

        #region Public Events

        public virtual void Draw(DeviceContext context, int index)
        {
        }

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        #endregion Public Events
    }
}
