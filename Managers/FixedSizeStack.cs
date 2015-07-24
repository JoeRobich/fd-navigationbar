using System.Collections;
using System.Collections.Generic;

namespace NavigationBar.Managers
{
    class FixedSizeStack<T> : IEnumerable<T>
    {
        private List<T> _list = new List<T>();
        private int _maxSize = 10;

        public FixedSizeStack(int size)
        {
            if (size > 0)
                _maxSize = size;
        }

        public int Count
        {
            get {  return _list.Count; }
        }

        public T Pop()
        {
            T item = _list[0];
            _list.RemoveAt(0);
            return item;
        }

        public void Push(T item)
        {
            _list.Insert(0, item);

            if (_list.Count >= _maxSize)
                _list.RemoveAt(_maxSize - 1);
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}