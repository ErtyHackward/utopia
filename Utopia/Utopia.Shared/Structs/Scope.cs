using System;

namespace Utopia.Shared.Structs
{
    public class Scope : IDisposable
    {
        Action _action;

        public Scope(Action action)
        {
            _action = action;
        }
        public void Dispose()
        {
            if (_action != null)
                _action();
        }
    }
}