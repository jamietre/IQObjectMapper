using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Data;
using System.Dynamic;

using IQObjectMapper.Impl;

namespace IQObjectMapper.Adapters
{
    /// <summary>
    /// Expose the properties of an object as IDictionary-string,object-. Changes to the dictionary
    /// (if allowed) will affect the underlying bound object.
    /// </summary>
    public class DynamicAdapter : DynamicObject,
        IDictionary<string, object>, 
        INotifyPropertyChanged
    {
       
        public DynamicAdapter(object obj, IMapOptions options=null):
            base()
        {
            Options = MapOptions.From(options);
            ConfigureInnerDict(obj, Options);
            
        }

        protected PropertyDictionaryAdapter InnerDict;

        public MapOptions Options { get; set; }


        #region public methods

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { InnerDict.PropertyChanged += value; }
            remove { InnerDict.PropertyChanged -= value; }
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return InnerDict.Keys;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name;

            if (InnerDict.TryGetValue(name, out result))
            {
                return true;
            }
            else
            {
                if (Options.CanAccessMissingProperties)
                {
                    result = Options.UndefinedValue;
                }
                return Options.CanAccessMissingProperties;
            }
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {

            
            if (!InnerDict.ContainsKey(binder.Name))
            {
                if (Options.CanAlterProperties)
                {
                    InnerDict[binder.Name] = value;
                }
                return Options.CanAlterProperties;
            }
            InnerDict[binder.Name] = value;
            return true;
            
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

        #region private methods

        protected void ConfigureInnerDict(object obj, IReflectionOptions options)
        {
            InnerDict = new PropertyDictionaryAdapter(obj, options);
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
            InnerDict.Add(key,value);
        }

        public bool ContainsKey(string key)
        {
            return InnerDict.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return InnerDict.Keys ; }
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
            InnerDict.Add(item);
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
            return InnerDict.Remove(item);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string,object>>)InnerDict).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)InnerDict).GetEnumerator();
        }

        #endregion
    
    }


}
