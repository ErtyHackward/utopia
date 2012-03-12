using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using S33M3_DXEngine.Main.Interfaces;

namespace S33M3_DXEngine.Main
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
