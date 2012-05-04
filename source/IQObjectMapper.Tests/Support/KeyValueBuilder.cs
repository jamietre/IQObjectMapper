using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IQObjectMapper.Tests
{
    public static class KeyValueBuilder
    {
        public static KeyValueDict<string, string> StringPair()
        {
            return new KeyValueDict<string, string>();
        }
        public static KeyValueDict<string, string> ObjectDictionary()
        {
            return new KeyValueDict<string, string>();
        }
        public static KeyValueDict<T, U> KeyValueDict<T, U>()
        {
            return new KeyValueDict<T, U>();
        }

    }

    public class KeyValueDict<T, U> : IDictionary<T, U>
    {
        protected IDictionary<T, U> _innerList;
        protected IDictionary<T, U> innerList
        {
            get
            {
                if (_innerList == null)
                {
                    _innerList = new Dictionary<T, U>();
                }
                return _innerList;
            }
        }

        public KeyValueDict<T, U> Add(T key, U value)
        {
            innerList.Add(new KeyValuePair<T, U>(key, value));
            return this;
        }



        public void Add(KeyValuePair<T, U> item)
        {
            innerList.Add(item);
        }

        public void Clear()
        {
            innerList.Clear();
        }

        public bool Contains(KeyValuePair<T, U> item)
        {
            return innerList.Contains(item);
        }

        public void CopyTo(KeyValuePair<T, U>[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return innerList.Count; }
        }

        public bool IsReadOnly
        {
            get
            {
                return innerList.IsReadOnly;
            }
        }

        public bool Remove(KeyValuePair<T, U> item)
        {
            return innerList.Remove(item);
        }

        public IEnumerator<KeyValuePair<T, U>> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void IDictionary<T, U>.Add(T key, U value)
        {
            innerList.Add(key, value);
        }

        public bool ContainsKey(T key)
        {
            return innerList.ContainsKey(key);
        }

        public ICollection<T> Keys
        {
            get { return innerList.Keys; }
        }

        public bool Remove(T key)
        {
            return innerList.Remove(key);
        }

        public bool TryGetValue(T key, out U value)
        {
            return innerList.TryGetValue(key, out value);
        }

        public ICollection<U> Values
        {
            get { return innerList.Values; }
        }

        public U this[T key]
        {
            get
            {
                return innerList[key];
            }
            set
            {
                innerList[key] = value;
            }
        }
    }
}