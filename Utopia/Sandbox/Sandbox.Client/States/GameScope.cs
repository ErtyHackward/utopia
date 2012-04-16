using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Infrastructure.Disposal;

namespace Sandbox.Client.States
{
    public static class GameScope
    {
        public static Scope CurrentGameScope = new Scope() { ScopeName = "Initial Game Scope" };
    }

    public class Scope : IDisposable, INotifyWhenDisposed
    {
        public string ScopeName { get; set; }

        public void Dispose()
        {
            if (Disposed != null) Disposed(this, null);

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }

        public event EventHandler Disposed;
        public bool IsDisposed
        {
            get { return IsDisposed; }
        }
    }
}
