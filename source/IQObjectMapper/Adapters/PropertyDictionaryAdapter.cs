using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Data;

using IQObjectMapper.Impl;

namespace IQObjectMapper.Adapters
{
    /// <summary>
    /// Expose the properties of an object as IDictionary-string,object-. Changes to the dictionary
    /// (if allowed) will affect the underlying bound object.
    /// </summary>
    public class PropertyDictionaryAdapter : 
        IDictionary<string, object>, 
        IEnumerable<KeyValuePair<string,object>>,
        INotifyPropertyChanged
    {

        #region constructor

        /// <summary>
        /// When UpdateSource is true, the underlying object will be updated to reflect dictionary changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="updateSource"></param>
        /// <param name="readOnly"></param>
        /// <param name="options"></param>
        public PropertyDictionaryAdapter(object obj, IMapOptions options=null)
        {
            Options = MapOptions.From(options);
            InnerObject = obj;
            InitializeDictionary();
        }
        #endregion

        #region private properties


        private object _InnerObject;

        protected object InnerObject
        {
            get
            {
                 
                return _InnerObject;
            }
            set
            {
                _InnerObject = value;

            }
        }

        /// <summary>
        /// The reflection info
        /// </summary>
        private IClassInfo _ClassInfo;
        protected IClassInfo ClassInfo 
        {
            get
            {
                if (_ClassInfo == null)
                {
                    _ClassInfo = ObjectMapper.MapperCache.GetClassInfo(InnerObject.GetType(), this.Options);
                }
                return _ClassInfo;
            }
        }

        /// <summary>
        /// Typed access to the reflected field data
        /// </summary>
        protected IDictionary<string, IDelegateInfo> _ClassInfoData;
        protected IDictionary<string, IDelegateInfo> ClassInfoData
        {
            get
            {
                if (_ClassInfoData == null)
                {
                    _ClassInfoData = (IDictionary<string, IDelegateInfo>)ClassInfo.Data;
                }
                return _ClassInfoData;
            }

        }
        /// <summary>
        /// Capture fields that are added by the user
        /// </summary>
        protected IDictionary<string, object> _UserData;
        protected IDictionary<string, object> UserData
        {
            get
            {
                if (_UserData == null)
                {
                    _UserData = new Dictionary<string, object>(
                        Options.CaseSensitive ? 
                            StringComparer.Ordinal : 
                            StringComparer.OrdinalIgnoreCase
                        );
                }
                return _UserData;
            }

        }
        protected bool _CanAlterProperties;

        #endregion

        #region public properties
        
        public MapOptions Options { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;    

        #endregion

        #region IDictionary members

        public void Add(string key, object value)
        {
            VerifyAlterProperties();
            if (UserData.ContainsKey(key))
            {
                throw new Exception("The key already exists.");
            }
            else
            {
                UserData[key] = value;
            }
        }

        public bool ContainsKey(string key)
        {
            return UserData.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get {
                return UserData.Keys;
            }
        }

        public bool Remove(string key)
        {
            VerifyAlterProperties();
            return UserData.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            if (UserData.TryGetValue(key, out value))
            {
                if (value == Undefined.Value)
                {
                    value = Get(key);
                }
                return true;
            } else {
                value=null;
                return false;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                IList<object> values = new List<object>();
                foreach (var key in UserData.Keys)
                {
                    values.Add(GetAny(key));
                }
                return values;
            }
        }

        public object this[string key]
        {
            get
            {
                return GetAny(key);

            }
            set
            {
                // TODO: this should support updating properties using the IDictionary interface
                SetAny(key,value);
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            VerifyAlterProperties();
            UserData.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return UserData.ContainsKey(item.Key) && 
                GetAny(item.Key)==item.Value;
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            int index=0;
            foreach (var key in UserData.Keys)
            {
                var value = GetAny(key);
                array[index + arrayIndex] = new KeyValuePair<string, object>(key, value);
            }
        }

        public int Count
        {
            get { return UserData.Count; }
        }

        /// <summary>
        /// When false, changes are not allowed to the underlying data
        /// </summary>
        public bool IsReadOnly
        {
            get; set;
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            VerifyAlterProperties();
            if (Contains(item))
            {
                return UserData.Remove(item.Key);
            }
            else
            {
                return false;
            }
        }
        //public override string ToString()
        //{
        //    string result = "";
        //    foreach (var kvp in this)
        //    {
        //        result += String.Format("[{0},{1}]", kvp.Key, kvp.Value) + "\n";

        //    };
        //    return result;
        //}
        IEnumerator IEnumerable.GetEnumerator()
        {
            return UserDataEnumerable().GetEnumerator();
        }
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return UserDataEnumerable().GetEnumerator();
        }


        #endregion

        #region private methods

        /// <summary>
        /// Enumerate the data from both teh
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<KeyValuePair<string, object>> UserDataEnumerable()
        {
            foreach (var key in UserData.Keys)
            {
                yield return new KeyValuePair<string, object>(key, GetAny(key));

            }
        }
        /// <summary>
        /// Set the underlying object value through it's delegate
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected void Set(string key, object value)
        {
            if (!Options.UpdateSource)
            {
                return;
            }
            var fldInfo = ClassInfoData[key];
            object finalVal;
            if (Options.ParseValues && 
                !fldInfo.Type.IsAssignableFrom(value.GetType())) {
                    finalVal = Types.Parse(value,fldInfo.Type);
            } else {
                finalVal = value;
            }
            fldInfo.SetValue(InnerObject, finalVal);
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(key);
                PropertyChanged(this, args);
            }
        }
        /// <summary>
        /// Set either the local value, or the underlying value if the key represents a property/field
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected void SetAny(string key, object value)
        {
            if (!Options.UpdateSource)
            {
                return;
            }
            object userValue;
            if (UserData.TryGetValue(key, out userValue))
            {
                if (userValue == Undefined.Value)
                {
                    VerifyReadOnly();
                    Set(key, value);
                    return;
                }
            }
            else
            {
                VerifyAlterProperties();
            }
            UserData[key] = value;
        }
        /// <summary>
        /// Get the underlying object value through it's delegate
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected object Get(string key) {
            return ClassInfoData[key].GetValue(InnerObject);
        }
        /// <summary>
        /// Get either the local value, or the underlying value if the key represents a prop/field
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected object GetAny(string key)
        {
            var value = UserData[key];
            if (value == Undefined.Value)
            {
                return Get(key);
            }
            else
            {
                return value;
            }

        }
           /// <summary>
        /// Populate the user dictionary with a stub
        /// </summary>
        protected void InitializeDictionary()
        {
            foreach (var fldInfo in ClassInfoData.Values)
            {
                UserData[fldInfo.Name] = Undefined.Value;
            }

        }

        private void VerifyReadOnly() {
             if (IsReadOnly) {
                 throw new Exception("This is a read-only dictionary.");
             }
        }
        private void VerifyAlterProperties()
        {
            if (!Options.CanAlterProperties)
            {
                throw new Exception("You cannot add or remove items from this dictionary.");
            }
        }
        #endregion


    }
}
