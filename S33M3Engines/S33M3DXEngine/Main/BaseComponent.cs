using SharpDX;

namespace S33M3DXEngine.Main
{
    public class BaseComponent : Component
    {
        public new void Dispose()
        {
            BeforeDispose();
            base.Dispose();
            AfterDispose();
        }

        public BaseComponent()         
            : this(null)
        {
        }

        public BaseComponent(string name)
            :base()
        {
            base.Name = name ?? GetType().Name;
        }

        public virtual void BeforeDispose()
        {
        }

        public virtual void AfterDispose()
        {
        }
    }
}
