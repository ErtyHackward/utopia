using System;
using System.Collections.ObjectModel;

namespace S33M3Engines.D3D
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

    public class GameComponentCollection : Collection<IGameComponent>
    {
        public event EventHandler<GameComponentCollectionEventArgs> ComponentAdded;
        public event EventHandler<GameComponentCollectionEventArgs> ComponentRemoved;


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