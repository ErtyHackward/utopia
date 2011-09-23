using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Server.AStar;

namespace PathFinder.AStar
{
    public class AStarList<T>
    {
        private List<T> _list = new List<T>();

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

        public bool Contains(T item, Predicate<T> sure)
        {
            foreach (var i in _list)
            {
                if (sure(i)) return true;
            }
            return false;
            //var index = _list.BinarySearch(item, Comparer);

            //if (index >= 0)
            //{
            //    while (true)
            //    {
            //        index++;
            //        if (index == _list.Count) return false;
            //        if (Comparer.Compare(_list[index], item) == 0)
            //        {
            //            if (sure(_list[index]))
            //                return true;
            //        }
            //        else return false;
            //    }
            //}
            return false;

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
