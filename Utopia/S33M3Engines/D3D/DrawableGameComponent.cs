using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Engines.D3D
{
    public class DrawableGameComponent : GameComponent, IDrawableComponent
    {
        #region Private Fields

        private int _drawOrder = 10;
        private bool _visible = true;

        #endregion Private Fields

        #region Public Properties

        public int DrawOrder
        {
            get { return _drawOrder; }
            set
            {
                if (_drawOrder != value)
                {
                    _drawOrder = value;
                    OnDrawOrderChanged(this, EventArgs.Empty);
                }
            }
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

        #endregion Public Properties

        protected virtual void OnDrawOrderChanged(object sender, EventArgs args)
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

        public virtual void Draw()
        {
        }

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        #endregion Public Events
    }
}