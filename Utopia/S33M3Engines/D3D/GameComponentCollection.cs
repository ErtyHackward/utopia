using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace S33M3Engines.D3D
{
    /// <summary>
    /// Specialized collection holding components, which triggers component add / remove events when calling add and remove methods
    /// </summary>
    public class GameComponentCollectionEventArgs : EventArgs
    {
        private readonly GameComponent _gameComponent;

        public GameComponentCollectionEventArgs(GameComponent gameComponent)
        {
            _gameComponent = gameComponent;
        }

        public GameComponent GameComponent
        {
            get { return _gameComponent; }
        }

    }

    public class GameComponentCollection : Collection<GameComponent>
    {
        public event EventHandler<GameComponentCollectionEventArgs> ComponentAdded;
        public event EventHandler<GameComponentCollectionEventArgs> ComponentRemoved;

        public void AddRange(IEnumerable<GameComponent> gameComponents)
        {
            foreach (var gameComponent in gameComponents)
            {
                this.Add(gameComponent);
            }
        }

        protected override void ClearItems()
        {
            foreach (var component in this)
            {
                OnComponentRemoved(new GameComponentCollectionEventArgs(component));
            }

            base.ClearItems();
        }

        protected override void InsertItem(int index, GameComponent item)
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
            GameComponent item = this[index];
            base.RemoveItem(index);
            OnComponentRemoved(new GameComponentCollectionEventArgs(item));
        }

        protected override void SetItem(int index, GameComponent item)
        {
            GameComponent oldItem = this[index];
            if (!oldItem.Equals(item))
            {
                OnComponentRemoved(new GameComponentCollectionEventArgs(oldItem));
                base.SetItem(index, item);
                OnComponentAdded(new GameComponentCollectionEventArgs(item));
            }
        }
    }
}