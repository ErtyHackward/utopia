using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Engines.D3D
{
    public class DrawableGameComponent : GameComponent, IDrawableComponent
    {
        #region Private Fields
        private DrawOrders _drawOrder;
        private bool _visible;

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

        #endregion

        public DrawableGameComponent()
        {
            _drawOrder = new DrawOrders(this);
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

        public virtual void Draw(int index)
        {
        }

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        #endregion Public Events
    }
}