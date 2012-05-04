using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Dynamic;
using Microsoft.CSharp.RuntimeBinder;
using IQObjectMapper.Adapters;
using IQObjectMapper.Impl;

namespace IQObjectMapper
{
    /// <summary>
    /// A dynamic object implementation that gives us control over handling of missing properties, 
    /// case sensitivity, and adding properties. This is the default dynamic type returned by 
    /// IQObjectMapper and is the internal class for adapters that expose a dynamic.
    /// </summary>
    public class IQDynamicObject : DynamicObject, IDictionary<string, object>
    {
        #region constructor

        public IQDynamicObject(IMapOptions options = null)
        {
            Options = MapOptions.From(options);
            InnerDict = ObjectMapper.MapperCache.GetDictionary<object>(Options);
        }

        #endregion

        #region private properties

        protected IDictionary<string,object> InnerDict;

        #endregion

        #region public methods

        public MapOptions Options { get; set; }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name;

            if (TryGetValue(name, out result))
            {
                return true;
            }
            else
            {
                result = Options.UndefinedValue;
                return Options.CanAccessMissingProperties;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // this is wrapped in try/catch to allow the Dynamic system to manage access
            // failures. The upshot of doing this is that inner errors (e.g. from accessing
            // a missing property) will have an exception typeof RuntimeBinderError instead of
            // the error thrown by the dictionary implemenation bubbling.
            try
            {
                this[binder.Name] = value;
                return true;
            }
            catch(KeyNotFoundException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return InnerDict.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return InnerDict.Equals(obj);
        }

        public override string ToString()
        {
            return InnerDict.ToString();
        }

        #endregion


        #region IDictionary members

        public bool IsReadOnly
        {
            get
            {
                return Options.IsReadOnly;
            }
        }

        public void Add(string key, object value)
        {
            if (Options.CanAlterProperties)
            {
                InnerDict.Add(key, value);
            }
            else
            {
                throw new InvalidOperationException("Adding properties prohibited by CanAlterProperties setting.");
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
            return InnerDict.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return InnerDict.TryGetValue(key, out value);
        }

        public ICollection<object> Values
        {
            get { return InnerDict.Values; }
        }

        public object this[string key]
        {
            get
            {
                return InnerDict[key];
            }
            set
            {

                InnerDict[key] = value;
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            InnerDict.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return InnerDict.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            InnerDict.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return InnerDict.Count; }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            // the string comparer won't be honored if we just try to remove kvp

            return InnerDict.Remove(item); 
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)InnerDict).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)InnerDict).GetEnumerator();
        }

        #endregion

        #region private methods

        protected virtual string GetRealName(string binderName)
        {
            return binderName;
        }

        #endregion

    }

}
