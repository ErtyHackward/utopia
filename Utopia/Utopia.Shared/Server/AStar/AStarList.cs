using System.Collections.Generic;

namespace Utopia.Shared.Server.AStar
{
    public class AStarList<T>
    {
        private readonly List<T> _list = new List<T>();
        private Dictionary<T,T>  _dictionary = new Dictionary<T, T>();

        public IComparer<T> Comparer { get; set; }

        public AStarList()
            : this(Comparer<T>.Default)
        {

        }

        public AStarList(IComparer<T> comparer)
        {
            Comparer = comparer;
        }

        public void Add(T item)
        {
            _dictionary.Add(item, item);

            var index = _list.BinarySearch(item, Comparer);

            if (index < 0)
            {
                _list.Insert(~index, item);
            }
            else
            {
                _list.Insert(index, item);
            }
        }

        public T Pop()
        {
            var value = _list[_list.Count - 1];
            _list.RemoveAt(_list.Count - 1);

            _dictionary.Remove(value);

            return value;
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }


        public bool TryGetValue(T key, out T value)
        {
            return _dictionary.TryGetValue(key, out value);
        }


        public void Clear()
        {
            _list.Clear();
            _dictionary.Clear();
        }

        public T this[T node]
        {
            get { return _dictionary[node]; }
            set { _dictionary[node] = value; }
        }
    }
}
