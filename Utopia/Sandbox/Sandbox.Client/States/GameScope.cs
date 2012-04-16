using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Infrastructure.Disposal;
using S33M3CoreComponents.States;

namespace Sandbox.Client.States
{
    public static class GameScope
    {
        public static Scope CurrentGameScope = new Scope();
        public static GameStatesManager StateManager;

        public static void CreateNewScope()
        {
            CurrentGameScope = new Scope();
        }
    }

    public class Scope : IDisposable, INotifyWhenDisposed
    {
        public Scope()
        {
        }

        public void Dispose()
        {
            if (Disposed != null) Disposed(this, null);  // Trigger the disposal of all object binded to this context object
            GameScope.StateManager.GameStatesCleanUp();  // Clean up states components disposed
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced); //Force a GC Collect
        }

        public event EventHandler Disposed;
        public bool IsDisposed
        {
            get { return IsDisposed; }
        }
    }
}
