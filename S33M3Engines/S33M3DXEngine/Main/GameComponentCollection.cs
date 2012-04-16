using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using S33M3DXEngine.Main.Interfaces;
using SharpDX;

namespace S33M3DXEngine.Main
{
    /// <summary>
    /// Specialized collection holding components, which triggers component add / remove events when calling add and remove methods
    /// </summary>
    public class GameComponentCollectionEventArgs : EventArgs
    {
        private readonly IGameComponent _gameComponent;

        public GameComponentCollectionEventArgs(IGameComponent gameComponent)
        {
            _gameComponent = gameComponent;
        }

        public IGameComponent GameComponent
        {
            get { return _gameComponent; }
        }

    }

    public class GameComponentCollection : Collection<IGameComponent>, IDisposable
    {
        public event EventHandler<GameComponentCollectionEventArgs> ComponentAdded;
        public event EventHandler<GameComponentCollectionEventArgs> ComponentRemoved;

        //Will remove, all components that have been disposed
        public void CleanUp()
        {
            var component2BRemoved = this.Where(x => x.IsDisposed);
            List<int> indexList = new List<int>();
            foreach (var fc in component2BRemoved)
            {
                indexList.Add(base.IndexOf(fc));
                OnComponentRemoved(new GameComponentCollectionEventArgs(fc));
            }

            foreach (int i in indexList.OrderByDescending(x => x))
            {
                base.RemoveAt(i);
            }
        }

        public void Dispose()
        {
            if (ComponentAdded != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in ComponentAdded.GetInvocationList())
                {
                    ComponentAdded -= (EventHandler<GameComponentCollectionEventArgs>)d;
                }
            }

            if (ComponentRemoved != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in ComponentRemoved.GetInvocationList())
                {
                    ComponentRemoved -= (EventHandler<GameComponentCollectionEventArgs>)d;
                }
            }
        }

        public void AddRange(IEnumerable<IGameComponent> gameComponents)
        {
            foreach (var gameComponent in gameComponents)
            {
                this.Add(gameComponent);
            }
        }

        protected override void ClearItems()
        {
            foreach (IGameComponent component in this)
            {
                OnComponentRemoved(new GameComponentCollectionEventArgs(component));
            }

            base.ClearItems();
        }

        protected override void InsertItem(int index, IGameComponent item)
        {
            base.InsertItem(index, item);
            OnComponentAdded(new GameComponentCollectionEventArgs(item));
        }

        private void OnComponentAdded(GameComponentCollectionEventArgs eventArgs)
        {
            if (ComponentAdded != null)
                ComponentAdded(this, eventArgs);
        }

        private void OnComponentRemoved(GameComponentCollectionEventArgs eventArgs)
        {
            if (ComponentRemoved != null)
                ComponentRemoved(this, eventArgs);
        }

        protected override void RemoveItem(int index)
        {
            IGameComponent item = this[index];
            base.RemoveItem(index);
            OnComponentRemoved(new GameComponentCollectionEventArgs(item));
        }

        protected override void SetItem(int index, IGameComponent item)
        {
            IGameComponent oldItem = this[index];
            if (!oldItem.Equals(item))
            {
                OnComponentRemoved(new GameComponentCollectionEventArgs(oldItem));
                base.SetItem(index, item);
                OnComponentAdded(new GameComponentCollectionEventArgs(item));
            }
        }

    }
}
