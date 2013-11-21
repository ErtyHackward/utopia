using System;
using Ninject.Infrastructure.Disposal;
using S33M3CoreComponents.States;

namespace Realms.Client.States
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

    public class Scope : INotifyWhenDisposed
    {
        public bool IsDisposed { get; private set; }

        public event EventHandler Disposed;

        public Scope()
        {
            IsDisposed = false;
        }

        public void Dispose()
        {
            if (Disposed != null) Disposed(this, null);  // Trigger the disposal of all object binded to this context object
            GameScope.StateManager.GameStatesCleanUp();  // Clean up states components disposed
            IsDisposed = true;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced); //Force a GC Collect
        }
    }
}
