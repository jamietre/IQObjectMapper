using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQObjectMapper.Impl
{
    /// <summary>
    /// A dictionary class that handles CanAddProperties and IsReadOnly
    /// </summary>
    public class ManagedDictionary<T>: IDictionary<string,T>
    {
        
        public ManagedDictionary(IMapOptions options=null)
        {
            Options = MapOptions.From(options);
            StringComparer comparer = ObjectMapper.MapperCache.GetStringComparer(Options);

            InnerDict = new Dictionary<string, T>(comparer);
        }

        public MapOptions Options { get; set; }

        protected IDictionary<string, T> InnerDict;

        protected bool CanAlterProperties
        {
            get
            {
                return Options.CanAlterProperties && !IsReadOnly;
            }
        }

        #region IDictionary members
        
        public bool IsReadOnly
        {
            get
            {
                return Options.IsReadOnly;
            }
        }

        public void Add(string key, T value)
        {
            if (CanAlterProperties)
            {
                InnerDict.Add(key, value);
            }
            else
            {
                throw new InvalidOperationException("Adding properties prohibited by options.");
            }
        }

        public bool ContainsKey(string key)
        {
            return InnerDict.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return InnerDict.Keys; }
        }

        public bool Remove(string key)
        {
            if (CanAlterProperties)
            {
                return InnerDict.Remove(key);
            }
            else
            {
                throw new InvalidOperationException("Removing properties is prohibited by options.");
            }
        }

        public bool TryGetValue(string key, out T value)
        {
            return InnerDict.TryGetValue(key, out value);
        }

        public ICollection<T> Values
        {
            get { return InnerDict.Values; }
        }

        public T this[string key]
        {
            get
            {
                return InnerDict[key];
            }
            set
            {
                bool allow = !IsReadOnly &&
                    (ContainsKey(key) || Options.CanAlterProperties);

                if (allow)
                {
                    InnerDict[key] = value;
                }
                else
                {
                    throw new InvalidOperationException(ContainsKey(key)  ?
                        "Changes are prohibited by options." :
                        "Adding properties is prohibited by options.");
                }
            }
        }

        public void Add(KeyValuePair<string, T> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            if (CanAlterProperties)
            {
                InnerDict.Clear();
            }
            else
            {
                throw new InvalidOperationException("Removing properties is prohibited by options.");
            }
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            return InnerDict.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            InnerDict.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return InnerDict.Count; }
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            // the string comparer won't be honored if we just try to remove kvp

            if (CanAlterProperties)
            {
                T value;

                return TryGetValue(item.Key, out value)
                    && (item.Value == null ?
                        value == null :
                        item.Value.Equals(value))
                    && InnerDict.Remove(item.Key);
                
            }
            else
            {
                throw new InvalidOperationException("Removing properties is prohibited by options.");
            }
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, T>>)InnerDict).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, T>>)InnerDict).GetEnumerator();
        }

        #endregion

    }
}
