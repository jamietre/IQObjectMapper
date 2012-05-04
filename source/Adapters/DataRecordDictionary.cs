using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQObjectMapper.Adapters
{
    /// <summary>
    /// Exposes an IDataRecord as a sequence of KeyValuePair-string,object- for each column
    /// </summary>
    public class DataRecordDictionary : IDictionary<string, object>
    {
        public DataRecordDictionary(IDataRecord record, IMapOptions options=null)
        {
            Options = MapOptions.From(options);
            InnerDictionary = GetDictionary(record);

        }
        public MapOptions Options { get; set; }

        IDictionary<string, object> InnerDictionary;
        
        IDictionary<string,object> GetDictionary(IDataRecord record)
        {
            var adapter = new DataRecordAdapter(record);

            IDictionary<string, object> dict = new Dictionary<string, object>(
                Options.CaseSensitive ? StringComparer.CurrentCulture : StringComparer.CurrentCultureIgnoreCase
                );
            foreach (var obj in adapter)
            {
                dict[obj.Key] = obj.Value;
            }
            return dict;
        }

        public void Add(string key, object value)
        {
            InnerDictionary.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return InnerDictionary.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return InnerDictionary.Keys; }
        }

        public bool Remove(string key)
        {
            return InnerDictionary.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return InnerDictionary.TryGetValue(key, out value);
        }

        public ICollection<object> Values
        {
            get { return InnerDictionary.Values; }
        }

        public object this[string key]
        {
            get
            {
                return InnerDictionary[key];
            }
            set
            {
                InnerDictionary[key] = Values;
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            InnerDictionary.Add(item);
        }

        public void Clear()
        {
            InnerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return InnerDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            InnerDictionary.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return InnerDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return InnerDictionary.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return InnerDictionary.Remove(item);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return InnerDictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
