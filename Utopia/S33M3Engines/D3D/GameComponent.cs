using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct;
using SharpDX;

namespace S33M3Engines.D3D
{

    public interface IDrawableComponent : IUpdatableComponent
    {
        void Draw();
    }

    public interface IUpdatableComponent : IDisposable
    {
        void Initialize();
        void Update(ref GameTime TimeSpend);
        void Interpolation(ref double interpolation_hd, ref float interpolation_ld);
    }

    public interface IGameComponent
    {
        bool CallUpdate { get; set; }
        bool CallDraw { get; set; }
        void Initialize();
        void LoadContent();
        void UnloadContent();
        void Update(ref GameTime TimeSpend);
        void Interpolation(ref double interpolation_hd, ref float interpolation_ld);
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
        public bool CallUpdate { get { return _callUpdate; } set { _callUpdate = value; } }
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

        public virtual void Update(ref GameTime TimeSpend)
        {
        }

        public virtual void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
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

    }
}
