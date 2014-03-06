using System.Collections.Generic;

namespace Utopia.Shared.Server.AStar
{
    public class AStarList<T>
    {
        private readonly List<T> _list = new List<T>();

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

        public void Clear()
        {
            _list.Clear();
        }
    }
}
