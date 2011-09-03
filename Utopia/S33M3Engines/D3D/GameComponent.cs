using System;

namespace S33M3Engines.D3D
{

    public interface IDrawableComponent : IUpdatableComponent
    {
        void Draw();
    }

    public interface IUpdatableComponent : IDisposable
    {
        void Initialize();
        void Update(ref GameTime timeSpent);
        void Interpolation(ref double interpolationHd, ref float interpolationLd);
    }

    public interface IGameComponent
    {
        bool CallUpdate { get; set; }
        bool CallDraw { get; set; }
        void Initialize();
        void LoadContent();
        void UnloadContent();
        void Update(ref GameTime timeSpent);
        void Interpolation(ref double interpolationHd, ref float interpolationLd);
        void DrawDepth0();
        void DrawDepth1();
        void DrawDepth2();
    }

    public class GameComponent : IGameComponent
    {
        #region Private variables
        bool _callUpdate = true;
        bool _callDraw = true;
        #endregion

        #region Public properties
        public bool CallUpdate { get { return _callUpdate; }
            set
        {
                if ( ! _callUpdate && value)
                {
                    OnEnable();
                } else if ( _callUpdate && ! value)
                {
                    OnDisable();
                }

            _callUpdate = value;
        } 
        }

        public bool CallDraw { get { return _callDraw; } set { _callDraw = value; } }
        #endregion

        //Ctor
        public GameComponent()
        {
        }

        public virtual void Initialize()
        {
        }

        public virtual void LoadContent()
        {
        }

        public virtual void UnloadContent()
        {
        }

        public virtual void Update(ref GameTime timeSpent)
        {
        }

        public virtual void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
        }

        public virtual void DrawDepth0()
        {
        }

        public virtual void DrawDepth1()
        {
        }

        public virtual void DrawDepth2()
        {
        }

        /// <summary>
        /// Called when component is disabled : CallDraw is changed from true to false
        /// </summary>
        protected virtual void OnDisable()
        {
        }

        /// <summary>
        /// Called when component is enabled : CallDraw is changed from false to true
        /// </summary>        
        protected virtual void OnEnable()
        {
        }

    }
}
